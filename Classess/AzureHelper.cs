using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlusCP.Classess
{
    public class AzureHelper
    {
        private static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=mammoth777;AccountKey=UL0iuqNp87dTbp8puyJ5lPyTVaZGxh34TGeH+Bvl+UpYXaygDACNlCrbiOhf54rJ3HNA4/9pjboW+AStGpLtuA==;EndpointSuffix=core.windows.net";
        private static string containerName = "mammothfiles"; // e.g. "documents"

        public static string GetBlobUrlWithSas(string fullBlobUrl)
        {
            if (string.IsNullOrEmpty(fullBlobUrl))
                return "";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Extract blob name from full URL
            string blobName = GetBlobNameFromUrl(fullBlobUrl);

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            // Generate a SAS token (valid for 10 minutes)
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(10),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasToken = blob.GetSharedAccessSignature(sasConstraints);

            return blob.Uri + sasToken;
        }

        private static string GetBlobNameFromUrl(string fullUrl)
        {
            Uri uri = new Uri(fullUrl);
            return uri.Segments[uri.Segments.Length - 1]; // returns the last segment (blob name)
        }
    }


}