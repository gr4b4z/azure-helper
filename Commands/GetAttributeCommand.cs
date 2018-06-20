using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace AzureHelper.Commands
{
    class GetAttributeCommand
    {
        public void Execute(CommandLineApplication command)
        {
            //public async System.Threading.Tasks.Task GetFileContent(string path, string container,string localFolder)
            command.Description = "Get attribute from terraform config";
            command.HelpOption("-?|-h|--help");
            var attribute = command.Argument("attribute", "Attribute from terraform resource");

            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);
            var file = command.Option("-s|--save", "Save as file", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {

                var state = new TerraformState(new StateFileContent(tfstate.HasValue() ? tfstate.Value() : null));
                var attributeValue = attribute.Value;
                if (res.HasValue())
                {
                    attributeValue = res.HasValue() + attribute.Value;
                }
                var content = state.GetResourceByPath(attributeValue);
                if (file.HasValue())
                {
                    await File.WriteAllTextAsync(file.Value(), content);
                }
                else
                {
                    Console.WriteLine(content);
                }
                return 0;
            });
        }
    }
}
