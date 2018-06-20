using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureHelper.Commands
{

    class UploadAzureFunctionCommand
    {
        public  void Execute(CommandLineApplication command)
        {
            command.Description = "Uploads zip function app to Azure";
            command.HelpOption("-?|-h|--help");
            var path = command.Argument("path", "Zip path");
            var user = command.Option("-u|--user", "User name", CommandOptionType.SingleValue);
            var pwd = command.Option("-p|--password", "Password", CommandOptionType.SingleValue);
            var appname = command.Option("-a|--appurl", "Function name", CommandOptionType.SingleValue);


            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                string _user = null;
                string _pwd = null;
                string _name = null;

                if (res.HasValue())
                {
                    var tfFile = tfstate.HasValue() ? tfstate.Value() : null;
                    var storage = new TerraformState(new StateFileContent(tfFile));
                    (_name, _user, _pwd) = storage.GetFunctionAppCredentials(res.Value());
                }
                else
                {
                    (_name, _user, _pwd) = (appname.Value(), user.Value(), pwd.Value());
                }

                Console.WriteLine("User:" + _user);
                Console.WriteLine("Name:" + _name);
                await new AzureFunctionDeployer().Upload(_user, _pwd, _name, path.Value);


                return 0;
            });
        }

    }
}
