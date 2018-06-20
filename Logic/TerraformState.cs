using System;
using System.Collections.Generic;
using System.Linq;
using AzureHelper.Commands;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class TerraformState
    {
        IDictionary<string, JObject> terraformResources = new Dictionary<string, JObject>();
        IDictionary<string, string> outputResources = new Dictionary<string, string>();

        public TerraformState(StateFileContent stateFileContent)
        {

            foreach (var content in stateFileContent)
            {
                var jobject = JObject.Parse(content);

                var zz = jobject.SelectTokens("$..resources");
                var ff = zz.Children();
                foreach (var item in ff)
                {
                    var t = (JProperty)item;
                    if(!t.Name.Contains("data.terraform_remote_state"))
                    terraformResources.Add(t.Name, (JObject)t.First);
                }


                var outputs = jobject.SelectTokens("$..outputs");
                foreach (var item in outputs.Children())
                {
                    var t = (JProperty)item;
                    outputResources.Add(t.Name, t.First["value"].Value<string>());
                    
                }

            }
        }

        public string GetResourceByPath(string path)
        {
            var d = path.Split('.');
            if (d[0]== "output")
            {
                return outputResources[string.Join('.', d.Skip(1))];
            }
            
            var resourceName = string.Join('.', d.Take(2));
            var attributeName = string.Join('.', d.Skip(2));
            try
            {
                var resources = terraformResources[resourceName]["primary"];
                if(resourceName != "id")
                {
                    resources = resources["attributes"];
                }

                resources = resources[attributeName];

                //attributes
                var cs = resources.Value<string>();
                return cs;
            }
            catch
            {
                Console.WriteLine($"Provided key = {path} doesn't exists");
                throw;
            }
        }


        public string[] GetDependencies(string terraformResourceId)
        {
            try
            {
                var dependencies = terraformResources
                    [terraformResourceId]["depends_on"];
                return ((JArray) dependencies).Select(j=>j.Value<string>()).ToArray();
            }
            catch 
            {
                Console.WriteLine($"Provided key = {terraformResourceId} doesn't exists");
                throw;
            }
        }
        public string GetBlobConnectionString(string terraformResourceId, bool useSecondary = false)
        {
            try
            {
                var attributes = terraformResources
                    [terraformResourceId]["primary"]["attributes"];
                var resources = attributes["primary_blob_connection_string"];
                var cs = resources.Value<string>();
                ;
                if (useSecondary)
                {
                    var primaryKey = attributes["primary_access_key"].Value<string>();
                    var secondaryKey = attributes["secondary_access_key"].Value<string>();
                    cs = cs.Replace(primaryKey, secondaryKey);
                }

                return cs;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Provided key = {terraformResourceId} doesn't exists");
                throw;
            }
            
        }

        

        public string GetResourceName(string terraformResourceId)
        {
            try
            {
                
            var attributes = terraformResources
                [terraformResourceId]["primary"]["attributes"];
            var resources = attributes["name"];
            var cs = resources.Value<string>(); ;
         
            return cs;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Provided key = {terraformResourceId} doesn't exists");
                throw;
            }

        }

        internal (string name, string user, string password) GetFunctionAppCredentials(string terraformResourceId)
        {
            try { 
            var attributes = terraformResources
                [terraformResourceId]["primary"]["attributes"];
            var pwd = attributes["site_credential.0.password"].Value<string>();
            var user = attributes["site_credential.0.username"].Value<string>();
            var name = attributes["name"].Value<string>();
            return (name, user, pwd);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Provided key = {terraformResourceId} doesn't exists");
                throw;
            }

        }
    }
}
