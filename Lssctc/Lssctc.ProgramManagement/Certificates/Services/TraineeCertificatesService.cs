using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Common.Services;
using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public class TraineeCertificatesService : ITraineeCertificatesService
    {
        private readonly LssctcDbContext _context;
        private readonly IFirebaseStorageService _firebaseStorage;

        public TraineeCertificatesService(LssctcDbContext context, IFirebaseStorageService firebaseStorage)
        {
            _context = context;
            _firebaseStorage = firebaseStorage;
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

        public async Task<TraineeCertificateResponseDto> CreateCertificateAsync(CreateTraineeCertificateDto dto)
        {
            // 1. Fetch Data
            var enrollment = await _context.Enrollments
                .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            var courseCert = await _context.CourseCertificates
                .Include(cc => cc.Certificate)
                .FirstOrDefaultAsync(cc => cc.Id == dto.CourseCertificateId);

            if (enrollment == null || courseCert == null)
                throw new Exception("Enrollment or Course Certificate Configuration not found");

            // 2. Prepare HTML
            string htmlContent = courseCert.Certificate.TemplateHtml ?? "<h1>No Template Found</h1>";
            htmlContent = htmlContent.Replace("{{TraineeName}}", enrollment.Trainee.IdNavigation.Fullname);
            htmlContent = htmlContent.Replace("{{CourseName}}", enrollment.Class.ProgramCourse.Course.Name);
            htmlContent = htmlContent.Replace("{{IssuedDate}}", DateTime.Now.ToString("dd MMM yyyy"));

            // ---------------------------------------------------------
            // 3. Generate REAL PDF (Linux/Azure Compatible Logic)
            // ---------------------------------------------------------

            // A. Define options to use the System Temp folder.
            // This is critical because Azure/Linux services often block writing to the app's root folder.
            var browserFetcherOptions = new BrowserFetcherOptions
            {
                Path = Path.GetTempPath()
            };

            var browserFetcher = new BrowserFetcher(browserFetcherOptions);

            // B. Download the browser. 
            // We do NOT pass 'DefaultChromiumRevision' because it is deprecated.
            // The method returns an 'InstalledBrowser' object which contains the correct path.
            var installedBrowser = await browserFetcher.DownloadAsync();

            // C. Launch Options
            // 'Headless = true' is required for servers.
            // The 'Args' are mandatory for Linux/Docker/Azure environments to bypass sandbox restrictions.
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = installedBrowser.GetExecutablePath(),
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage", // Fixes "out of memory" crashes in containers
                    "--disable-gpu"            // Servers typically don't have GPUs
                }
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

            // ---------------------------------------------------------
            // 4. Upload to Firebase
            // ---------------------------------------------------------
            string uniqueFileName = $"certificates/{Guid.NewGuid()}_{enrollment.Trainee.TraineeCode}.pdf";
            string pdfUrl = await _firebaseStorage.UploadFileAsync(new MemoryStream(pdfBytes), uniqueFileName, "application/pdf");

            // 5. Save to DB
            var newCert = new TraineeCertificate
            {
                EnrollmentId = dto.EnrollmentId,
                CourseCertificateId = dto.CourseCertificateId,
                CertificateCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                IssuedDate = DateTime.Now,
                PdfUrl = pdfUrl
            };

            _context.TraineeCertificates.Add(newCert);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(newCert.Id);
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
    }
}