using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Controllers
{
    [Route("api/class-downloads")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ClassDownloadsController : ControllerBase
    {
        /// <summary>
        /// Downloads an Excel template for bulk importing Trainees into a specific Class.
        /// This template helps administrators prepare data for the POST /api/classes/{classId}/import-trainees endpoint.
        /// </summary>
        /// <returns>Excel file (.xlsx) with sample data and proper column structure</returns>
        [HttpGet("import-trainee-to-class-template")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetImportTraineeToClassTemplate()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    // 1. Create Worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Import_Trainees_Template");

                    // 2. Create Header (Row 1)
                    string[] headers = {
                        "Username", "Email", "Fullname", "Password", "PhoneNumber", "AvatarUrl"
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
                    var sampleData = new List<object[]>
                    {
                        new object[] {
                            "trainee001",
                            "trainee001@example.com",
                            "Nguyen Van A",
                            "Password123",
                            "0901234567",
                            "https://example.com/avatar1.jpg"
                        },
                        new object[] {
                            "trainee002",
                            "trainee002@example.com",
                            "Tran Thi B",
                            "Password123",
                            "0902345678",
                            "https://example.com/avatar2.jpg"
                        },
                        new object[] {
                            "trainee003",
                            "trainee003@example.com",
                            "Le Van C",
                            "Password123",
                            "", // Optional phone number
                            "" // Optional avatar URL
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
                    worksheet.Column(1).Style.Numberformat.Format = "@"; // Username
                    worksheet.Column(2).Style.Numberformat.Format = "@"; // Email
                    worksheet.Column(3).Style.Numberformat.Format = "@"; // Fullname
                    worksheet.Column(4).Style.Numberformat.Format = "@"; // Password
                    worksheet.Column(5).Style.Numberformat.Format = "@"; // PhoneNumber
                    worksheet.Column(6).Style.Numberformat.Format = "@"; // AvatarUrl

                    // 7. Add Instructions Sheet
                    var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
                    instructionsSheet.Cells["A1"].Value = "INSTRUCTIONS FOR BULK TRAINEE IMPORT TO CLASS";
                    instructionsSheet.Cells["A1"].Style.Font.Bold = true;
                    instructionsSheet.Cells["A1"].Style.Font.Size = 14;

                    var instructions = new List<string>
                    {
                        "",
                        "OVERVIEW:",
                        "This template allows you to import multiple trainees into a specific class in a single Excel file.",
                        "Each row represents one trainee to be created (if they don't exist) and enrolled in the class.",
                        "",
                        "IMPORTANT RULES:",
                        "1. Each row represents ONE trainee account to be created and enrolled",
                        "2. If a User already exists (by Username or Email), the existing user will be used",
                        "3. If the User is already enrolled in the class, that row will be SKIPPED silently",
                        "4. All new trainee accounts will automatically have the 'Trainee' role assigned",
                        "5. A unique Trainee Code will be automatically generated (format: CS + 6 random characters)",
                        "6. Required fields: Username, Email, Fullname, Password",
                        "7. Optional fields: PhoneNumber, AvatarUrl",
                        "",
                        "COLUMN DESCRIPTIONS:",
                        "• Username: 3-50 characters (required, must be unique)",
                        "• Email: Valid email format (required, must be unique)",
                        "• Fullname: Up to 100 characters (required)",
                        "• Password: At least 6 characters (required) - Will be securely hashed",
                        "• PhoneNumber: Valid phone format, up to 15 digits (optional)",
                        "• AvatarUrl: Valid URL format (optional)",
                        "",
                        "IMPORT LOGIC:",
                        "For each row in the Excel file:",
                        "1. Check if a User with this Username or Email already exists",
                        "   - If YES: Use the existing User's ID",
                        "   - If NO: Create a new User with Role = 'Trainee'",
                        "2. Check if an Enrollment already exists for this User and the target Class",
                        "   - If YES: SKIP this row silently (do not throw exception)",
                        "   - If NO: Create a new Enrollment with Status = 'Enrolled'",
                        "",
                        "VALIDATION RULES:",
                        "• Username: 3-50 characters, required",
                        "• Email: Must be a valid email format, required",
                        "• Fullname: Maximum 100 characters, required",
                        "• Password: Minimum 6 characters, maximum 100 characters, required",
                        "• PhoneNumber: Maximum 15 digits, optional",
                        "• AvatarUrl: Must be a valid URL if provided, optional",
                        "",
                        "EXAMPLE STRUCTURE:",
                        "Row 1 (Headers): Username | Email | Fullname | Password | PhoneNumber | AvatarUrl",
                        "Row 2: trainee001 | trainee001@example.com | Nguyen Van A | Password123 | 0901234567 | https://...",
                        "Row 3: trainee002 | trainee002@example.com | Tran Thi B | Password123 | 0902345678 | https://...",
                        "Row 4: trainee003 | trainee003@example.com | Le Van C | Password123 | | (empty optional fields)",
                        "",
                        "NEXT STEPS:",
                        "1. Fill out the template following the sample data in the first sheet",
                        "2. Ensure all required fields are filled",
                        "3. Remove the sample data rows and add your actual trainee data",
                        "4. Import this file using: POST /api/classes/{classId}/import-trainees",
                        "5. Check the response message for import results (created users, enrolled count, skipped count)",
                        "",
                        "SECURITY NOTES:",
                        "• Passwords will be securely hashed using PBKDF2 with SHA256 before storing",
                        "• Each user is assigned a random salt for additional security",
                        "• Consider using strong, unique passwords for each trainee account",
                        "",
                        "TRANSACTION SAFETY:",
                        "• The entire batch operation is wrapped in a database transaction",
                        "• If any critical error occurs, all changes will be rolled back",
                        "• Individual row failures (duplicates, validation errors) will be reported but won't stop the import"
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

                    string excelName = "Import_Trainees_To_Class_Template.xlsx";
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
