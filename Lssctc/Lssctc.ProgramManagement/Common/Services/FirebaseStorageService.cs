using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Lssctc.ProgramManagement.Common.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration configuration)
        {
            // 1. Load credentials from the JSON file you downloaded
            // Ensure "firebase_config.json" is in your project root and copied to output
            var credential = GoogleCredential.FromFile("firebase_config.json");
            _storageClient = StorageClient.Create(credential);

            // 2. Get bucket name from appsettings.json
            _bucketName = configuration["Firebase:StorageBucket"]
                          ?? throw new ArgumentNullException("Firebase:StorageBucket configuration is missing");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Upload the object
            var dataObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                fileName,
                contentType,
                fileStream
            );

            // Make the object publicly readable (Optional: depends on your privacy needs)
            // If you want private files, you'd generate a SignedURL instead.
            // For public certificates, we often make the specific file public:
            // await _storageClient.UpdateObjectAsync(dataObject, new UpdateObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });

            // Construct the public URL (This format works for public Firebase objects)
            // Format: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{path}?alt=media
            var encodedName = Uri.EscapeDataString(fileName);
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedName}?alt=media";
        }
    }
}
