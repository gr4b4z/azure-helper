using System;
using System.IO;
using TerraformCloudHelper.Commands;
using Microsoft.Extensions.CommandLineUtils;
using TerraformCloudHelper.Builders;

namespace TerraformCloudHelper.Commands
{
    class GetSqlConnectionStringCommand
    {
    
        public void Execute(CommandLineApplication command)
        {
            //public async System.Threading.Tasks.Task GetFileContent(string path, string container,string localFolder)
            command.Description = "Get connection string from terraform config";
            command.HelpOption("-?|-h|--help");

            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);
            var file = command.Option("-s|--save", "Save as file", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                var cstring = new AzureSqlConnectionStringBuilder().Build(tfstate.HasValue() ? tfstate.Value() : null, res.Value());
                if (file.HasValue())
                {
                    await File.WriteAllTextAsync(file.Value(), cstring);
                }
                else
                {
                    Console.WriteLine(cstring);
                }
                return 0;
            });
        }
    }
}
