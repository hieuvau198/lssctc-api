using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Common.Services;
using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text;
using System.Text.Json;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public class TraineeCertificatesService : ITraineeCertificatesService
    {
        private readonly LssctcDbContext _context;
        private readonly IFirebaseStorageService _firebaseStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;

        public TraineeCertificatesService(
            LssctcDbContext context,
            IFirebaseStorageService firebaseStorage,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMailService mailService)
        {
            _context = context;
            _firebaseStorage = firebaseStorage;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _mailService = mailService;
        }

        public async Task<IEnumerable<TraineeCertificateResponseDto>> GetAllAsync()
        {
            return await _context.TraineeCertificates
                .Include(x => x.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(x => x.CourseCertificate).ThenInclude(cc => cc.Course)
                .Select(x => new TraineeCertificateResponseDto
                {
                    Id = x.Id,
                    CertificateCode = x.CertificateCode,
                    IssuedDate = x.IssuedDate,
                    PdfUrl = x.PdfUrl,
                    TraineeName = x.Enrollment.Trainee.IdNavigation.Fullname,
                    CourseName = x.CourseCertificate.Course.Name
                }).ToListAsync();
        }

        public async Task<TraineeCertificateResponseDto> GetByIdAsync(int id)
        {
            var entity = await _context.TraineeCertificates
                .Include(x => x.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(x => x.CourseCertificate).ThenInclude(cc => cc.Course)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<TraineeCertificateResponseDto> GetByCodeAsync(string code)
        {
            var entity = await _context.TraineeCertificates
                .Include(x => x.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(x => x.CourseCertificate).ThenInclude(cc => cc.Course)
                .FirstOrDefaultAsync(x => x.CertificateCode == code);

            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<IEnumerable<TraineeCertificateResponseDto>> GetTraineeCertificatesByClassIdAsync(int classId)
        {
            return await _context.TraineeCertificates
                .Include(x => x.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(x => x.CourseCertificate).ThenInclude(cc => cc.Course)
                .Where(x => x.Enrollment.ClassId == classId)
                .Select(x => new TraineeCertificateResponseDto
                {
                    Id = x.Id,
                    CertificateCode = x.CertificateCode,
                    IssuedDate = x.IssuedDate,
                    PdfUrl = x.PdfUrl,
                    TraineeName = x.Enrollment.Trainee.IdNavigation.Fullname,
                    CourseName = x.CourseCertificate.Course.Name
                }).ToListAsync();
        }

        public async Task<TraineeCertificateResponseDto> CreateCertificateAsync(CreateTraineeCertificateDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            var courseCert = await _context.CourseCertificates
                .Include(cc => cc.Certificate)
                .FirstOrDefaultAsync(cc => cc.Id == dto.CourseCertificateId);

            if (enrollment == null || courseCert == null)
                throw new Exception("Enrollment or Course Certificate Configuration not found");

            string htmlContent = PrepareHtmlContent(courseCert.Certificate.TemplateHtml, enrollment);

            string pdfUrl = await GeneratePdfAndUploadAsync(htmlContent, enrollment.Trainee.TraineeCode);

            var newCert = new TraineeCertificate
            {
                EnrollmentId = dto.EnrollmentId,
                CourseCertificateId = dto.CourseCertificateId,
                CertificateCode = GenerateCertificateCode(),
                IssuedDate = DateTime.UtcNow.AddHours(7),
                PdfUrl = pdfUrl
            };

            _context.TraineeCertificates.Add(newCert);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newCert.Id);
        }

        public async Task CreateTraineeCertificatesForCompleteClass(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null) throw new Exception("Class not found.");

            if (classEntity.Status != (int)ClassStatusEnum.Completed)
            {
                return;
            }

            int courseId = classEntity.ProgramCourse.CourseId;
            string courseName = classEntity.ProgramCourse.Course.Name;

            var courseCert = await _context.CourseCertificates
                .Include(cc => cc.Certificate)
                .FirstOrDefaultAsync(cc => cc.CourseId == courseId && cc.IsActive == true);

            if (courseCert == null)
            {
                var defaultCert = await _context.Certificates.FirstOrDefaultAsync(c => c.IsActive == true);
                if (defaultCert == null) return;

                courseCert = new CourseCertificate
                {
                    CourseId = courseId,
                    CertificateId = defaultCert.Id,
                    IsActive = true,
                    PassingScore = 0
                };
                _context.CourseCertificates.Add(courseCert);
                await _context.SaveChangesAsync();
                courseCert.Certificate = defaultCert;
            }

            var eligibleEnrollments = await _context.Enrollments
                .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(e => e.FinalExams)
                .Include(e => e.TraineeCertificates)
                .Where(e => e.ClassId == classId
                            && e.Status == (int)EnrollmentStatusEnum.Completed
                            && e.FinalExams.Any(fe => fe.IsPass == true)
                            && !e.TraineeCertificates.Any(tc => tc.CourseCertificate.CourseId == courseId))
                .ToListAsync();

            foreach (var enrollment in eligibleEnrollments)
            {
                try
                {
                    string htmlContent = PrepareHtmlContent(courseCert.Certificate.TemplateHtml, enrollment);
                    string pdfUrl = await GeneratePdfAndUploadAsync(htmlContent, enrollment.Trainee.TraineeCode);

                    var newCert = new TraineeCertificate
                    {
                        EnrollmentId = enrollment.Id,
                        CourseCertificateId = courseCert.Id,
                        CertificateCode = GenerateCertificateCode(),
                        IssuedDate = DateTime.UtcNow.AddHours(7),
                        PdfUrl = pdfUrl
                    };
                    _context.TraineeCertificates.Add(newCert);

                    var userEmail = enrollment.Trainee.IdNavigation.Email;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        string subject = $"Certificate of Completion - {courseName}";
                        string body = $"<p>Dear {enrollment.Trainee.IdNavigation.Fullname},</p>" +
                                      $"<p>Congratulations! You have successfully completed the course <strong>{courseName}</strong>.</p>" +
                                      $"<p>Your official certificate is attached below:</p>" +
                                      $"<p><a href='{pdfUrl}'>Download Certificate</a></p>" +
                                      $"<p>Best Regards,<br/>LSSCTC Team</p>";

                        await _mailService.SendEmailAsync(userEmail, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing certificate for Enrollment {enrollment.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCertificateAsync(int id)
        {
            var entity = await _context.TraineeCertificates.FindAsync(id);
            if (entity == null) return false;

            _context.TraineeCertificates.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private TraineeCertificateResponseDto MapToDto(TraineeCertificate entity)
        {
            return new TraineeCertificateResponseDto
            {
                Id = entity.Id,
                CertificateCode = entity.CertificateCode,
                IssuedDate = entity.IssuedDate,
                PdfUrl = entity.PdfUrl,
                TraineeName = entity.Enrollment?.Trainee?.IdNavigation?.Fullname ?? "Unknown",
                CourseName = entity.CourseCertificate?.Course?.Name ?? "Unknown"
            };
        }

        private string PrepareHtmlContent(string templateHtml, Enrollment enrollment)
        {
            string htmlContent = templateHtml ?? "<h1>No Template Found</h1>";
            htmlContent = htmlContent.Replace("{{TraineeName}}", enrollment.Trainee.IdNavigation.Fullname);
            htmlContent = htmlContent.Replace("{{CourseName}}", enrollment.Class.ProgramCourse.Course.Name);
            htmlContent = htmlContent.Replace("{{IssuedDate}}", DateTime.UtcNow.AddHours(7).ToString("dd MMM yyyy"));
            return htmlContent;
        }

        private string GenerateCertificateCode()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        private async Task<string> GeneratePdfAndUploadAsync(string htmlContent, string traineeCode)
        {
            string apiKey = _configuration["PdfBoltApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("PDFBolt API Key is missing in configuration.");
            }

            using var client = _httpClientFactory.CreateClient();
            string base64Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlContent));

            var requestPayload = new
            {
                html = base64Html,
                format = "A4",
                landscape = true,
                scale = 1.0,
                printBackground = true,
                waitUntil = "networkidle"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pdfbolt.com/v1/direct");
            request.Headers.Add("API-KEY", apiKey);
            request.Content = jsonContent;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"PDF Generation failed. Status: {response.StatusCode}. Error: {errorContent}");
            }

            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

            string uniqueFileName = $"certificates/{Guid.NewGuid()}_{traineeCode}.pdf";
            return await _firebaseStorage.UploadFileAsync(new MemoryStream(pdfBytes), uniqueFileName, "application/pdf");
        }

        public async Task<TraineeCertificateResponseDto> PuppeCreateCertificateAsync(CreateTraineeCertificateDto dto)
        {
            var enrollment = await _context.Enrollments
               .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
               .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
               .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            var courseCert = await _context.CourseCertificates
                .Include(cc => cc.Certificate)
                .FirstOrDefaultAsync(cc => cc.Id == dto.CourseCertificateId);

            if (enrollment == null || courseCert == null)
                throw new Exception("Enrollment or Course Certificate Configuration not found");

            string htmlContent = PrepareHtmlContent(courseCert.Certificate.TemplateHtml, enrollment);

            var browserFetcherOptions = new BrowserFetcherOptions { Path = Path.GetTempPath() };
            var browserFetcher = new BrowserFetcher(browserFetcherOptions);
            var installedBrowser = await browserFetcher.DownloadAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = installedBrowser.GetExecutablePath(),
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
            };

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);

            byte[] pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "0px", Bottom = "0px", Left = "0px", Right = "0px" }
            });

            string uniqueFileName = $"certificates/{Guid.NewGuid()}_{enrollment.Trainee.TraineeCode}.pdf";
            string pdfUrl = await _firebaseStorage.UploadFileAsync(new MemoryStream(pdfBytes), uniqueFileName, "application/pdf");

            var newCert = new TraineeCertificate
            {
                EnrollmentId = dto.EnrollmentId,
                CourseCertificateId = dto.CourseCertificateId,
                CertificateCode = GenerateCertificateCode(),
                IssuedDate = DateTime.UtcNow.AddHours(7),
                PdfUrl = pdfUrl
            };

            _context.TraineeCertificates.Add(newCert);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newCert.Id);
        }
    }
}