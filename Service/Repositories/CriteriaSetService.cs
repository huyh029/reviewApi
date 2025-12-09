using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using reviewApi.DTO;
using reviewApi.Models;
using System.Linq;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QDoc = QuestPDF.Fluent.Document;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace reviewApi.Service.Repositories
{
    public class CriteriaSetService : ICriteriaSetService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        private string? _exportTitle;
        private string? _exportDate;
        public CriteriaSetService(IUnitOfWork iUnitOfWork)
        {
            _iUnitOfWork = iUnitOfWork;
        }
        public async Task<byte[]> GenerateCriteriaTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("CriteriaTemplate");

            ws.Cells["A1:E1"].Merge = true;
            ws.Cells["A1"].Value = "MẪU DỮ LIỆU TIÊU CHÍ";
            ws.Cells["A1"].Style.Font.Name = "Times New Roman";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 15;
            ws.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            ws.Cells["A2:E2"].Merge = true;
            ws.Cells["A2"].Value = "Lưu ý (ghi nghiêng):";
            ws.Cells["A2"].Style.Font.Italic = true;
            ws.Cells["A2"].Style.Font.Name = "Times New Roman";
            ws.Cells["A2"].Style.Font.Size = 11;

            ws.Cells["A3:E3"].Merge = true;
            ws.Cells["A3"].Value = "1) Tiêu chí gốc: để trống Mã tiêu chí cha. Nếu nhập mã cha sai/không tồn tại, tiêu chí sẽ được tính là tiêu chí gốc.";
            ws.Cells["A3"].Style.Font.Italic = true;
            ws.Cells["A3"].Style.Font.Name = "Times New Roman";
            ws.Cells["A3"].Style.Font.Size = 11;

            ws.Cells["A4:E4"].Merge = true;
            ws.Cells["A4"].Value = "2) Loại điểm: nhập \"cộng\" (điểm cộng) hoặc \"trừ\" (điểm trừ).";
            ws.Cells["A4"].Style.Font.Italic = true;
            ws.Cells["A4"].Style.Font.Name = "Times New Roman";
            ws.Cells["A4"].Style.Font.Size = 11;

            ws.Cells["A5:E5"].Merge = true;
            ws.Cells["A5"].Value = "3) Tiêu chí cha không cần nhập Điểm tối đa; điểm sẽ tổng hợp từ tiêu chí con.";
            ws.Cells["A5"].Style.Font.Italic = true;
            ws.Cells["A5"].Style.Font.Name = "Times New Roman";
            ws.Cells["A5"].Style.Font.Size = 11;

            string[] headers = { "Mã tiêu chí", "Nội dung", "Điểm tối đa", "Loại điểm", "Mã tiêu chí cha" };
            int headerRow = 7;
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[headerRow, i + 1].Value = headers[i];
                ws.Cells[headerRow, i + 1].Style.Font.Bold = true;
                ws.Cells[headerRow, i + 1].Style.Font.Name = "Times New Roman";
                ws.Cells[headerRow, i + 1].Style.Font.Size = 13;
                ws.Cells[headerRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f5f5f5"));
                ws.Cells[headerRow, i + 1].Style.Font.Color.SetColor(System.Drawing.ColorTranslator.FromHtml("#272727"));
                ws.Cells[headerRow, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#d1d5db"));
            }

            int sampleRow = headerRow + 1;
            ws.Cells[sampleRow, 1].Value = "TC01";
            ws.Cells[sampleRow, 2].Value = "Tiêu chí gốc mẫu";
            ws.Cells[sampleRow, 3].Value = "";
            ws.Cells[sampleRow, 4].Value = "cộng";
            ws.Cells[sampleRow, 5].Value = "";

            ws.Cells[sampleRow + 1, 1].Value = "TC01-1";
            ws.Cells[sampleRow + 1, 2].Value = "Tiêu chí con mẫu";
            ws.Cells[sampleRow + 1, 3].Value = 10;
            ws.Cells[sampleRow + 1, 4].Value = "cộng";
            ws.Cells[sampleRow + 1, 5].Value = "TC01";

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public async Task ImportCriteriaFromTemplate(int criteriaSetId, IFormFile file)
        {
            if (file == null || file.Length == 0) throw new Exception("Tệp không hợp lệ");
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage(file.OpenReadStream());
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null) throw new Exception("Tệp không chứa dữ liệu");

            // clear old criteria
            var criteriaSet = _iUnitOfWork.CriteriaSet.GetById(criteriaSetId);
            if (criteriaSet == null) throw new Exception("Không tìm thấy bộ tiêu chí");
            var oldCriteria = _iUnitOfWork.Criteria.Find(c => c.CriteriaSetId == criteriaSetId).ToList();
            if (oldCriteria.Any()) _iUnitOfWork.Criteria.RemoveRange(oldCriteria);

            int headerRow = 7;
            int row = headerRow + 1;
            var tempNodes = new List<(Criteria node, string? parentCode)>();

            while (true)
            {
                var code = ws.Cells[row, 1].Text?.Trim();
                var name = ws.Cells[row, 2].Text?.Trim();
                var maxScoreText = ws.Cells[row, 3].Text?.Trim();
                var typeText = (ws.Cells[row, 4].Text ?? "").Trim().ToLower();
                var parentCode = ws.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                    break;

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    row++;
                    continue;
                }

                var typeScore = typeText == "trừ" ? "subtractive" : "additive";
                int.TryParse(maxScoreText, out var maxScore);

                tempNodes.Add((new Criteria
                {
                    Code = code,
                    Name = name,
                    MaxScore = maxScore,
                    TypeScore = typeScore,
                    CriteriaSetId = criteriaSetId,
                    parentId = null,
                }, parentCode));
                row++;
            }

            foreach (var item in tempNodes)
            {
                _iUnitOfWork.Criteria.Add(item.node);
            }
            _iUnitOfWork.Complete();

            var persistedMap = _iUnitOfWork.Criteria
                .Find(c => c.CriteriaSetId == criteriaSetId)
                .ToDictionary(c => c.Code, c => c);

            foreach (var item in tempNodes)
            {
                if (string.IsNullOrWhiteSpace(item.parentCode)) continue;
                if (!persistedMap.TryGetValue(item.node.Code, out var child)) continue;
                if (persistedMap.TryGetValue(item.parentCode, out var parent))
                {
                    child.parentId = parent.Id;
                }
            }
            _iUnitOfWork.Complete();
        }

        public async Task<List<CriteriaNodeDTO>> PreviewCriteriaFromTemplate(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new Exception("Tệp không hợp lệ");
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage(file.OpenReadStream());
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null) throw new Exception("Tệp không chứa dữ liệu");

            int headerRow = 7;
            int row = headerRow + 1;
            var temp = new List<(CriteriaNodeDTO node, string? parentCode)>();
            var tempId = -1; // tạm id âm để đảm bảo duy nhất khi preview

            while (true)
            {
                var code = ws.Cells[row, 1].Text?.Trim();
                var name = ws.Cells[row, 2].Text?.Trim();
                var maxScoreText = ws.Cells[row, 3].Text?.Trim();
                var typeText = (ws.Cells[row, 4].Text ?? "").Trim().ToLower();
                var parentCode = ws.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) break;
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    row++;
                    continue;
                }

                int.TryParse(maxScoreText, out var maxScore);
                var typeScore = typeText == "trừ" ? "subtractive" : "additive";

                temp.Add((new CriteriaNodeDTO
                {
                    id = tempId--,
                    code = code,
                    name = name,
                    maxScore = maxScore,
                    scoreType = typeScore,
                    children = new List<CriteriaNodeDTO>()
                }, parentCode));
                row++;
            }

            // Build tree
            var lookup = temp.ToDictionary(t => t.node.code, t => t.node);
            foreach (var item in temp)
            {
                if (string.IsNullOrWhiteSpace(item.parentCode)) continue;
                if (lookup.TryGetValue(item.parentCode, out var parent))
                {
                    parent.children ??= new List<CriteriaNodeDTO>();
                    parent.children.Add(item.node);
                }
            }

            var roots = temp.Where(t => string.IsNullOrWhiteSpace(t.parentCode) || !lookup.ContainsKey(t.parentCode!))
                            .Select(t => t.node)
                            .ToList();
            return roots;
        }

        private void SetExportMetadata()
        {
            var dt = DateTime.Now;
            _exportTitle = "DANH SÁCH BỘ TIÊU CHÍ";
            _exportDate = $"Ngày {dt:dd} tháng {dt:MM} năm {dt:yyyy}";
        }
        public async Task<responseDTO<List<CriteriaSetDTO>>> getCriteriaSets(int page, int limit, string? search, string? _object, int? year, int? month)
        {
            IEnumerable<CriteriaSet> CriteriaSetDTOs;
            if (search != null) search = search.ToLower();
       
            var ids1 = _iUnitOfWork.CriteriaSet.Find(e => (search == null ? true : e.Name.ToLower().Contains(search))).Select(e => e.Id).ToList();
            var ids2 = _iUnitOfWork.CriteriaSetPeriod.Find(e => (month==null?true:e.month == month)&&(year==null?true:e.year==year)).Select(e => e.CriteriaSetId).ToList();
            var ids3 = _iUnitOfWork.CriteriaSetObject.Find(e => (_object == null ? true : e.EvaluationObjectCode == _object)).Select(e => e.CriteriaSetId).ToList();
            var common = ids1.Intersect(ids2).Intersect(ids3).Distinct().ToList();
            CriteriaSetDTOs = _iUnitOfWork.CriteriaSet.GetByIds(common);
            var allObjects = _iUnitOfWork.CriteriaSetObject.Find(o => common.Contains(o.CriteriaSetId)).ToList();
            var allPeriods = _iUnitOfWork.CriteriaSetPeriod.Find(p => common.Contains(p.CriteriaSetId)).ToList();
            int startIndex = (page - 1) * limit;
            var data = CriteriaSetDTOs
               .Skip(startIndex)
               .Take(limit)
               .Select((e, index) => new CriteriaSetDTO
               {
                    id = e.Id,
                    name = e.Name,
                    applicableObjects = allObjects.Where(o => o.CriteriaSetId == e.Id)
                                                   .Select(o => o.EvaluationObjectCode)
                                                   .ToList(),
                    applicationPeriods = allPeriods.Where(p => p.CriteriaSetId == e.Id)
                    .Select(f => new PeriodDTO
                    {
                        month = f.month,
                        year = f.year
                    }).ToList()
               })
               .ToList();
            return new responseDTO<List<CriteriaSetDTO>>
            {
                data = data,
                pagination = new Pagination
                {
                    currentPage = page,
                    totalItems = CriteriaSetDTOs.Count(),
                    itemsPerPage = limit,
                    totalPages = (int)Math.Ceiling((double)CriteriaSetDTOs.Count() / limit)
                }
            };
        }
        public async Task creatCriteriaSets(CriteriaSetDTO payload)
        {
            if (_iUnitOfWork.CriteriaSet.FindFirst(f => f.Name.Trim() == payload.name.Trim()) != null) throw new Exception("Tên bộ tiêu chí đã tồn tại vui lòng chọn tên khác!");

            var applicableObjects = payload.applicableObjects ?? new List<string>();
            var applicationPeriods = payload.applicationPeriods ?? new List<PeriodDTO>();
            foreach (var obj in applicableObjects)
            {
                foreach (var period in applicationPeriods)
                {
                    var exists = _iUnitOfWork.CriteriaSetObject
                        .Find(o => o.EvaluationObjectCode == obj)
                        .Any(o =>
                        {
                            var periods = _iUnitOfWork.CriteriaSetPeriod.Find(p => p.CriteriaSetId == o.CriteriaSetId);
                            return periods.Any(p => p.month == period.month && p.year == period.year);
                        });
                    if (exists)
                    {
                        throw new Exception($"Đã tồn tại bộ tiêu chí cho đối tượng {obj} tại Tháng {period.month}/{period.year}. Không thể thêm trùng thời gian.");
                    }
                }
            }

            var e = new CriteriaSet();
            e.Name = payload.name;
            e.Classifications = payload.classificationLevels.Select(c => new Classification
            {
                Code = c.code,
                Name = c.name,
                ShortName = c.abbreviation,
                Min = c.fromScore,
                Max = c.toScore,
                CriteriaSet = e
            }).ToList();
            e.Criterias = payload.criteriaTree.Select(c => MapToModel(c,e)).ToList();
            e.CriteriaSetObjects = payload.applicableObjects.Select(c => new CriteriaSetObject
            {
                EvaluationObjectCode = c,
                CriteriaSet = e
            }).ToList();
            e.CriteriaSetPeriods = payload.applicationPeriods.Select(c => new CriteriaSetPeriod
            {
                month = c.month,
                year = c.year,
                CriteriaSet = e
            }).ToList();
            _iUnitOfWork.CriteriaSet.Add(e);
            _iUnitOfWork.Complete();
        }
        private Criteria MapToModel(CriteriaNodeDTO node,CriteriaSet e)
        {
            return new Criteria
            {
                Name = node.name,
                Code = node.code,
                TypeScore = node.scoreType,
                CriteriaSet = e,
                MaxScore = node.maxScore??0,
                Children = node.children?.Select(c => MapToModel(c,e)).ToList()  
            };
        }
        public async Task<CriteriaSetDTO> getById(int id) 
        {
            var e = _iUnitOfWork.CriteriaSet.GetById(id);
            if (e == null) throw new Exception("Tiêu chí đánh giá này chưa tồn tại!");

            var classifications = _iUnitOfWork.Classification.Find(c => c.CriteriaSetId == id).ToList();
            var applicableObjects = _iUnitOfWork.CriteriaSetObject.Find(c => c.CriteriaSetId == id).Select(c => c.EvaluationObjectCode).ToList();
            var applicationPeriods = _iUnitOfWork.CriteriaSetPeriod.Find(c => c.CriteriaSetId == id).Select(c => new PeriodDTO
            {
                month = c.month,
                year = c.year
            }).ToList();
            var criteriaList = _iUnitOfWork.Criteria.Find(c => c.CriteriaSetId == id).ToList();
            var criteriaTree = BuildCriteriaTree(criteriaList, null);

            var res = new CriteriaSetDTO
            {
                id = id,
                name = e.Name,
                classificationLevels = (classifications ?? new List<Classification>()).Select(c => new ClassficationDTO
                {
                    id = c.Id,
                    code = c.Code,
                    name = c.Name,
                    abbreviation = c.ShortName,
                    fromScore = c.Min,
                    toScore = c.Max
                }).ToList(),
                applicableObjects = applicableObjects ?? new List<string>(),
                applicationPeriods = applicationPeriods ?? new List<PeriodDTO>(),
                criteriaTree = (criteriaTree ?? new List<Criteria>()).Select(MapToDtoFromFlat).ToList()
            };
            return res;
        }

        public async Task updateCriteriaSet(CriteriaSetDTO payload)
        {
            var e = _iUnitOfWork.CriteriaSet.GetById(payload.id);
            if (e == null) throw new Exception("Tiêu chí đánh giá này chưa tồn tại!");
            if (_iUnitOfWork.CriteriaSet.FindFirst(f => f.Name.Trim() == payload.name.Trim() && f.Id!=payload.id) != null) throw new Exception("Tên bộ tiêu chí đã tồn tại vui lòng chọn tên khác!");

            // Kiểm tra trùng bộ tiêu chí cho cùng đối tượng và kỳ (loại trừ chính nó)
            var applicableObjects = payload.applicableObjects ?? new List<string>();
            var applicationPeriods = payload.applicationPeriods ?? new List<PeriodDTO>();
            foreach (var obj in applicableObjects)
            {
                foreach (var period in applicationPeriods)
                {
                    var exists = _iUnitOfWork.CriteriaSetObject
                        .Find(o => o.EvaluationObjectCode == obj && o.CriteriaSetId != payload.id)
                        .Any(o =>
                        {
                            var periods = _iUnitOfWork.CriteriaSetPeriod.Find(p => p.CriteriaSetId == o.CriteriaSetId);
                            return periods.Any(p => p.month == period.month && p.year == period.year);
                        });
                    if (exists)
                    {
                        throw new Exception($"Đã tồn tại bộ tiêu chí cho đối tượng {obj} tại Tháng {period.month}/{period.year}. Không thể cập nhật trùng thời gian.");
                    }
                }
            }
            _iUnitOfWork.CriteriaSetObject.GetAll();
            _iUnitOfWork.CriteriaSetPeriod.GetAll();
            _iUnitOfWork.Criteria.GetAll();
            _iUnitOfWork.Classification.GetAll();
            _iUnitOfWork.Classification.RemoveRange(e.Classifications);
            _iUnitOfWork.Criteria.RemoveRange(e.Criterias);
            _iUnitOfWork.CriteriaSetObject.RemoveRange(e.CriteriaSetObjects);
            _iUnitOfWork.CriteriaSetPeriod.RemoveRange(e.CriteriaSetPeriods);
            e.Name = payload.name;
            e.Classifications = payload.classificationLevels.Select(c => new Classification
            {
                Code = c.code,
                Name = c.name,
                ShortName = c.abbreviation,
                Min = c.fromScore,
                Max = c.toScore,
                CriteriaSet = e
            }).ToList();
            e.Criterias = payload.criteriaTree.Select(c => MapToModel(c, e)).ToList();
            e.CriteriaSetObjects = payload.applicableObjects.Select(c => new CriteriaSetObject
            {
                EvaluationObjectCode = c,
                CriteriaSet = e
            }).ToList();
            e.CriteriaSetPeriods = payload.applicationPeriods.Select(c => new CriteriaSetPeriod
            {
                month = c.month,
                year = c.year,
                CriteriaSet = e
            }).ToList();
            _iUnitOfWork.CriteriaSet.Update(e);
            _iUnitOfWork.Complete();
        }

        private List<Criteria> BuildCriteriaTree(List<Criteria> all, int? parentId)
        {
            return all.Where(c => c.parentId == parentId)
                      .Select(c => new Criteria
                      {
                          Id = c.Id,
                          Name = c.Name,
                          Code = c.Code,
                          MaxScore = c.MaxScore,
                          TypeScore = c.TypeScore,
                          parentId = c.parentId,
                          Children = BuildCriteriaTree(all, c.Id)
                      }).ToList();
        }
        private CriteriaNodeDTO MapToDtoFromFlat(Criteria model)
        {
            return new CriteriaNodeDTO
            {
                id = model.Id,
                name = model.Name,
                code = model.Code,
                maxScore = model.MaxScore,
                scoreType = model.TypeScore,
                children = model.Children?.Select(MapToDtoFromFlat).ToList()
            };
        }
        private IEnumerable<CriteriaSet> FilterCriteriaSets(string? search, string? _object, int? year, int? month)
        {
            if (search != null) search = search.ToLower();
            var ids1 = _iUnitOfWork.CriteriaSet.Find(e => search == null || e.Name.ToLower().Contains(search)).Select(e => e.Id).ToList();
            var ids2 = _iUnitOfWork.CriteriaSetPeriod.Find(e => (month == null || e.month == month) && (year == null || e.year == year)).Select(e => e.CriteriaSetId).ToList();
            var ids3 = _iUnitOfWork.CriteriaSetObject.Find(e => (_object == null || e.EvaluationObjectCode == _object)).Select(e => e.CriteriaSetId).ToList();
            var common = ids1.Intersect(ids2).Intersect(ids3).Distinct().ToList();
            return _iUnitOfWork.CriteriaSet.GetByIds(common);
        }
        public async Task<byte[]> GenerateExcel()
        {
            return await GenerateTemplate();
        }
        public async Task<byte[]> GenerateTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Template");
            ws.Cells[1, 1].Value = "Tên bộ tiêu chí";
            ws.Cells[1, 2].Value = "Mã đối tượng áp dụng (ngăn cách bằng dấu phẩy)";
            ws.Cells[1, 3].Value = "Tháng áp dụng";
            ws.Cells[1, 4].Value = "Năm áp dụng";

            // Sample row
            ws.Cells[2, 1].Value = "Bo tieu chi mau";
            ws.Cells[2, 2].Value = "OBJ1,OBJ2";
            ws.Cells[2, 3].Value = DateTime.Now.Month;
            ws.Cells[2, 4].Value = DateTime.Now.Year;

            return package.GetAsByteArray();
        }
        public async Task<byte[]> ExportCriteriaSets(string format, string? search, string? _object, int? year, int? month)
        {
            var criteriaSets = FilterCriteriaSets(search, _object, year, month);
            _iUnitOfWork.CriteriaSetObject.GetAll();
            _iUnitOfWork.CriteriaSetPeriod.GetAll();
            var objectMap = _iUnitOfWork.EvaluationObject.GetAll().ToDictionary(o => o.Code, o => o.Name);
            SetExportMetadata();

            var exportData = criteriaSets.Select(e => new CriteriaSetExportRow
            {
                Name = e.Name,
                Objects = e.CriteriaSetObjects != null
                    ? e.CriteriaSetObjects.Select(o => objectMap.ContainsKey(o.EvaluationObjectCode) ? objectMap[o.EvaluationObjectCode] : o.EvaluationObjectCode).ToList()
                    : new List<string>(),
                Periods = e.CriteriaSetPeriods != null
                    ? e.CriteriaSetPeriods.Select(p => $"T{p.month}/{p.year}").ToList()
                    : new List<string>()
            }).ToList();

            format = string.IsNullOrWhiteSpace(format) ? "xlsx" : format.ToLower();
            return format switch
            {
                "xlsx" => BuildExcel(exportData),
                "docx" => BuildDocx(exportData),
                "pdf" => BuildPdf(exportData),
                _ => BuildHtml(exportData),
            };
        }
        public async Task ImportCriteriaSets(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new Exception("Không tìm thấy tệp tải lên.");

            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) throw new Exception("Tệp không chứa dữ liệu nào.");

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrEmpty(name)) continue;

                var objectCell = worksheet.Cells[row, 2].Text;
                var month = TryParseInt(worksheet.Cells[row, 3].Text);
                var year = TryParseInt(worksheet.Cells[row, 4].Text);

                var applicableObjects = string.IsNullOrEmpty(objectCell)
                    ? new List<string>()
                    : objectCell.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

                var periods = new List<PeriodDTO>();
                if (month.HasValue && year.HasValue)
                {
                    periods.Add(new PeriodDTO { month = month.Value, year = year.Value });
                }

                var payload = new CriteriaSetDTO
                {
                    name = name,
                    applicableObjects = applicableObjects,
                    applicationPeriods = periods,
                    classificationLevels = new List<ClassficationDTO>(),
                    criteriaTree = new List<CriteriaNodeDTO>()
                };

                await creatCriteriaSets(payload);
            }
        }
        public async Task deleteCriteriaSet(int id)
        {
            await deleteCriteriaSets(new DeleteMultiple<int>
            {
                ids = new List<int> { id }
            });
        }
        public async Task deleteCriteriaSets(DeleteMultiple<int> payload)
        {
            if (payload.all == null || payload.all == false)
            {
                if (payload.ids != null)
                    _iUnitOfWork.CriteriaSet.RemoveRange(_iUnitOfWork.CriteriaSet.GetByIds(payload.ids));
            }
            else
            {
                var search = payload.filters?.search?.ToLower();
                var month = payload.filters?.month;
                var year = payload.filters?.year;
                var obj = payload.filters?._object;
                var ids1 = _iUnitOfWork.CriteriaSet.Find(e => (search == null ? true : e.Name.ToLower().Contains(search))).Select(e => e.Id).ToList();
                var ids2 = _iUnitOfWork.CriteriaSetPeriod.Find(e => (month == null ? true : e.month == month) && (year == null ? true : e.year == year)).Select(e => e.CriteriaSetId).ToList();
                var ids3 = _iUnitOfWork.CriteriaSetObject.Find(e => (obj == null ? true : e.EvaluationObjectCode == obj)).Select(e => e.CriteriaSetId).ToList();
                var common = ids1.Intersect(ids2).Intersect(ids3).Distinct();
                common = payload.exclude == null ? common.ToList() : common.Except(payload.exclude).ToList();
                _iUnitOfWork.CriteriaSet.RemoveRange(_iUnitOfWork.CriteriaSet.GetByIds(common));
            }
            _iUnitOfWork.Complete();
        }

        private class CriteriaSetExportRow
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Objects { get; set; } = new();
            public List<string> Periods { get; set; } = new();
        }
        private byte[] BuildExcel(IEnumerable<CriteriaSetExportRow> exportData)
        {
            ExcelPackage.License.SetNonCommercialPersonal("YourName");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("CriteriaSets");
            string[] headers = { "STT", "Tên bộ tiêu chí", "Đối tượng áp dụng", "Kỳ áp dụng" };

            // Title
            ws.Cells["A1:D1"].Merge = true;
            ws.Cells["A1"].Value = _exportTitle ?? "DANH SÁCH BỘ TIÊU CHÍ";
            ws.Cells["A1"].Style.Font.Name = "Times New Roman";
            ws.Cells["A1"].Style.Font.Size = 15;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Date
            ws.Cells["A2:D2"].Merge = true;
            ws.Cells["A2"].Value = _exportDate ?? string.Empty;
            ws.Cells["A2"].Style.Font.Name = "Times New Roman";
            ws.Cells["A2"].Style.Font.Size = 11;
            ws.Cells["A2"].Style.Font.Italic = true;
            ws.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            int headerRow = 3;
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[headerRow, i + 1].Value = headers[i];
                ws.Cells[headerRow, i + 1].Style.Font.Bold = true;
                ws.Cells[headerRow, i + 1].Style.Font.Name = "Times New Roman";
                ws.Cells[headerRow, i + 1].Style.Font.Size = 13;
                ws.Cells[headerRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f5f5f5"));
                ws.Cells[headerRow, i + 1].Style.Font.Color.SetColor(System.Drawing.ColorTranslator.FromHtml("#272727"));
                ws.Cells[headerRow, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#d1d5db"));
                ws.Cells[headerRow, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }

            int row = headerRow + 1;
            int stt = 1;
            foreach (var item in exportData)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = item.Name;
                ws.Cells[row, 3].Value = string.Join(", ", item.Objects);
                ws.Cells[row, 4].Value = string.Join(", ", item.Periods);
                for (int col = 1; col <= headers.Length; col++)
                {
                    ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.ColorTranslator.FromHtml("#d1d5db"));
                    ws.Cells[row, col].Style.Font.Name = "Times New Roman";
                    ws.Cells[row, col].Style.Font.Size = 13;
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }
                row++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }
        private byte[] BuildHtml(IEnumerable<CriteriaSetExportRow> exportData)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"UTF-8\">");
            sb.Append("<style>");
            sb.Append("body{font-family:'Times New Roman',serif;font-size:13px;color:#272727;}");
            sb.Append("table{border-collapse:collapse;width:100%;font-family:'Times New Roman',serif;font-size:13px;}");
            sb.Append("th,td{border:1px solid #d1d5db;padding:8px;text-align:left;}");
            sb.Append("th{background:#f5f5f5;font-weight:700;color:#272727;}");
            sb.Append("</style></head><body>");
            var dt = DateTime.Now;
            sb.Append("<h3 style='text-align:center;margin:8px 0;font-size:15px;font-weight:bold;'>DANH SÁCH BỘ TIÊU CHÍ</h3>");
            sb.Append($"<div style='text-align:right;font-size:11px;font-style:italic;margin-bottom:8px;'>Ngày {dt:dd} tháng {dt:MM} năm {dt:yyyy}</div>");
            sb.Append("<table>");
            sb.Append("<tr><th>STT</th><th>Tên bộ tiêu chí</th><th>Đối tượng áp dụng</th><th>Kỳ áp dụng</th></tr>");
            int stt = 1;
            foreach (var item in exportData)
            {
                sb.Append($"<tr><td>{stt++}</td><td>{item.Name}</td><td>{string.Join(", ", item.Objects)}</td><td>{string.Join(", ", item.Periods)}</td></tr>");
            }
            sb.Append("</table></body></html>");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] BuildDocx(IEnumerable<CriteriaSetExportRow> exportData)
        {
            using var ms = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new W.Document();
                var body = new W.Body();

                var titleText = _exportTitle ?? "DANH SÁCH BỘ TIÊU CHÍ";
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

                string[] headers = { "STT", "Tên bộ tiêu chí", "Đối tượng áp dụng", "Kỳ áp dụng" };
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

                int stt = 1;
                foreach (var r in exportData)
                {
                    var row = new W.TableRow();
                    var vals = new[] { stt.ToString(), r.Name, string.Join(", ", r.Objects), string.Join(", ", r.Periods) };
                    stt++;
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

        private byte[] BuildPdf(IEnumerable<CriteriaSetExportRow> exportData)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var rows = exportData.ToList();
            var doc = QDoc.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Header().Column(col =>
                    {
                        col.Item().Text("DANH SÁCH BỘ TIÊU CHÍ")
                            .FontFamily("Times New Roman")
                            .FontSize(15)
                            .Bold()
                            .AlignCenter();
                        var dt = DateTime.Now;
                        col.Item().AlignRight().Text($"Ngày {dt:dd} tháng {dt:MM} năm {dt:yyyy}")
                            .FontFamily("Times New Roman")
                            .FontSize(11)
                            .Italic();
                    });

                    page.Content().Table(table =>
                    {
                        string[] headers = { "STT", "Tên bộ tiêu chí", "Đối tượng áp dụng", "Kỳ áp dụng" };
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(4);
                            c.RelativeColumn(4);
                            c.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Background("#f5f5f5").Border(0.5f).BorderColor("#d1d5db").Padding(6).Text(h)
                                    .FontFamily("Times New Roman").FontSize(13).Bold().FontColor("#272727").AlignCenter();
                            }
                        });

                        int stt = 1;
                        foreach (var r in rows)
                        {
                            table.Cell().Border(0.5f).BorderColor("#d1d5db").Padding(6).Text($"{stt++}").FontFamily("Times New Roman").FontSize(13).AlignCenter();
                            table.Cell().Border(0.5f).BorderColor("#d1d5db").Padding(6).Text($"{r.Name}").FontFamily("Times New Roman").FontSize(13);
                            table.Cell().Border(0.5f).BorderColor("#d1d5db").Padding(6).Text($"{string.Join(", ", r.Objects ?? new List<string>())}").FontFamily("Times New Roman").FontSize(13).AlignCenter();
                            table.Cell().Border(0.5f).BorderColor("#d1d5db").Padding(6).Text($"{string.Join(", ", r.Periods ?? new List<string>())}").FontFamily("Times New Roman").FontSize(13);
                        }
                    });

                    page.Footer().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(s => s.FontFamily("Times New Roman").FontSize(11));
                        t.Span("Trang ");
                        t.CurrentPageNumber();
                    });
                });
            });

            return doc.GeneratePdf();
        }
        private int? TryParseInt(string? value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        public async Task<int> LookupCriteriaSet(string period, string year)
        {
            var month = ParseMonth(period);
            var y = int.TryParse(year, out var yy) ? yy : 0;
            if (month == 0 || y == 0) throw new Exception("Kỳ đánh giá không hợp lệ");

            // Chỉ lấy bộ tiêu chí phù hợp với đối tượng của người dùng và đang hiệu lực
            var activeObjectCodes = _iUnitOfWork.EvaluationObject
                .Find(o => o.Status != null && o.Status.ToLower() == "active")
                .Select(o => o.Code)
                .ToList();

            var userObjects = _iUnitOfWork.EvaluationObjectRole
                .Find(r => r.UserId == _iUnitOfWork.userId)
                .Select(r => r.EvaluationObjectCode)
                .Where(code => activeObjectCodes.Contains(code))
                .ToList();

            var periodCriteriaSetIds = _iUnitOfWork.CriteriaSetPeriod
                .Find(p => p.month == month && p.year == y)
                .Select(p => p.CriteriaSetId)
                .ToList();

            var objectCriteriaSetIds = _iUnitOfWork.CriteriaSetObject
                .Find(o => userObjects.Contains(o.EvaluationObjectCode))
                .Select(o => o.CriteriaSetId)
                .ToList();

            var criteriaSetId = periodCriteriaSetIds
                .Intersect(objectCriteriaSetIds)
                .FirstOrDefault();

            if (criteriaSetId == 0) throw new Exception("Không tìm thấy bộ tiêu chí cho kỳ này");
            return criteriaSetId;
        }

        private int ParseMonth(string? period)
        {
            if (string.IsNullOrWhiteSpace(period)) return 0;
            var parts = period.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], out var m)) return m;
            return 0;
        }
    }
}
