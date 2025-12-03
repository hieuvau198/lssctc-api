using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Common.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration configuration)
        {
            // 1. Get bucket name
            _bucketName = configuration["Firebase:StorageBucket"]
                          ?? throw new ArgumentNullException("Firebase:StorageBucket configuration is missing");

            // 2. Get Credentials safely
            // PRIORITY: Check for Environment Variable (Azure) or AppSettings string
            string credentialJson = configuration["Firebase:CredentialJson"];

            // FALLBACK: If not found in config, try local file (Localhost development)
            if (string.IsNullOrEmpty(credentialJson) && File.Exists("firebase_config.json"))
            {
                credentialJson = File.ReadAllText("firebase_config.json");
            }

            if (string.IsNullOrEmpty(credentialJson))
            {
                throw new Exception("Firebase credentials not found. Set 'Firebase:CredentialJson' in AppSettings/Azure or place 'firebase_config.json' in root.");
            }

            // Create credential from the JSON STRING, not file
            var credential = GoogleCredential.FromJson(credentialJson);
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var dataObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                fileName,
                contentType,
                fileStream
            );

            var encodedName = Uri.EscapeDataString(fileName);
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedName}?alt=media";
        }
    }
}