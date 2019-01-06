using StorageRestApiAuth;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace TerraformCloudHelper.Logic
{
    class WebsiteCreator
    {
        public async System.Threading.Tasks.Task GenerateAsync(string storageAccountName, string storageAccountKey)
        {
            var xmlRequest = @"<?xml version=""1.0"" encoding=""utf-8""?><StorageServiceProperties><StaticWebsite><Enabled>true</Enabled><IndexDocument>index.html</IndexDocument><ErrorDocument404Path>error.html</ErrorDocument404Path></StaticWebsite></StorageServiceProperties>
";
            String uri = string.Format("http://{0}.blob.core.windows.net?restype=service&comp=properties", storageAccountName);
            Byte[] requestPayload = Encoding.UTF8.GetBytes(xmlRequest);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", "2018-03-28");

                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);


                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage))
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
                    {
                        Console.WriteLine("Website has been created");
                    }
                    else
                    {
                        Console.WriteLine("Errors "+ await httpRequestMessage.Content.ReadAsStringAsync());
                    }
                }
            }
        }

    }
}

