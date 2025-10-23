using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PlusCP.Classess
{
    public class AzureHelper
    {

        public static string GetBlobUrlWithSas(string fullBlobUrl)
        {
            if (string.IsNullOrEmpty(fullBlobUrl))
                return "";
            // Azure Blob Info
            DataTable dtAzure = cCommon.GetAzureSetting();
            string connectionString = "";
            string containerName = "";
            if (dtAzure.Rows.Count > 0)
            {
                connectionString = dtAzure.Rows[0]["Token"].ToString();
                containerName = dtAzure.Rows[0]["ContainerName"].ToString();
            }
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
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