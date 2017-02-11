using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types


namespace BlobStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            // 01 - Connect to your azure storage account

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            Console.WriteLine("Connected to Account");

            // 02 - Create a container called "text-files"

            CloudBlobContainer container = blobClient.GetContainerReference("text-files");
            container.CreateIfNotExists();
            Console.WriteLine("Created Container");

            // 03 - Set the container permissions to BlobContainerPublicAccessType.Blob

            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            container.SetPermissions(permissions);
            Console.WriteLine("Permission Changed");

            // 04 - Upload SampleText.txt to a block block called "UploadedSampleText.txt"

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("UploadedSampleText.txt");
            using (var fileStream = System.IO.File.OpenRead(@"C:\Users\Student\Desktop\homework1-master\source\BlobStorage\SampleText.txt"))
            {
                blockBlob.UploadFromStream(fileStream);
            }
            Console.WriteLine("Uploaded file");

            // 05 - Download "UploadedSampleText.txt" from the storage account and prints its contents using Console.WriteLine()

            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference("UploadedSampleText.txt");

            string text;
            using (var memoryStream = new MemoryStream())
            {
                blockBlob2.DownloadToStream(memoryStream);
                text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                Console.WriteLine("File:");
                Console.WriteLine(text);
            }

            // 05 - Delete UploadedSampleText.txt from the storage

            blockBlob.Delete();
            Console.WriteLine("Deleted Blob");
            Console.ReadLine();

        }
    }
}
