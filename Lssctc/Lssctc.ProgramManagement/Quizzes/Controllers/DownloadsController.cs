using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Lssctc.ProgramManagement.Quizzes.Controllers
{
    [Route("api/v1/downloads")]
    [ApiController]
    [Authorize]
    public class DownloadsController : ControllerBase
    {
        [HttpGet("quiz-template")]
        [Authorize(Roles = "Admin, Instructor")]
        public IActionResult GetQuizTemplate()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    // 1. Tạo Sheet
                    var worksheet = package.Workbook.Worksheets.Add("Crane_Training_Quiz_Template");

                    // 2. Tạo Header (Dòng 1)
                    string[] headers = {
                        "Question Name", "Score", "Is Multiple", "Description",
                        "Option Name", "Is Correct", "Explanation"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                    }

                    // 3. Định dạng Header (In đậm, nền xám)
                    using (var range = worksheet.Cells["A1:G1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // 4. Dữ liệu mẫu (Tiếng Anh - Chủ đề Cẩu tự hành)
                    var sampleData = new List<object[]>
                    {
                        // --- Câu 1 (2.5 điểm) ---
                        new object[] { "What must the operator check before operating the crane?", 2.5, false, "Safety Check", "Check engine oil, coolant, tires, and brake system.", true, "Mandatory safety procedure." },
                        new object[] { "What must the operator check before operating the crane?", 2.5, false, "Safety Check", "Just check if the key is present.", false, "Incorrect and dangerous." },
                        
                        // --- Câu 2 (2.5 điểm) ---
                        new object[] { "When deploying outriggers, which requirement is correct?", 2.5, false, "Crane Technique", "Fully extend outriggers and use pads on soft ground.", true, "Ensures maximum stability." },
                        new object[] { "When deploying outriggers, which requirement is correct?", 2.5, false, "Crane Technique", "Extend outriggers halfway to save space.", false, "Risk of tipping over." },
                        
                        // --- Câu 3 (2.5 điểm) ---
                        new object[] { "Hand signal: Arm extended, palm down, moving hand horizontally means?", 2.5, false, "Hand Signals", "Emergency Stop.", true, "Standard ISO signal." },
                        new object[] { "Hand signal: Arm extended, palm down, moving hand horizontally means?", 2.5, false, "Hand Signals", "Raise the boom.", false, "Incorrect signal." },

                        // --- Câu 4 (2.5 điểm) 
                        new object[] { "Which action is STRICTLY PROHIBITED when operating a crane?", 2.5, false, "Safety Rules", "Using the crane hook/bucket to lift people.", true, "Strictly forbidden by safety regulations." },
                        new object[] { "Which action is STRICTLY PROHIBITED when operating a crane?", 2.5, false, "Safety Rules", "Operating at night with proper lighting.", false, "Allowed if visibility is good." }
                    };

                    int startRow = 2;
                    foreach (var rowData in sampleData)
                    {
                        for (int col = 0; col < rowData.Length; col++)
                        {
                            worksheet.Cells[startRow, col + 1].Value = rowData[col];
                        }
                        startRow++;
                    }

                    // 5. Format cột Score (Cột 2) thành Text
                    // Để Excel không tự động sửa "2.5" thành ngày tháng "02-May"
                    worksheet.Column(2).Style.Numberformat.Format = "@";

                    // Tự động căn chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // 6. Xuất file ra bộ nhớ và trả về
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string excelName = "Crane_Training_Quiz_Template.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating sample file: " + ex.Message });
            }
        }
    }
}