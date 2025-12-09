using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Infrastructure;
using reviewApi.Service.Repositories;

public class FlowPdfDocument : IDocument
{
    public List<FlowItem> Data { get; }

    public FlowPdfDocument(List<FlowItem> data)
    {
        Data = data;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata();

    public DocumentSettings GetSettings() => new DocumentSettings();

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Text("Evaluation Flow Report")
                .FontSize(20)
                .Bold();

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(col =>
                {
                    col.ConstantColumn(80);
                    col.RelativeColumn();
                    col.ConstantColumn(80);
                    col.RelativeColumn();
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Padding(5).Text("Code").Bold();
                    header.Cell().Padding(5).Text("Name").Bold();
                    header.Cell().Padding(5).Text("Status").Bold();
                    header.Cell().Padding(5).Text("Departments").Bold();
                });

                // Rows
                foreach (var item in Data)
                {
                    table.Cell().Padding(5).Text(item.Code);
                    table.Cell().Padding(5).Text(item.Name);
                    table.Cell().Padding(5).Text(item.Status);
                    table.Cell().Padding(5).Text(string.Join(", ", item.ApplicableDepartments));
                }
            });
        });
    }
}

namespace reviewApi.Service.Repositories
{
    public class FlowItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public List<string> ApplicableDepartments { get; set; }
    }

    public class DowloadService
    {
        public byte[] ExportPdf(List<FlowItem> data)
        {
            var doc = new FlowPdfDocument(data);
            return doc.GeneratePdf();
        }
        public byte[] ExportWord(List<FlowItem> data)
        {
            string templatePath = "template.docx"; // file bạn đã tải về
            byte[] templateBytes = File.ReadAllBytes(templatePath);

            using var ms = new MemoryStream();
            ms.Write(templateBytes, 0, templateBytes.Length);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(ms, true))
            {
                string fullText = null;
                using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    fullText = sr.ReadToEnd();
                }

                // Bạn truyền CHỈ 1 record hoặc record đầu tiên
                var item = data.First();

                fullText = fullText
                    .Replace("{{Code}}", item.Code)
                    .Replace("{{Name}}", item.Name)
                    .Replace("{{Status}}", item.Status)
                    .Replace("{{Departments}}", string.Join(", ", item.ApplicableDepartments));

                using (var sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(fullText);
                }
            }

            return ms.ToArray();
        }
        public byte[] ExportExcel(List<FlowItem> data)
        {
            ExcelPackage.License.SetNonCommercialPersonal("YourName");

            string template = "template.xlsx";
            byte[] fileBytes = File.ReadAllBytes(template);

            using var ms = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(ms);
            var ws = package.Workbook.Worksheets["Data"];

            int row = 2; // dòng sau header
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.Code;
                ws.Cells[row, 2].Value = item.Name;
                ws.Cells[row, 3].Value = item.Status;
                ws.Cells[row, 4].Value = string.Join(", ", item.ApplicableDepartments);
                row++;
            }

            return package.GetAsByteArray();
        }

    }
}
