using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class StateReader
    {
        JObject root;
        public StateReader(string content)
        {
            root = JObject.Parse(content);

        }
        public string GetBlobConnectionString(string terraformResourceId, bool useSecondary = false)
        {
            var attributes = root["modules"][0]["resources"]
                [terraformResourceId]["primary"]["attributes"];
            var resources = attributes["primary_blob_connection_string"];
            var cs = resources.Value<string>(); ;
            if (useSecondary)
            {
                var primaryKey = attributes["primary_access_key"].Value<string>();
                var secondaryKey = attributes["secondary_access_key"].Value<string>();
                cs = cs.Replace(primaryKey, secondaryKey);
            }
            return cs;
        }

        public string GetResourceAccessKey(string terraformResourceId, bool useSecondary = false)
        {
            var attributes = root["modules"][0]["resources"]
                [terraformResourceId]["primary"]["attributes"];
            var resources = attributes["primary_access_key"];
            return useSecondary ? attributes["secondary_access_key"].Value<string>() : attributes["primary_access_key"].Value<string>();
        }

        public string GetResourceName(string terraformResourceId)
        {
            var attributes = root["modules"][0]["resources"]
                [terraformResourceId]["primary"]["attributes"];
            var resources = attributes["name"];
            var cs = resources.Value<string>(); ;
         
            return cs;
        }

        internal (string name, string user, string password) GetFunctionAppCredentials(string terraformResourceId)
        {
            var attributes = root["modules"][0]["resources"]
                [terraformResourceId]["primary"]["attributes"];
            var pwd = attributes["site_credential.0.password"].Value<string>();
            var user = attributes["site_credential.0.username"].Value<string>();
            var name = attributes["name"].Value<string>();
            return (name, user, pwd);
        }
    }
}
