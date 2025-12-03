using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Common.Services; // Your Firebase Service Namespace
using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            // 1. Fetch Data (Same as before)
            var enrollment = await _context.Enrollments
                .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            var courseCert = await _context.CourseCertificates
                .Include(cc => cc.Certificate)
                .FirstOrDefaultAsync(cc => cc.Id == dto.CourseCertificateId);

            if (enrollment == null || courseCert == null)
                throw new Exception("Enrollment or Course Certificate Configuration not found");

            // 2. Prepare HTML (Replace Placeholders - Same as before)
            string htmlContent = courseCert.Certificate.TemplateHtml ?? "<h1>No Template Found</h1>";
            htmlContent = htmlContent.Replace("{{TraineeName}}", enrollment.Trainee.IdNavigation.Fullname);
            htmlContent = htmlContent.Replace("{{CourseName}}", enrollment.Class.ProgramCourse.Course.Name);
            htmlContent = htmlContent.Replace("{{IssuedDate}}", DateTime.Now.ToString("dd MMM yyyy"));

            // ---------------------------------------------------------
            // 3. Generate REAL PDF using PuppeteerSharp (NEW LOGIC)
            // ---------------------------------------------------------

            // Download the browser revision if not present (only happens once usually)
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            // Launch a headless browser
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();

            // Set the HTML content
            await page.SetContentAsync(htmlContent);

            // Generate the PDF bytes
            // Format.A4 is standard, PrintBackground=true ensures colors/borders show up
            byte[] pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "0px", Bottom = "0px", Left = "0px", Right = "0px" }
            });

            // ---------------------------------------------------------
            // 4. Upload to Firebase (Same as before)
            // ---------------------------------------------------------
            string uniqueFileName = $"certificates/{Guid.NewGuid()}_{enrollment.Trainee.TraineeCode}.pdf";

            // Now 'pdfBytes' contains actual PDF binary data
            string pdfUrl = await _firebaseStorage.UploadFileAsync(new MemoryStream(pdfBytes), uniqueFileName, "application/pdf");

            // 5. Save to DB (Same as before)
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

            // Optional: You could also delete the file from Firebase here if the API supports it.

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