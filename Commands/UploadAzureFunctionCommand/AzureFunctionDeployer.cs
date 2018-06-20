using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class AzureFunctionDeployer
    {
        private string siteurl = "https://[FUNCTION_NAME].scm.azurewebsites.net/api/zipdeploy";
        public Task<HttpResponseMessage> Upload(string user, string password, string appUrl, string zipPath)
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
            return client.PostAsync(appUrl, content);

        }
    }
}
