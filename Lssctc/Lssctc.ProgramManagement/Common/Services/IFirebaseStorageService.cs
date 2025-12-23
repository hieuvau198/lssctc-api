namespace Lssctc.ProgramManagement.Common.Services
{
    public interface IFirebaseStorageService
    {
        /// <summary>
        /// Uploads a file stream to Firebase Storage and returns the public URL.
        /// </summary>
        /// <param name="fileStream">The file content stream</param>
        /// <param name="fileName">The destination file name (e.g., "certificates/cert_123.pdf")</param>
        /// <param name="contentType">Mime type (e.g., "application/pdf")</param>
        /// <returns>The public download URL</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteFileAsync(string fileUrl); // Add this method
    }
}
