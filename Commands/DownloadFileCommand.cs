using TerraformCloudHelper;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerraformCloudHelper.Commands
{
    class DownloadFileCommand
    {
        public static void Execute(CommandLineApplication command)
        {
            //public async System.Threading.Tasks.Task GetFileContent(string path, string container,string localFolder)
            command.Description = "Download file from Azure Storage";
            command.HelpOption("-?|-h|--help");
            var container = command.Argument("container", "Container name");
            var remote = command.Argument("remote", "Remote file");
            var path = command.Argument("path", "Local path");
            var cs = command.Option("-c|--connectionstring", "Azure blob storage connection string", CommandOptionType.SingleValue);
            command.OnExecute(async () =>
            {
                string connectionString = cs.Value();
                var scs = new AzureStorage(connectionString);

                await scs.SaveFileContent(remote.Value, container.Value, path.Value);
                return 0;
            });
        }
    }
}
