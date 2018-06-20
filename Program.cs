using AzureHelper.Commands;
using AzureHelper.Logic;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace ConsoleApp1
{
    class Program
    {


        static void Main(string[] args)
        {

            var app = new CommandLineApplication
            {
                Name = "Azure Helper",
            };

            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                app.ShowHint();
                return 0;
            });



            app.Command("download", DownloadFileCommand.Execute);
            app.Command("upload", (command) =>
            {
                command.Description = "Upload app or folder";
                command.HelpOption("-?|-h|--help");

                command.Command("app", new UploadAzureFunctionCommand().Execute);
                command.Command("folder", new UploadFolderCommand().Execute);

            });
            app.Command("tf", (command) =>
            {
                command.Description = "Get terraform data";
                command.HelpOption("-?|-h|--help");
                command.Command("attribute", new GetAttributeCommand().Execute);
                command.Command("cs", new GetSqlConnectionStringCommand().Execute);
                command.Command("replace", new ReplaceAttributeCommand().Execute);

            });

            app.Command("sql", new MsSqlQueryCommand().Execute);


            app.Command("setsite", (command) =>
            {
                command.Description = "Set azure website temporary solution";
                command.HelpOption("-?|-h|--help");

                var cs = command.Option("-c|--connectionstring", "Azure blob storage connection string", CommandOptionType.SingleValue);

                var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
                var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);

                command.OnExecute(async () =>
                {
                    string name = null;
                    string key = null;
                    if (cs.HasValue())
                    {
                        var cstr = cs.Value()
                                        .Split(';')
                                        .Select(e => e.Split('=', 2))
                                        .ToDictionary(e => e[0].ToLower(), v => v[1]);
                        name = cstr["accountname"];
                        key = cstr["accountkey"];
                    }
                    else
                    {
                        var state = new TerraformState(new StateFileContent(tfstate.HasValue() ? tfstate.Value() : null));
                        name = state.GetResourceName(res.Value());
                        key = state.GetResourceByPath(res.Value()+ ".primary_access_key");
                    }

                    await new WebsiteCreator().GenerateAsync(name, key);
                    return 0;
                });
            });







            app.Execute(args);


        }


   

    
    }
}
