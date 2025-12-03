using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Lssctc.ProgramManagement.Programs.Controllers
{
    [Route("api/program-downloads")]
    [ApiController]
    [Authorize(Roles = "Admin, Instructor")]
    public class ProgramDownloadsController : ControllerBase
    {
        /// <summary>
        /// Downloads an Excel template for creating a Program with its complete hierarchy (Courses and Sections).
        /// This template helps users prepare data for the POST /api/programs/create-full endpoint.
        /// </summary>
        /// <returns>Excel file (.xlsx) with sample data and proper column structure</returns>
        [HttpGet("template/create-hierarchy")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetProgramHierarchyTemplate()
        {
            try
            {
                // Set EPPlus license (matching the pattern from DownloadsController)
                ExcelPackage.License.SetNonCommercialPersonal("Instructor");

                using (var package = new ExcelPackage())
                {
                    // 1. Create Worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Program_Hierarchy_Template");

                    // 2. Create Header (Row 1)
                    string[] headers = {
                        "Program Name", "Program Description", "Program Image URL",
                        "Course Name", "Course Description", "Category ID", "Level ID",
                        "Duration Hours", "Price (VND)", "Course Image URL",
                        "Section Title", "Section Description", "Duration Minutes"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                    }

                    // 3. Format Header (Bold, Gray Background, Center Aligned)
                    using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // 4. Sample Data
                    // IMPORTANT NOTES:
                    // - Program info (columns 1-3) should be repeated for each row of the same program
                    // - Course info (columns 4-10) should be repeated for each section of that course
                    // - Section info (columns 11-13) changes per section
                    // - Leave Section columns empty if the course has no sections
                    
                    var sampleData = new List<object[]>
                    {
                        // Program: Mobile Crane Training Program
                        // Course 1: Basic Crane Operations (with 2 sections)
                        new object[] {
                            "Mobile Crane Training Program",
                            "Complete training program for mobile crane operation",
                            "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg",
                            "Basic Crane Operations",
                            "Introduction to crane operations and safety fundamentals",
                            1, // Category ID (e.g., 1 = Heavy Equipment)
                            1, // Level ID (e.g., 1 = Beginner)
                            40, // Duration Hours
                            5000000, // Price in VND
                            "https://example.com/course1.jpg",
                            "Safety Procedures",
                            "Safety guidelines and standard operating procedures",
                            120 // Duration in Minutes
                        },
                        new object[] {
                            "Mobile Crane Training Program",
                            "Complete training program for mobile crane operation",
                            "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg",
                            "Basic Crane Operations",
                            "Introduction to crane operations and safety fundamentals",
                            1,
                            1,
                            40,
                            5000000,
                            "https://example.com/course1.jpg",
                            "Equipment Overview",
                            "Understanding crane components and controls",
                            180
                        },
                        
                        // Course 2: Advanced Crane Techniques (with 1 section)
                        new object[] {
                            "Mobile Crane Training Program",
                            "Complete training program for mobile crane operation",
                            "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg",
                            "Advanced Crane Techniques",
                            "Advanced lifting techniques and load calculations",
                            1,
                            2, // Level ID (e.g., 2 = Advanced)
                            60,
                            7500000,
                            "https://example.com/course2.jpg",
                            "Load Calculation Methods",
                            "Advanced methods for calculating safe loads",
                            240
                        },
                        
                        // Course 3: Course Without Sections (leave section columns empty)
                        new object[] {
                            "Mobile Crane Training Program",
                            "Complete training program for mobile crane operation",
                            "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg",
                            "Maintenance and Inspection",
                            "Regular maintenance procedures and inspection checklists",
                            1,
                            1,
                            20,
                            3000000,
                            "https://example.com/course3.jpg",
                            "", // No section title
                            "", // No section description
                            "" // No duration
                        }
                    };

                    // 5. Write Sample Data to Worksheet
                    int startRow = 2;
                    foreach (var rowData in sampleData)
                    {
                        for (int col = 0; col < rowData.Length; col++)
                        {
                            worksheet.Cells[startRow, col + 1].Value = rowData[col];
                        }
                        startRow++;
                    }

                    // 6. Format specific columns as Text to prevent Excel auto-formatting issues
                    worksheet.Column(6).Style.Numberformat.Format = "@"; // Category ID
                    worksheet.Column(7).Style.Numberformat.Format = "@"; // Level ID
                    worksheet.Column(8).Style.Numberformat.Format = "@"; // Duration Hours
                    worksheet.Column(9).Style.Numberformat.Format = "#,##0"; // Price (Number with thousand separator)
                    worksheet.Column(13).Style.Numberformat.Format = "@"; // Duration Minutes

                    // 7. Add Instructions Sheet
                    var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
                    instructionsSheet.Cells["A1"].Value = "INSTRUCTIONS FOR CREATING PROGRAM HIERARCHY";
                    instructionsSheet.Cells["A1"].Style.Font.Bold = true;
                    instructionsSheet.Cells["A1"].Style.Font.Size = 14;

                    var instructions = new List<string>
                    {
                        "",
                        "OVERVIEW:",
                        "This template allows you to create a complete Training Program with Courses and Sections in a single Excel file.",
                        "The structure is hierarchical: Program -> Courses -> Sections",
                        "",
                        "IMPORTANT RULES:",
                        "1. Each row represents ONE SECTION of a course (or one course without sections if section columns are empty)",
                        "2. Program information (columns 1-3) must be IDENTICAL for all rows belonging to the same program",
                        "3. Course information (columns 4-10) must be IDENTICAL for all rows belonging to the same course",
                        "4. Section information (columns 11-13) is unique per row",
                        "5. A course can have NO sections (leave section columns empty)",
                        "6. At least ONE course is required per program",
                        "7. Maximum 50 courses per program, maximum 100 sections per course",
                        "",
                        "COLUMN DESCRIPTIONS:",
                        "• Program Name: 3-200 characters (required)",
                        "• Program Description: Up to 1000 characters (optional)",
                        "• Program Image URL: Valid URL format (optional)",
                        "• Course Name: 3-200 characters (required)",
                        "• Course Description: Up to 1000 characters (optional)",
                        "• Category ID: Must exist in system (required) - Check with admin for valid IDs",
                        "• Level ID: Must exist in system (required) - Check with admin for valid IDs",
                        "• Duration Hours: 1-500 hours (required)",
                        "• Price (VND): 0 to 1,000,000,000 (optional, default currency is VND)",
                        "• Course Image URL: Valid URL format (optional)",
                        "• Section Title: 3-200 characters (required if creating sections)",
                        "• Section Description: Up to 1000 characters (optional)",
                        "• Duration Minutes: 1-1000 minutes (required if creating sections)",
                        "",
                        "EXAMPLE STRUCTURE:",
                        "Row 1: Program A | Course 1 | Section 1",
                        "Row 2: Program A | Course 1 | Section 2",
                        "Row 3: Program A | Course 2 | Section 1",
                        "Row 4: Program A | Course 3 | (no sections - leave empty)",
                        "",
                        "NEXT STEPS:",
                        "1. Fill out the template following the sample data in the first sheet",
                        "2. Verify Category IDs and Level IDs exist in your system",
                        "3. Import this file using your application's import functionality",
                        "4. Or convert to JSON and POST to: /api/programs/create-full"
                    };

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        instructionsSheet.Cells[i + 2, 1].Value = instructions[i];
                    }

                    instructionsSheet.Cells.AutoFitColumns();

                    // 8. Auto-fit columns in main sheet
                    worksheet.Cells.AutoFitColumns();

                    // 9. Export file to memory stream and return
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string excelName = "Program_Hierarchy_Template.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating template file: " + ex.Message });
            }
        }
    }
}
