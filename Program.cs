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

        private static string GetStateFileContent(string filepath)
        {
            if (filepath.EndsWith('/') || filepath.EndsWith('\\'))
            {
                filepath += "terraform.tfstate";
            }
            return File.ReadAllText(filepath);
        }
        private static void UploadAzureFunctionCommand(CommandLineApplication command)
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

                if (tfstate.HasValue())
                {
                    var storage = new StateReader(GetStateFileContent(tfstate.Value()));
                    (_name, _user, _pwd) = storage.GetFunctionAppCredentials(res.Value());
                }
                else
                {
                    (_name, _user, _pwd) = (appname.Value(), user.Value(), pwd.Value());
                }
                await new AppDeployer().Upload(_user, _pwd, _name, path.Value);


                return 0;
            });
        }
        private static void DownloadFileCommand(CommandLineApplication command)
        {
            //public async System.Threading.Tasks.Task GetFileContent(string path, string container,string localFolder)
            command.Description = "Download file";
            command.HelpOption("-?|-h|--help");
            var container = command.Argument("container", "Container name");
            var remote = command.Argument("remote", "Remote file");
            var path = command.Argument("path", "Local path");
            var cs = command.Option("-c|--connectionstring", "Azure blob storage connection string", CommandOptionType.SingleValue);
            command.OnExecute(async () =>
            {
                string connectionString = cs.Value();
                var scs = new Storage(connectionString);

                await scs.GetFileContent(remote.Value, container.Value, path.Value);
                return 0;
            });
        }

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


          
            app.Command("download", DownloadFileCommand);
            app.Command("upload", (command) =>
            {
                command.Command("app", UploadAzureFunctionCommand);
                command.Command("folder", UploadFolderCommand);
            });

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
                        var state = new StateReader(GetStateFileContent(tfstate.Value()));
                        name = state.GetResourceName(res.Value());
                        key = state.GetResourceAccessKey(res.Value());
                    }

                    await new GenerateWebsite().GenerateAsync(name, key);
                    return 0;
                });
            });



            



            app.Execute(args);


        }

       
        private static void UploadFolderCommand(CommandLineApplication command)
        {
            command.Description = "Uploads folder to Azure storage";
            command.HelpOption("-?|-h|--help");
            var container = command.Argument("container", "Container name");
            var path = command.Argument("path", "Folder path");

            var cs = command.Option("-c|--connectionstring", "Azure blob storage connection string", CommandOptionType.SingleValue);
            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);
            var labels = command.Option("-m|--labels", "Tag files with key=value paris", CommandOptionType.MultipleValue);

            command.OnExecute(async () =>
            {
                string connectionString = GetConnectionString(cs, tfstate, res);
                var scs = new Storage(connectionString);
                var lbl = labels.Values.Select(v => v.Split('=')).ToDictionary(a => a[0], b => b[1]);
                await scs.PushFolderAsync(path.Value, container.Value, false, lbl);
                return 0;
            });
        }

        private static string GetConnectionString(CommandOption cs, CommandOption tfstate, CommandOption res)
        {
            string connectionString = null;
            if (tfstate.HasValue())
            {
                if (!res.HasValue())
                    throw new Exception("When using state file, resource key has to be provided");
                var storage = new StateReader(GetStateFileContent(tfstate.Value()));
                connectionString = storage.GetBlobConnectionString(res.Value());
            }
            else
            {
                connectionString = cs.Value();
            }

            return connectionString;
        }
    }
}
