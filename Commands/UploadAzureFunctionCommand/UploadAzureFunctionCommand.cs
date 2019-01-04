using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureHelper.Commands
{
    public static class CommandExtension {
        public static string GetValueOrNull(this CommandOption  option){
            return option.HasValue() ? option.Value() : null;
        }
    }
    class UploadAzureFunctionCommand
    {
        public void Execute(CommandLineApplication command)
        {
            command.Description = "Uploads zip function app to Azure";
            command.HelpOption("-?|-h|--help");
            var path = command.Argument("path", "File location");
            var user = command.Option("-u|--user", "Azure user name", CommandOptionType.SingleValue);
            var pwd = command.Option("-p|--password", "Azure user password", CommandOptionType.SingleValue);
            var appname = command.Option("-a|--name", "Function name", CommandOptionType.SingleValue);

            var forceUpload = command.Option("-f|--force", "Upload even if the same function exits", CommandOptionType.NoValue);
            var storageConnectionStringParam = command.Option("--scstring", "Azure storage connection string for state", CommandOptionType.SingleValue);
            var storageRes = command.Option("--sresourcekey", "TF Resource key containing azure storage connection string ", CommandOptionType.SingleValue);
            var container = command.Option("--container", "Remote container name", CommandOptionType.SingleValue);

            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                string _user = null;
                string _pwd = null;
                string _name = null;
                string _storageConenctionString = null;
                TerraformState storage = null;

                if (res.HasValue() || storageRes.HasValue())
                {
                    storage = new TerraformState(new StateFileContent(tfstate.HasValue() ? tfstate.Value() : null));
                }

                if (res.HasValue())
                {

                    (_name, _user, _pwd) = storage.GetFunctionAppCredentials(res.Value());
                }
                else
                {
                    (_name, _user, _pwd) = (appname.Value(), user.Value(), pwd.Value());
                }

                if (storageRes.HasValue())
                {
                    _storageConenctionString = storage.GetResourceByPath(storageRes.Value());
                }
                else if (storageConnectionStringParam.HasValue())
                {
                    _storageConenctionString = storageConnectionStringParam.Value();
                }


                Console.WriteLine("User:" + _user);
                Console.WriteLine("Name:" + _name);


                AzureFunctionDeployer functionDeployer;
                if (_storageConenctionString == null)
                {
                    functionDeployer =new AzureFunctionDeployer();
                }
                else
                {
                    functionDeployer = new AzureFunctionDeployerWithState(
                      new AzureStorage(_storageConenctionString),
                      container.GetValueOrNull(),
                      forceUpload.HasValue()
              );
                }

                await functionDeployer.UploadAsync(_user, _pwd, _name, path.Value);

                return 0;
            });
        }

    }
}
