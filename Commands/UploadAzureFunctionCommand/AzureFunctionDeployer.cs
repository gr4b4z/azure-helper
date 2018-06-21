using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    public class AzureFunctionDeployerWithState:AzureFunctionDeployer
    {
        private readonly AzureStorage azureStorage;
        private string container;
        private readonly bool force;
        private  string checksum;


        public AzureFunctionDeployerWithState(AzureStorage azureStorage, string container, bool force)
        {
            this.azureStorage = azureStorage;
            this.container = container??"azure_function_checksum";
            this.force = force;
        }
        public string GetNormalizedContainerName(string container)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
           return rgx.Replace(container, "").ToLower();
        }
        public override async Task<bool> UploadAsync(string user, string password, string appName, string zipPath)
        {
            container = GetNormalizedContainerName(container);
            var lastVersionCheckSum = await azureStorage.GetFileContent(appName, container);
            var currentVersionChecksum = GenerateCheckSum(zipPath);
            if(currentVersionChecksum == lastVersionCheckSum)
            {
                Console.WriteLine("Function already has been uploaded");
                if (!force)
                {
                    return false;
                }
            }

            Console.WriteLine("Uploading function "+appName);
            await base.UploadAsync(user, password, appName, zipPath);
            await azureStorage.UploadContent(checksum, appName, container);
            Console.WriteLine("Function has been uploaded " + appName);
            return true;
            
        }
 
        private string GenerateCheckSum(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                var base64String = Convert.ToBase64String(hash);
                checksum =  base64String;
            }
            return checksum;
        }

    }
    public class AzureFunctionDeployer
    {
        private string siteurl = "https://[FUNCTION_NAME].scm.azurewebsites.net/api/zipdeploy";
        public virtual async Task<bool> UploadAsync(string user, string password, string appUrl, string zipPath)
        {
            if (!appUrl.Contains("https://"))
            {
                appUrl = siteurl.Replace("[FUNCTION_NAME]", appUrl);
            }

            Console.WriteLine("Uploading to :" + appUrl);

            var base64AuthInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(($"{user}:{password}")));
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization
                = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64AuthInfo);
            MemoryStream responseStream = new MemoryStream();
            Stream fileStream = System.IO.File.Open(zipPath, FileMode.Open);
            fileStream.CopyTo(responseStream);
            fileStream.Close();
            responseStream.Position = 0;
            HttpContent content = new StreamContent(responseStream);
            var result =  await client.PostAsync(appUrl, content);
            return result.IsSuccessStatusCode;

        }
    }
}
