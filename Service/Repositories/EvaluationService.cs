using Microsoft.AspNetCore.Http;
using reviewApi.DTO;
using reviewApi.Models;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QDoc = QuestPDF.Fluent.Document;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace reviewApi.Service.Repositories
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        public EvaluationService(
            IUnitOfWork iUnitOfWork
         )
        {
            _iUnitOfWork = iUnitOfWork;
        }

        private string NormalizeString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public async Task<responseDTO<List<EvaluationDTO>>> GetEvaluations(
                string tab,
                int page,
                int limit,
                string? search,
                string? status,
                string? period,
                int? year)
        {
            var usersDict = _iUnitOfWork.User.GetAll().ToDictionary(u => u.Id, u => u);
            var departmentsDict = _iUnitOfWork.Department.GetAll().ToDictionary(d => d.Code, d => d);

            var query = FilterEvaluations(tab, search, status, period, year, usersDict, departmentsDict);

            var draftTotalItems = tab == "self"
                ? query.Where(ev => ev.Status == "Dự thảo").Count()
                : 0;

            var evaluations = SortEvaluations(query);
            var skip = (page - 1) * limit;

            var users = usersDict;
            var departments = departmentsDict;
            var allCriteria = _iUnitOfWork.Criteria.GetAll().ToList();
            var allScores = _iUnitOfWork.EvaluationScore.GetAll().ToList();

            var data = evaluations
                .Skip(skip)
                .Take(limit)
                ?.Select(ev => new EvaluationDTO
                {
                    id = ev.Id,
                    fullName = users.ContainsKey(ev.UserId) ? users[ev.UserId].FullName : ev?.User?.FullName,
                    status = ev.Status,
                    evaluationPeriod = $"Tháng {ev.PeriodMonth}/{ev.PeriodYear}",
                    statusColor = ev.Status == "completed" ? "green" :
                                  ev.Status == "in_progress" ? "blue" :
                                  ev.Status == "not_started" ? "gray" : "red",
                    selfScore = CalculateScore(ev.Id, ev.CriteriaSetId, allCriteria, allScores, true),
                    managerScore = CalculateScore(ev.Id, ev.CriteriaSetId, allCriteria, allScores, false),
                    criteriaSetId = ev.CriteriaSetId,
                    department = users.ContainsKey(ev.UserId) && departments.ContainsKey(users[ev.UserId].DepartmentCode)
                        ? departments[users[ev.UserId].DepartmentCode].Name
                        : ev?.User?.Department?.Name,

                })
                .ToList();

            return new responseDTO<List<EvaluationDTO>>
            {
                message = "Lấy danh sách đánh giá thành công",
                data = data,
                pagination = new Pagination
                {
                    currentPage = page,
                    totalItems = evaluations.Count(),
                    itemsPerPage = limit,
                    totalPages = (int)Math.Ceiling((double)evaluations.Count() / limit),
                    draftTotalItems = draftTotalItems
                }
            };
        }

        private IEnumerable<Evaluation> FilterEvaluations(
            string tab,
            string? search,
            string? status,
            string? period,
            int? year,
            Dictionary<int, User> usersDict,
            Dictionary<string, Department> departmentsDict)
        {
            IEnumerable<Evaluation> query = _iUnitOfWork.Evaluation.GetAll();
            int userId = _iUnitOfWork.userId ?? 0;
            switch (tab)
            {
                case "self":
                    query = _iUnitOfWork.Evaluation.Find(ev => ev.UserId == userId).ToList();
                    break;

                case "manager":
                    query = _iUnitOfWork.Evaluation.Find(ev => ev.ManagerId == userId).ToList();
                    break;

                case "results":
                    query = _iUnitOfWork.Evaluation.Find(ev =>
                        ev.ManagerId == userId ||
                        ev.UserId == userId).ToList();
                    break;
            }

            var periodMonth = stringToInt(period ?? string.Empty);
            if (periodMonth > 0)
                query = query.Where(ev => ev.PeriodMonth == periodMonth);

            if (year != null)
                query = query.Where(ev => ev.PeriodYear == year);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = NormalizeString(search);
                query = query.Where(ev =>
                {
                    var fullName = usersDict.ContainsKey(ev.UserId) ? NormalizeString(usersDict[ev.UserId].FullName) : NormalizeString(ev.User?.FullName);
                    var departmentName = usersDict.ContainsKey(ev.UserId) && !string.IsNullOrEmpty(usersDict[ev.UserId].DepartmentCode)
                        ? NormalizeString(departmentsDict.ContainsKey(usersDict[ev.UserId].DepartmentCode) ? departmentsDict[usersDict[ev.UserId].DepartmentCode].Name : null)
                        : NormalizeString(ev.User?.Department?.Name);
                    var evaluationPeriod = NormalizeString($"Tháng {ev.PeriodMonth}/{ev.PeriodYear}");
                    return (!string.IsNullOrEmpty(fullName) && fullName.Contains(s)) ||
                           (!string.IsNullOrEmpty(departmentName) && departmentName.Contains(s)) ||
                           (!string.IsNullOrEmpty(evaluationPeriod) && evaluationPeriod.Contains(s));
                });
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (tab == "manager" && status == "Chờ đánh giá")
                {
                    // Manager tab cần thấy cả 2 trạng thái đang chờ
                    query = query.Where(ev => ev.Status == "Chờ đánh giá" || ev.Status == "Chờ đánh giá của thủ trưởng");
                }
                else
                {
                    query = query.Where(ev => ev.Status == status);
                }
            }

            return query;
        }

        private List<Evaluation> SortEvaluations(IEnumerable<Evaluation> query)
        {
            var statusOrder = new Dictionary<string, int>
            {
                { "Dự thảo", 0 },
                { "Chờ đánh giá", 1 },
                { "Chờ đánh giá của thủ trưởng", 2 },
                { "Hoàn thành", 3 }
            };

            return query
                .OrderBy(ev => statusOrder.ContainsKey(ev.Status ?? string.Empty) ? statusOrder[ev.Status ?? string.Empty] : 99)
                .ThenByDescending(ev => ev.PeriodYear)
                .ThenByDescending(ev => ev.PeriodMonth)
                .ToList();
        }

        private string? _exportTitle;
        private string? _exportDate;

        public async Task<byte[]> ExportEvaluations(string format, string tab, string? search, string? status, string? period, int? year)
        {
            SetExportMetadata(tab);
            var usersDict = _iUnitOfWork.User.GetAll().ToDictionary(u => u.Id, u => u);
            var departmentsDict = _iUnitOfWork.Department.GetAll().ToDictionary(d => d.Code, d => d);
            var query = FilterEvaluations(tab, search, status, period, year, usersDict, departmentsDict);
            var evaluations = SortEvaluations(query);

            var exportRows = evaluations.Select((ev, index) => new
            {
                Stt = index + 1,
                HoTen = usersDict.ContainsKey(ev.UserId) ? usersDict[ev.UserId].FullName : ev.User?.FullName,
                DonVi = usersDict.ContainsKey(ev.UserId) && departmentsDict.ContainsKey(usersDict[ev.UserId].DepartmentCode)
                    ? departmentsDict[usersDict[ev.UserId].DepartmentCode].Name
                    : ev.User?.Department?.Name,
                KyDanhGia = $"Tháng {ev.PeriodMonth}/{ev.PeriodYear}",
                DiemCaNhan = CalculateScore(ev.Id, ev.CriteriaSetId, _iUnitOfWork.Criteria.GetAll().ToList(), _iUnitOfWork.EvaluationScore.GetAll().ToList(), true),
                DiemDonVi = CalculateScore(ev.Id, ev.CriteriaSetId, _iUnitOfWork.Criteria.GetAll().ToList(), _iUnitOfWork.EvaluationScore.GetAll().ToList(), false),
                TrangThai = ev.Status
            }).ToList();

            var fmt = format?.ToLower();
            if (fmt == "xlsx") return BuildExcel(exportRows);
            if (fmt == "docx") return BuildDoc(exportRows, tab);
            if (fmt == "pdf") return BuildPdf(exportRows, tab);
            return BuildHtml(exportRows, tab);
        }

        private byte[] BuildExcel(IEnumerable<dynamic> rows)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Evaluations");
            var headers = new[] { "STT", "Họ tên", "Đơn vị", "Kỳ đánh giá", "Điểm cá nhân", "Điểm đơn vị", "Trạng thái" };

            // Title row
            ws.Cells["A1:G1"].Merge = true;
            ws.Cells["A1"].Value = _exportTitle ?? "DANH SÁCH ĐÁNH GIÁ";
            ws.Cells["A1"].Style.Font.Name = "Times New Roman";
            ws.Cells["A1"].Style.Font.Size = 15;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Date row
            ws.Cells["A2:G2"].Merge = true;
            ws.Cells["A2"].Value = _exportDate ?? string.Empty;
            ws.Cells["A2"].Style.Font.Name = "Times New Roman";
            ws.Cells["A2"].Style.Font.Size = 11;
            ws.Cells["A2"].Style.Font.Italic = true;
            ws.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            // Header row starts at row 3
            int headerRow = 3;
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[headerRow, i + 1].Value = headers[i];
                ws.Cells[headerRow, i + 1].Style.Font.Bold = true;
                ws.Cells[headerRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f5f5f5"));
                ws.Cells[headerRow, i + 1].Style.Font.Color.SetColor(System.Drawing.ColorTranslator.FromHtml("#272727"));
                ws.Cells[headerRow, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#d1d5db"));
                ws.Cells[headerRow, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                ws.Cells[headerRow, i + 1].Style.Font.Name = "Times New Roman";
                ws.Cells[headerRow, i + 1].Style.Font.Size = 13;
            }

            int rowIndex = headerRow + 1;
            foreach (var r in rows)
            {
                ws.Cells[rowIndex, 1].Value = r.Stt;
                ws.Cells[rowIndex, 2].Value = r.HoTen;
                ws.Cells[rowIndex, 3].Value = r.DonVi;
                ws.Cells[rowIndex, 4].Value = r.KyDanhGia;
                ws.Cells[rowIndex, 5].Value = r.DiemCaNhan;
                ws.Cells[rowIndex, 6].Value = r.DiemDonVi;
                ws.Cells[rowIndex, 7].Value = r.TrangThai;
                for (int col = 1; col <= headers.Length; col++)
                {
                    ws.Cells[rowIndex, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#d1d5db"));
                    ws.Cells[rowIndex, col].Style.Font.Name = "Times New Roman";
                    ws.Cells[rowIndex, col].Style.Font.Size = 13;
                    ws.Cells[rowIndex, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }
                rowIndex++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        private byte[] BuildDoc(IEnumerable<dynamic> rows, string tab)
        {
            using var ms = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new W.Document();
                var body = new W.Body();

                var titleText = _exportTitle ?? "DANH SÁCH ĐÁNH GIÁ";
                var titleRun = new W.Run(new W.Text(titleText))
                {
                    RunProperties = new W.RunProperties(
                        new W.RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                        new W.FontSize() { Val = "30" },
                        new W.Bold())
                };
                var titleParagraph = new W.Paragraph(titleRun)
                {
                    ParagraphProperties = new W.ParagraphProperties(
                        new W.Justification() { Val = W.JustificationValues.Center },
                        new W.SpacingBetweenLines() { After = "120" })
                };
                body.Append(titleParagraph);

                var dateText = _exportDate ?? string.Empty;
                var dateRun = new W.Run(new W.Text(dateText))
                {
                    RunProperties = new W.RunProperties(
                        new W.RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                        new W.FontSize() { Val = "22" },
                        new W.Italic())
                };
                var dateParagraph = new W.Paragraph(dateRun)
                {
                    ParagraphProperties = new W.ParagraphProperties(
                        new W.Justification() { Val = W.JustificationValues.Right },
                        new W.SpacingBetweenLines() { After = "200" })
                };
                body.Append(dateParagraph);

                var table = new W.Table();
                var props = new W.TableProperties(
                    new W.TableBorders(
                        new W.TopBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" },
                        new W.LeftBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" },
                        new W.BottomBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" },
                        new W.RightBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" },
                        new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" },
                        new W.InsideVerticalBorder { Val = W.BorderValues.Single, Size = 6, Color = "d1d5db" }
                    ));
                table.AppendChild(props);

                string[] headers = { "STT", "Họ tên", "Đơn vị", "Kỳ đánh giá", "Điểm cá nhân", "Điểm đơn vị", "Trạng thái" };
                var headerRow = new W.TableRow();
                foreach (var h in headers)
                {
                    var cell = new W.TableCell(
                        new W.Paragraph(
                            new W.Run(
                                new W.Text(h)
                            )
                            {
                                RunProperties = new W.RunProperties(
                                    new W.RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                                    new W.FontSize() { Val = "26" },
                                    new W.Bold(),
                                    new W.Color() { Val = "272727" })
                            })
                        {
                            ParagraphProperties = new W.ParagraphProperties(new W.SpacingBetweenLines() { After = "40" })
                        });
                    cell.TableCellProperties = new W.TableCellProperties(
                        new W.TableCellBorders(
                            new W.TopBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                            new W.BottomBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                            new W.LeftBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                            new W.RightBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" }
                        ),
                        new W.TableCellMargin(
                            new W.TopMargin() { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                            new W.BottomMargin() { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                            new W.LeftMargin() { Width = "120", Type = W.TableWidthUnitValues.Dxa },
                            new W.RightMargin() { Width = "120", Type = W.TableWidthUnitValues.Dxa }
                        ),
                        new W.Shading() { Val = W.ShadingPatternValues.Clear, Color = "auto", Fill = "f5f5f5" }
                    );
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var r in rows)
                {
                    var row = new W.TableRow();
                    var vals = new[] { r.Stt.ToString(), r.HoTen, r.DonVi, r.KyDanhGia, r.DiemCaNhan.ToString(), r.DiemDonVi.ToString(), r.TrangThai };
                    foreach (var v in vals)
                    {
                        var cell = new W.TableCell(
                            new W.Paragraph(
                                new W.Run(
                                    new W.Text(v ?? string.Empty)
                                )
                                {
                                    RunProperties = new W.RunProperties(
                                        new W.RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                                        new W.FontSize() { Val = "26" })
                                })
                            {
                                ParagraphProperties = new W.ParagraphProperties(new W.SpacingBetweenLines() { After = "20" })
                            });
                        cell.TableCellProperties = new W.TableCellProperties(
                            new W.TableCellBorders(
                                new W.TopBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                                new W.BottomBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                                new W.LeftBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" },
                                new W.RightBorder { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" }
                            ),
                            new W.TableCellMargin(
                                new W.TopMargin() { Width = "60", Type = W.TableWidthUnitValues.Dxa },
                                new W.BottomMargin() { Width = "60", Type = W.TableWidthUnitValues.Dxa },
                                new W.LeftMargin() { Width = "120", Type = W.TableWidthUnitValues.Dxa },
                                new W.RightMargin() { Width = "120", Type = W.TableWidthUnitValues.Dxa }
                            )
                        );
                        row.Append(cell);
                    }
                    table.Append(row);
                }

                body.Append(table);
                var footerPart = doc.MainDocumentPart.AddNewPart<FooterPart>();
                var footerParagraph = new W.Paragraph(
                    new W.ParagraphProperties(
                        new W.Justification() { Val = W.JustificationValues.Right },
                        new W.SpacingBetweenLines() { After = "0", Before = "0" },
                        new W.ParagraphBorders(
                            new W.TopBorder() { Val = W.BorderValues.Single, Size = 4, Color = "d1d5db" }
                        )
                    ),
                    new W.Run(new W.FieldChar() { FieldCharType = W.FieldCharValues.Begin }),
                    new W.Run(new W.FieldCode(" PAGE ")),
                    new W.Run(new W.FieldChar() { FieldCharType = W.FieldCharValues.End })
                );

                var footer = new W.Footer(footerParagraph);
                footerPart.Footer = footer;

                var sectionProps = new W.SectionProperties();
                var footerRef = new W.FooterReference() { Type = W.HeaderFooterValues.Default, Id = doc.MainDocumentPart.GetIdOfPart(footerPart) };
                sectionProps.Append(footerRef);
                body.Append(sectionProps);

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
            return ms.ToArray();
        }

        private byte[] BuildHtml(IEnumerable<dynamic> rows, string tab)
        {
            var html = BuildHtmlString(rows, tab);
            return Encoding.UTF8.GetBytes(html);
        }

        private byte[] BuildPdf(IEnumerable<dynamic> rows, string tab)
        {
            SetExportMetadata(tab);
            QuestPDF.Settings.License = LicenseType.Community;

            var title = _exportTitle ?? "DANH SÁCH ĐÁNH GIÁ";
            var dateText = _exportDate ?? string.Empty;

            var dataRows = rows.ToList();

            var doc = QDoc.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Header().Column(col =>
                    {
                        col.Item()
                            .PaddingTop(8)
                            .PaddingBottom(8)
                            .Text(title)
                            .FontFamily("Times New Roman")
                            .FontSize(15)
                            .Bold()
                            .AlignCenter();
                        col.Item()
                            .PaddingTop(8)
                            .PaddingBottom(8)
                            .AlignRight().Text(dateText)
                            .FontFamily("Times New Roman")
                            .FontSize(11)
                            .Italic();
                    });

                    page.Content().Table(table =>
                    {
                        string[] headers = { "STT", "Họ tên", "Đơn vị", "Kỳ đánh giá", "Điểm cá nhân", "Điểm đơn vị", "Trạng thái" };
                        var columns = headers.Length;
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Background("#f5f5f5").Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text(h)
                                    .FontFamily("Times New Roman").FontSize(13).Bold().FontColor("#272727").AlignCenter();
                            }
                        });

                        foreach (var r in dataRows)
                        {
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.Stt}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.HoTen}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.DonVi}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.KyDanhGia}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.DiemCaNhan}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.DiemDonVi}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).Padding(2).BorderColor("#d1d5db").Padding(4).Text($"{r.TrangThai}").FontFamily("Times New Roman").FontSize(13);
                        }
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(t => t.FontFamily("Times New Roman").FontSize(11));
                        text.Span("Trang ");
                        text.CurrentPageNumber();
                    });
                });
            });

            return doc.GeneratePdf();
        }

        private string BuildHtmlString(IEnumerable<dynamic> rows, string tab)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><style>");
            sb.Append("body{font-family:'Times New Roman',serif;font-size:13px;color:#272727;}");
            sb.Append("table{border-collapse:collapse;width:100%;font-family:'Times New Roman',serif;font-size:13px;}");
            sb.Append("th,td{border:1px solid #d1d5db;padding:8px;text-align:left;}");
            sb.Append("th{background:#f5f5f5;font-weight:700;color:#272727;}");
            sb.Append("</style></head><body>");
            var titleText = _exportTitle ?? "DANH SÁCH ĐÁNH GIÁ";
            sb.Append($"<h3 style='text-align:center;margin:8px 0;font-size:15px;font-weight:bold;'>{titleText}</h3>");
            if (!string.IsNullOrEmpty(_exportDate))
            {
                sb.Append($"<div style='text-align:right;font-size:11px;font-style:italic;margin:8px 0;'>{_exportDate}</div>");
            }
            sb.Append("<table><thead><tr>");
            var headers = new[] { "STT", "Họ tên", "Đơn vị", "Kỳ đánh giá", "Điểm cá nhân", "Điểm đơn vị", "Trạng thái" };
            foreach (var h in headers) sb.Append($"<th>{h}</th>");
            sb.Append("</tr></thead><tbody>");
            foreach (var r in rows)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{r.Stt}</td>");
                sb.Append($"<td>{r.HoTen}</td>");
                sb.Append($"<td>{r.DonVi}</td>");
                sb.Append($"<td>{r.KyDanhGia}</td>");
                sb.Append($"<td>{r.DiemCaNhan}</td>");
                sb.Append($"<td>{r.DiemDonVi}</td>");
                sb.Append($"<td>{r.TrangThai}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></body></html>");
            return sb.ToString();
        }

        private void SetExportMetadata(string tab)
        {
            _exportTitle = tab switch
            {
                "manager" => "DANH SÁCH ĐÁNH GIÁ",
                "results" => "DANH SÁCH KẾT QUẢ ĐÁNH GIÁ",
                _ => "DANH SÁCH TỰ ĐÁNH GIÁ"
            };
            var dt = DateTime.Now;
            _exportDate = $"Ngày {dt:dd} tháng {dt:MM} năm {dt:yyyy}";
        }
        public async Task<TemplateDTO> GetTemplates(int? id)
        {
            if (id == null) return await GetTemplates();

            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            EnsureEvaluationAccess(ev);

            _iUnitOfWork.User.GetAll();
            _iUnitOfWork.Department.GetAll();
            var criteriaAll = _iUnitOfWork.Criteria.Find(c => c.CriteriaSetId == ev.CriteriaSetId).ToList();
            var rootCriteriaIds = criteriaAll.Where(c => c.parentId == null).Select(c => c.Id).ToList();

            var scores = _iUnitOfWork.EvaluationScore.Find(e => e.EvaluationId == id).ToList();
            var selfScores = scores.ToDictionary(s => s.CriteriaId, s => s.SelfScore ?? 0);
            var managerScores = scores.ToDictionary(s => s.CriteriaId, s => s.ManagerScore ?? 0);
            if (ev.Status == "Chờ đánh giá")
            {
                foreach (var kv in selfScores)
                {
                    managerScores[kv.Key] = kv.Value;
                }
            }

            return new TemplateDTO
            {
                Id = ev.Id,
                FullName = ev?.User?.FullName,
                Department = ev?.User?.Department?.Name,
                SelfScore = scores.Where(s => rootCriteriaIds.Contains(s.CriteriaId)).Sum(e => e.SelfScore ?? 0),
                ManagerScore = scores.Where(s => rootCriteriaIds.Contains(s.CriteriaId)).Sum(e => e.ManagerScore ?? 0),
                ManagerScores = managerScores,
                CriteriaSetId = ev.CriteriaSetId,
                EvaluationPeriod = $"Tháng {ev.PeriodMonth}/{ev.PeriodYear}",
                Scores = selfScores,
                Status = ev.Status,
                period = $"Tháng {ev.PeriodMonth}",
                year = ev.PeriodYear
            };
        }
        private async Task<TemplateDTO> GetTemplates()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            var activeObjectCodes = _iUnitOfWork.EvaluationObject
                .Find(o => o.Status != null && o.Status.ToLower() == "active")
                .Select(o => o.Code)
                .ToList();

            var ids1 = _iUnitOfWork.CriteriaSetPeriod.Find(e => e.month == month && e.year == year).Select(e => e.CriteriaSetId).ToList();
            var ids2 = _iUnitOfWork.EvaluationObjectRole.Find(e => e.UserId == _iUnitOfWork.userId)
                .Select(e => e.EvaluationObjectCode)
                .Where(code => activeObjectCodes.Contains(code))
                .ToList();
            var ids3 = _iUnitOfWork.CriteriaSetObject.Find(e => ids2.Contains(e.EvaluationObjectCode)).Select(e => e.CriteriaSetId).ToList();

            return new TemplateDTO
            {
                Id = 0,
                FullName = _iUnitOfWork.name,
                Department = _iUnitOfWork.Department.GetById(_iUnitOfWork.department).Name,
                SelfScore = 100,
                ManagerScore = 100,
                ManagerScores = null,
                CriteriaSetId = ids1.Intersect(ids3).ToList().First(),
                EvaluationPeriod = $"Tháng {month}/{year}",
                Scores = null,
                Status = null
            };
        }
        public async Task CreatEvaluation(TemplateDTO payload)
        {
            var userId = _iUnitOfWork.userId ?? 0;
            var month = stringToInt(payload.period);
            var year = payload.year ?? 0;
            EnsureNoDuplicateEvaluation(userId, month, year, null);
            var ev = new Evaluation
            {
                CriteriaSetId = payload.CriteriaSetId??0,
                UserId = userId,
                PeriodMonth = month,
                PeriodYear = year,
                Status = "Dự thảo"
            };
            var score = payload.Scores.Select(e => new EvaluationScore
            {
                CriteriaId = e.Key,
                SelfScore = e.Value,
                Evaluation = ev
            }).ToList();
            ev.EvaluationScores = score;
            _iUnitOfWork.Evaluation.Add(ev);
            _iUnitOfWork.Complete();
        }

        public async Task<EvaluationDTO> CreateAndSubmitEvaluation(CreateSubmitEvaluationDTO payload, IFormFileCollection? files)
        {
            var userId = _iUnitOfWork.userId ?? 0;
            var month = stringToInt(payload.period);
            var year = int.TryParse(payload.year, out var y) ? y : 0;
            EnsureNoDuplicateEvaluation(userId, month, year, null);
            var ev = new Evaluation
            {
                CriteriaSetId = payload.criteriaSetId,
                UserId = userId,
                ManagerId = payload.managerId,
                PeriodMonth = month,
                PeriodYear = year,
                Status = "Chờ đánh giá"
            };

            var scoresDict = payload.scores;
            if ((scoresDict == null || !scoresDict.Any()) && !string.IsNullOrWhiteSpace(payload.scoresJson))
            {
                try
                {
                    scoresDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(payload.scoresJson);
                }
                catch { }
            }

            var score = scoresDict?.Select(e => new EvaluationScore
            {
                CriteriaId = e.Key,
                SelfScore = e.Value,
                Evaluation = ev
            }).ToList() ?? new List<EvaluationScore>();
            ev.EvaluationScores = score;
            _iUnitOfWork.Evaluation.Add(ev);
            _iUnitOfWork.Complete();

            // Lưu comment (nếu có) dưới dạng chat gắn với evaluation
            if (!string.IsNullOrWhiteSpace(payload.comments) || (files != null && files.Count > 0))
            {
                var chat = new EvaluationChat
                {
                    EvaluationId = ev.Id,
                    Message = payload.comments ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    SenderId = _iUnitOfWork.userId ?? 0,
                    TypeChat = "send-evaluation"
                };
                _iUnitOfWork.EvaluationChat.Add(chat);
                _iUnitOfWork.Complete();
                if (files != null && files.Count > 0)
                {
                    var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                    Directory.CreateDirectory(uploadRoot);
                    var chatFiles = new List<EvaluationChatFile>();
                    foreach (var file in files)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var savePath = Path.Combine(uploadRoot, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var relativePath = $"/uploads/chat/{fileName}";
                        chatFiles.Add(new EvaluationChatFile
                        {
                            EvaluationChatId = chat.Id,
                            FileName = file.FileName,
                            FilePath = relativePath
                        });
                    }
                    _iUnitOfWork.EvaluationChatFile.AddRange(chatFiles);
                    _iUnitOfWork.Complete();
                }
            }
            return await BuildEvaluationDTO(ev.Id);
        }

        public async Task<EvaluationDTO> UpdateEvaluation(int id, Dictionary<int, int>? scores, string? evaluationPeriod, Dictionary<int, int>? managerScores)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            EnsureEvaluationAccess(ev);
            var currentUserId = _iUnitOfWork.userId ?? 0;
            var isOwner = ev.UserId == currentUserId;
            var isManager = ev.ManagerId == currentUserId;

            if (scores != null && !isOwner) throw new Exception("Không có quyền cập nhật điểm tự đánh giá.");
            if (managerScores != null && !isManager) throw new Exception("Không có quyền cập nhật điểm quản lý.");

            // Cập nhật kỳ đánh giá nếu truyền vào
            if (!string.IsNullOrWhiteSpace(evaluationPeriod))
            {
                if (!isOwner) throw new Exception("Không có quyền cập nhật kỳ đánh giá.");
                var parts = evaluationPeriod.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var month = stringToInt(parts[0].Trim());
                    var year = int.TryParse(parts[1].Trim(), out var y) ? y : 0;
                    if (month == 0 || year == 0) throw new Exception("Kỳ đánh giá không hợp lệ");
                    EnsureNoDuplicateEvaluation(ev.UserId, month, year, id);
                    ev.PeriodMonth = month;
                    ev.PeriodYear = year;
                }
            }
            var existingScores = _iUnitOfWork.EvaluationScore.Find(s => s.EvaluationId == id).ToList();
            var merged = existingScores.ToDictionary(s => s.CriteriaId, s => s);

            if (scores != null)
            {
                foreach (var kv in scores)
                {
                    if (!merged.ContainsKey(kv.Key))
                    {
                        merged[kv.Key] = new EvaluationScore
                        {
                            CriteriaId = kv.Key,
                            EvaluationId = id
                        };
                    }
                    merged[kv.Key].SelfScore = kv.Value;
                }
            }

            if (managerScores != null)
            {
                foreach (var kv in managerScores)
                {
                    if (!merged.ContainsKey(kv.Key))
                    {
                        merged[kv.Key] = new EvaluationScore
                        {
                            CriteriaId = kv.Key,
                            EvaluationId = id
                        };
                    }
                    merged[kv.Key].ManagerScore = kv.Value;
                }
            }

            // Update existing scores, add new ones to avoid PK conflicts
            var newScores = merged.Values.Where(s => s.Id == 0).ToList();
            var updatedScores = merged.Values.Where(s => s.Id != 0).ToList();

            if (updatedScores.Any())
            {
                foreach (var sc in updatedScores)
                {
                    _iUnitOfWork.EvaluationScore.Update(sc);
                }
            }
            if (newScores.Any())
            {
                _iUnitOfWork.EvaluationScore.AddRange(newScores);
            }
            _iUnitOfWork.Complete();
            return await BuildEvaluationDTO(id);
        }

        public async Task<EvaluationDTO> SubmitEvaluation(int id, SubmitEvaluationDTO payload, IFormFileCollection? files)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            ev.ManagerId = payload.managerId;
            ev.Status = "Chờ đánh giá";
            _iUnitOfWork.Evaluation.Update(ev);
            _iUnitOfWork.Complete();
            // lưu comment/files nếu có
            if (!string.IsNullOrWhiteSpace(payload.comments) || (files != null && files.Count > 0))
            {
                var chat = new EvaluationChat
                {
                    EvaluationId = id,
                    Message = payload.comments ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    SenderId = _iUnitOfWork.userId ?? 0,
                    TypeChat = "send-evaluation"
                };
                _iUnitOfWork.EvaluationChat.Add(chat);
                _iUnitOfWork.Complete();

                if (files != null && files.Count > 0)
                {
                    var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                    Directory.CreateDirectory(uploadRoot);
                    var chatFiles = new List<EvaluationChatFile>();
                    foreach (var file in files)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var savePath = Path.Combine(uploadRoot, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var relativePath = $"/uploads/chat/{fileName}";
                        chatFiles.Add(new EvaluationChatFile
                        {
                            EvaluationChatId = chat.Id,
                            FileName = file.FileName,
                            FilePath = relativePath
                        });
                    }
                    _iUnitOfWork.EvaluationChatFile.AddRange(chatFiles);
                    _iUnitOfWork.Complete();
                }
            }
            return await BuildEvaluationDTO(id);
        }

        public async Task<EvaluationDTO> WithdrawEvaluation(int id, string? reason, IFormFileCollection? files)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            ev.Status = "Dự thảo";
            _iUnitOfWork.Evaluation.Update(ev);
            _iUnitOfWork.Complete();
            // log chat for withdraw
            await LogActionChat(id, reason ?? "Thu hồi đánh giá", "withdraw-evaluation", files);
            return await BuildEvaluationDTO(id);
        }

        public async Task<EvaluationDTO> ReturnEvaluation(int id, string? reason, IFormFileCollection? files)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            ev.Status = "Dự thảo";
            ev.ManagerId = null;
            _iUnitOfWork.Evaluation.Update(ev);
            _iUnitOfWork.Complete();
            await LogActionChat(id, reason ?? "Trả lại đánh giá", "return-evaluation", files);
            return await BuildEvaluationDTO(id);
        }

        public async Task<EvaluationDTO> ForwardEvaluation(int id)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            ev.Status = "Chờ đánh giá của thủ trưởng";
            _iUnitOfWork.Evaluation.Update(ev);
            _iUnitOfWork.Complete();
            return await BuildEvaluationDTO(id);
        }

        public async Task<EvaluationDTO> CompleteEvaluation(int id)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(id);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            ev.Status = "Hoàn thành";
            _iUnitOfWork.Evaluation.Update(ev);
            _iUnitOfWork.Complete();
            return await BuildEvaluationDTO(id);
        }

        public async Task deleteEvaluations(DeleteMultiple<int> payload)
        {
            if (payload == null) throw new Exception("Payload không hợp lệ");
            var ids = payload.ids ?? new List<int>();
            if ((payload.all == null || payload.all == false) && !ids.Any()) return;

            IEnumerable<int> targetIds;
            if (payload.all == true)
            {
                var userId = _iUnitOfWork.userId ?? 0;
                var periodMonth = stringToInt(payload.filters?.period ?? string.Empty);
                var year = payload.filters?.year;
                var status = payload.filters?.status;
                var search = payload.filters?.search?.ToLower();

                var query = _iUnitOfWork.Evaluation.Find(e => e.UserId == userId);
                if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);
                if (periodMonth > 0) query = query.Where(e => e.PeriodMonth == periodMonth);
                if (year != null) query = query.Where(e => e.PeriodYear == year);
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(e =>
                        (!string.IsNullOrEmpty(e.User?.FullName) && e.User.FullName.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(e.User?.Department?.Name) && e.User.Department.Name.ToLower().Contains(search)));
                }
                targetIds = query.Select(e => e.Id).ToList();
                if (payload.exclude != null && payload.exclude.Any())
                {
                    targetIds = targetIds.Except(payload.exclude).ToList();
                }
            }
            else
            {
                targetIds = ids;
            }

            var evalIds = targetIds.ToList();
            if (!evalIds.Any()) return;

            var scores = _iUnitOfWork.EvaluationScore.Find(s => evalIds.Contains(s.EvaluationId)).ToList();
            if (scores.Any()) _iUnitOfWork.EvaluationScore.RemoveRange(scores);

            var chats = _iUnitOfWork.EvaluationChat.Find(c => evalIds.Contains(c.EvaluationId)).ToList();
            if (chats.Any())
            {
                var chatIds = chats.Select(c => c.Id).ToList();
                var chatFiles = _iUnitOfWork.EvaluationChatFile.Find(f => chatIds.Contains(f.EvaluationChatId)).ToList();
                if (chatFiles.Any()) _iUnitOfWork.EvaluationChatFile.RemoveRange(chatFiles);
                _iUnitOfWork.EvaluationChat.RemoveRange(chats);
            }

            var evaluations = _iUnitOfWork.Evaluation.GetByIds(evalIds);
            _iUnitOfWork.Evaluation.RemoveRange(evaluations);
            _iUnitOfWork.Complete();
        }

        private int stringToInt(string period)
        {
            for (int i = 1; i < 13; i++)
            {
                if (period == $"Tháng {i}") return i;
            }
            return 0;
        }

        private void EnsureNoDuplicateEvaluation(int userId, int month, int year, int? excludeId)
        {
            var exists = _iUnitOfWork.Evaluation.Find(e =>
                e.UserId == userId &&
                e.PeriodMonth == month &&
                e.PeriodYear == year &&
                (excludeId == null || e.Id != excludeId)).Any();
            if (exists)
            {
                throw new Exception("Bạn đã có đánh giá trong kỳ này, không thể tạo thêm.");
            }
        }

        private int CalculateScore(int evaluationId, int criteriaSetId, List<Criteria> allCriteria, List<EvaluationScore> allScores, bool isSelf)
        {
            var criteria = allCriteria.Where(c => c.CriteriaSetId == criteriaSetId).ToList();
            if (!criteria.Any()) return 0;

            var scoreDict = allScores
                .Where(s => s.EvaluationId == evaluationId)
                .ToDictionary(s => s.CriteriaId, s => isSelf ? (s.SelfScore ?? 0) : (s.ManagerScore ?? 0));

            // Lưu ý: Dictionary không chấp nhận key null, dùng -1 để đại diện cho root
            var childrenMap = criteria
                .GroupBy(c => c.parentId ?? -1)
                .ToDictionary(g => g.Key, g => g.ToList());

            int GetSubtreeScore(Criteria node)
            {
                var ownScore = scoreDict.ContainsKey(node.Id) ? scoreDict[node.Id] : 0;
                var children = childrenMap.ContainsKey(node.Id) ? childrenMap[node.Id] : new List<Criteria>();
                var childrenScore = children.Sum(child => GetSubtreeScore(child));
                var total = ownScore + childrenScore;
                return (node.TypeScore ?? string.Empty).ToLower() == "subtractive" ? -total : total;
            }

            var roots = criteria.Where(c => c.parentId == null).ToList();
            return roots.Sum(r => GetSubtreeScore(r));
        }

        private void EnsureEvaluationAccess(Evaluation ev)
        {
            var currentUserId = _iUnitOfWork.userId ?? 0;
            if (ev.UserId != currentUserId && ev.ManagerId != currentUserId)
            {
                throw new Exception("Không có quyền truy cập dữ liệu.");
            }
        }

        private async Task<EvaluationDTO> BuildEvaluationDTO(int evaluationId)
        {
            var ev = _iUnitOfWork.Evaluation.GetById(evaluationId);
            if (ev == null) throw new Exception("Đánh giá không tồn tại");
            _iUnitOfWork.User.GetAll();
            _iUnitOfWork.Department.GetAll();
            _iUnitOfWork.EvaluationScore.GetAll();
            _iUnitOfWork.Criteria.GetAll();

            var criteriaIds = _iUnitOfWork.Criteria.Find(c => c.CriteriaSetId == ev.CriteriaSetId).Select(c => c.Id).ToList();
            var scores = _iUnitOfWork.EvaluationScore.Find(s => s.EvaluationId == evaluationId).ToList();
            var selfScores = scores.ToDictionary(s => s.CriteriaId, s => s.SelfScore ?? 0);
            var managerScores = scores.ToDictionary(s => s.CriteriaId, s => s.ManagerScore ?? 0);

            // Nếu vẫn ở trạng thái chờ đánh giá, mặc định điểm quản lý = điểm tự chấm để hiển thị
            if (ev.Status == "Chờ đánh giá")
            {
                foreach (var kv in selfScores)
                {
                    managerScores[kv.Key] = kv.Value;
                }
            }

            var managerScoreSum = criteriaIds.Sum(id => managerScores.ContainsKey(id) ? managerScores[id] : 0);
            var selfScoreSum = criteriaIds.Sum(id => selfScores.ContainsKey(id) ? selfScores[id] : 0);

            return new EvaluationDTO
            {
                id = ev.Id,
                criteriaSetId = ev.CriteriaSetId,
                fullName = ev.User?.FullName,
                evaluationPeriod = $"Tháng {ev.PeriodMonth}/{ev.PeriodYear}",
                department = ev.User?.Department?.Name,
                selfScore = selfScoreSum,
                managerScore = managerScoreSum,
                status = ev.Status,
                statusColor = MapStatusColor(ev.Status),
                Scores = selfScores,
                ManagerScores = managerScores
            };
        }

        private string MapStatusColor(string status)
        {
            return status switch
            {
                "Dự thảo" => "gray",
                "Chờ đánh giá" => "yellow",
                "Chờ đánh giá của thủ trưởng" => "orange",
                "Hoàn thành" => "green",
                _ => "gray"
            };
        }

        public Tuple<string, string>? GetChatFileById(int id)
        {
            var file = _iUnitOfWork.EvaluationChatFile.GetById(id);
            if (file == null) return null;
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relative = file.FilePath?.TrimStart('/') ?? string.Empty;
            var fullPath = Path.Combine(rootPath, relative);
            return Tuple.Create(fullPath, file.FileName);
        }

        public async Task<List<ChatMessageDTO>> GetChatMessages(int evaluationId)
        {
            var evaluation = _iUnitOfWork.Evaluation.GetById(evaluationId);
            if (evaluation == null) return new List<ChatMessageDTO>();
            EnsureEvaluationAccess(evaluation);

            var chats = _iUnitOfWork.EvaluationChat.Find(c => c.EvaluationId == evaluationId)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var chatIds = chats.Select(c => c.Id).ToList();
            var users = _iUnitOfWork.User.GetAll().ToDictionary(u => u.Id, u => u);
            var files = _iUnitOfWork.EvaluationChatFile
                .Find(f => chatIds.Contains(f.EvaluationChatId))
                .GroupBy(f => f.EvaluationChatId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var messageMap = chats.ToDictionary(c => c.Id, c => c);

            var result = chats.Select(c => new ChatMessageDTO
            {
                id = c.Id,
                sender = users.ContainsKey(c.SenderId) ? users[c.SenderId].FullName : "Người dùng",
                avatar = null,
                message = c.Message,
                timestamp = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                typeChat = c.TypeChat,
                attachments = files.ContainsKey(c.Id)
                    ? files[c.Id].Select(f => new ChatAttachmentDTO
                    {
                        id = f.Id,
                        name = f.FileName
                    }).ToList()
                    : new List<ChatAttachmentDTO>(),
                replyToMessageId = c.ReplyToChatId,
                replyToMessagePreview = c.ReplyToChatId.HasValue && messageMap.ContainsKey(c.ReplyToChatId.Value)
                    ? messageMap[c.ReplyToChatId.Value].Message
                    : null
            }).ToList();

            return result;
        }

        public async Task<ChatMessageDTO> SendChatMessage(int evaluationId, string? message, int? replyToMessageId, IFormFileCollection? files)
        {
            var evaluation = _iUnitOfWork.Evaluation.GetById(evaluationId);
            if (evaluation == null) throw new Exception("Đánh giá không tồn tại");

            var senderId = _iUnitOfWork.userId ?? 0;

            if ((files == null || files.Count == 0) && string.IsNullOrWhiteSpace(message))
            {
                throw new Exception("Vui lòng nhập nội dung hoặc đính kèm tệp tin.");
            }

            var chat = new EvaluationChat
            {
                EvaluationId = evaluationId,
                Message = message ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                ReplyToChatId = replyToMessageId,
                SenderId = senderId,
                TypeChat = null
            };

            _iUnitOfWork.EvaluationChat.Add(chat);
            _iUnitOfWork.Complete();

            if (files != null && files.Count > 0)
            {
                var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                Directory.CreateDirectory(uploadRoot);

                var chatFiles = new List<EvaluationChatFile>();
                foreach (var file in files)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var savePath = Path.Combine(uploadRoot, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var relativePath = $"/uploads/chat/{fileName}";
                    chatFiles.Add(new EvaluationChatFile
                    {
                        EvaluationChatId = chat.Id,
                        FileName = file.FileName,
                        FilePath = relativePath
                    });
                }
                _iUnitOfWork.EvaluationChatFile.AddRange(chatFiles);
                _iUnitOfWork.Complete();
            }

            var user = _iUnitOfWork.User.GetById(senderId);
            var attachments = _iUnitOfWork.EvaluationChatFile.Find(f => f.EvaluationChatId == chat.Id)
                ?.Select(f => new ChatAttachmentDTO { id = f.Id, name = f.FileName })
                .ToList();

            return new ChatMessageDTO
            {
                id = chat.Id,
                sender = user?.FullName ?? "Người dùng",
                avatar = null,
                message = chat.Message,
                timestamp = chat.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                attachments = attachments,
                replyToMessageId = chat.ReplyToChatId,
                replyToMessagePreview = null
            };
        }

        private async Task LogActionChat(int evaluationId, string message, string typeChat, IFormFileCollection? files)
        {
            var chat = new EvaluationChat
            {
                EvaluationId = evaluationId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                SenderId = _iUnitOfWork.userId ?? 0,
                TypeChat = typeChat
            };
            _iUnitOfWork.EvaluationChat.Add(chat);
            _iUnitOfWork.Complete();

            if (files != null && files.Count > 0)
            {
                var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                Directory.CreateDirectory(uploadRoot);
                var chatFiles = new List<EvaluationChatFile>();
                foreach (var file in files)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var savePath = Path.Combine(uploadRoot, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var relativePath = $"/uploads/chat/{fileName}";
                    chatFiles.Add(new EvaluationChatFile
                    {
                        EvaluationChatId = chat.Id,
                        FileName = file.FileName,
                        FilePath = relativePath
                    });
                }
                _iUnitOfWork.EvaluationChatFile.AddRange(chatFiles);
                _iUnitOfWork.Complete();
            }
        }
    }
}
