using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;

namespace AzureHelper.Commands
{
    class UploadFolderCommand
    {
        private  string GetConnectionString(CommandOption cs, CommandOption tfstate, CommandOption res)
        {
            string connectionString = null;
            if (res.HasValue())
            {
                var tfFile = tfstate.HasValue() ? tfstate.Value() : null;
                var storage = new TerraformState(new StateFileContent(tfFile));
                connectionString = storage.GetBlobConnectionString(res.Value());
            }
            else
            {
                connectionString = cs.Value();
            }
            return connectionString;
        }
        public void Execute(CommandLineApplication command)
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
                var scs = new AzureStorage(connectionString);
                var lbl = labels.Values.Select(v => v.Split('=')).ToDictionary(a => a[0], b => b[1]);
                await scs.PushFolderAsync(path.Value, container.Value, false, lbl);
                return 0;
            });
        }
    }
}
