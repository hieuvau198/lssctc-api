namespace Lssctc.ProgramManagement.Certificates.Dtos
{
    public class TraineeCertificateResponseDto
    {
        public int Id { get; set; }
        public string TraineeName { get; set; }
        public string CourseName { get; set; }
        public string CertificateCode { get; set; }
        public string PdfUrl { get; set; } // The Firebase Link
        public DateTime? IssuedDate { get; set; }
    }
}
