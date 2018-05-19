using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    public class Storage
    {
        CloudStorageAccount storageAccount;
        public Storage(string storageConenctionString)
        {
            storageAccount = CloudStorageAccount.Parse(storageConenctionString);
        }
        private async System.Threading.Tasks.Task<string>
            UploadFileAsync(string file, string path, CloudBlobContainer cloudBlobContainer, Dictionary<string, string> labels = null)
        {
            string cloudfilename = file.Replace(path, "").Replace('\\', '/');
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(cloudfilename);

            await blob.UploadFromFileAsync(file);
            if (labels != null)
            {
                foreach (var label in labels)
                {
                    blob.Metadata[label.Key] = label.Value;
                }
            }
            await blob.SetMetadataAsync();
            return cloudfilename;
        }
        public async System.Threading.Tasks.Task PushFolderAsync(string path, string container, bool clear, Dictionary<string, string> labels)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(container);
            await cloudBlobContainer.CreateIfNotExistsAsync();
            var uploadedFiles = new List<string>();
            foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {

                uploadedFiles.Add(await UploadFileAsync(file, path, cloudBlobContainer, labels));
            }

            if (clear)
            {
                BlobContinuationToken blobContinuationToken = null;

                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {

                        var r = cloudBlobContainer.GetBlockBlobReference(item.Uri.ToString());
                        await r.DeleteAsync();


                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null);
            }
        }


        public async System.Threading.Tasks.Task GetFileContent(string remotefilename, string container, string localFolder)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(container);
            if (localFolder.EndsWith("\\") || localFolder.EndsWith("/"))
            {
                localFolder += remotefilename;
            }
            if (await cloudBlobContainer.ExistsAsync())
            {
                var r = cloudBlobContainer.GetBlockBlobReference(remotefilename);
                await r.DownloadToFileAsync(localFolder, FileMode.CreateNew);
            }
        }




    }
}
