using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Lssctc.ProgramManagement.Accounts.Users.Controllers
{
    [Route("api/user-downloads")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserDownloadsController : ControllerBase
    {
        /// <summary>
        /// Downloads an Excel template for bulk importing Trainee accounts.
        /// This template helps administrators prepare data for the POST /api/users/import-trainees endpoint.
        /// </summary>
        /// <returns>Excel file (.xlsx) with sample data and proper column structure</returns>
        [HttpGet("trainee-template")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetTraineeTemplate()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    // 1. Create Worksheet
                    var worksheet = package.Workbook.Worksheets.Add("User_Import_Template");

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
                    // IMPORTANT NOTES:
                    // - Username: 3-50 characters (required, must be unique)
                    // - Email: Valid email format (required, must be unique)
                    // - Fullname: Up to 100 characters (required)
                    // - Password: At least 6 characters (required)
                    // - PhoneNumber: Valid phone format, up to 15 digits (optional)
                    // - AvatarUrl: Valid URL format (optional)
                    // - Role will be automatically set to "Trainee" during import

                    var sampleData = new List<object[]>
                    {
                        new object[] {
                            "user001",
                            "user001@example.com",
                            "Nguyen Van A",
                            "Password123",
                            "0901234567",
                            "https://example.com/avatar1.jpg"
                        },
                        new object[] {
                            "user002",
                            "user002@example.com",
                            "Tran Thi B",
                            "Password123",
                            "0902345678",
                            "https://example.com/avatar2.jpg"
                        },
                        new object[] {
                            "user003",
                            "user003@example.com",
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
                    instructionsSheet.Cells["A1"].Value = "INSTRUCTIONS FOR BULK TRAINEE IMPORT";
                    instructionsSheet.Cells["A1"].Style.Font.Bold = true;
                    instructionsSheet.Cells["A1"].Style.Font.Size = 14;

                    var instructions = new List<string>
                    {
                        "",
                        "OVERVIEW:",
                        "This template allows you to create multiple Trainee accounts in a single Excel file.",
                        "Each row represents one trainee account to be created.",
                        "",
                        "IMPORTANT RULES:",
                        "1. Each row represents ONE trainee account",
                        "2. Username and Email must be UNIQUE across the system",
                        "3. If a duplicate Username or Email is found, that row will be SKIPPED silently",
                        "4. All trainee accounts will automatically have the 'Trainee' role assigned",
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
                        "DEDUPLICATION LOGIC:",
                        "• Before creating each trainee, the system checks if the Username OR Email already exists",
                        "• If a duplicate is found, that row is SKIPPED without causing an error",
                        "• The import will continue processing remaining rows",
                        "• A summary message will indicate how many users were imported and how many were skipped",
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
                        "4. Import this file using: POST /api/users/import-trainees",
                        "5. Check the response message for import results (imported count vs. skipped count)",
                        "",
                        "SECURITY NOTES:",
                        "• Passwords will be securely hashed using PBKDF2 with SHA256 before storing",
                        "• Each user is assigned a random salt for additional security",
                        "• Consider using strong, unique passwords for each trainee account"
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

                    string excelName = "User_Import_Template.xlsx";
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
