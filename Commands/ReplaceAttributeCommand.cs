using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AzureHelper.Commands
{
    class ReplaceAttributeCommand
    {
        public void Execute(CommandLineApplication command)
        {
            //public async System.Threading.Tasks.Task GetFileContent(string path, string container,string localFolder)
            command.Description = "Replace wildcards in files with attributes from trerraform config. " +
                "All @#terraform.azurerm_function_app.test.storage_connection_string strings will be replaced with azurerm_function_app.test.storage_connection_string value taken fom terraform state file";

            command.HelpOption("-?|-h|--help");
            var files    = command.Argument("file", "Files to replace tags",multipleValues:true);
            var tfperfix = command.Option("-g|--tag", "Specify tag prefix if different than default @#terraform", CommandOptionType.SingleValue);
            var pattern  = command.Option("-p|--pattern", "Search pattern", CommandOptionType.SingleValue);
            var tfstate  = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res      = command.Option("-k|--resourcekey", "Teraform resource key file", CommandOptionType.SingleValue);


            command.OnExecute(() =>
            {
                var prefix = "@#terraform.";
                if (tfperfix.HasValue())
                {
                    prefix = tfperfix.Value();
                }
                var state = new TerraformState(new StateFileContent(tfstate.HasValue() ? tfstate.Value() : null));
                var fi = new List<string>();
                
                foreach (var item in files.Values.Select(e=> e.TrimEnd('/', '\\')))
                {
                    if (Directory.Exists(item))
                    {
                        if (pattern.HasValue())
                        {
                            pattern.Value().Split('|').ToList()
                            .ForEach(p =>
                            {
                                var f = Directory.EnumerateFiles(item, p, new EnumerationOptions { RecurseSubdirectories = true });
                                fi.AddRange(f);
                            });
                           
                            
                        }
                        else
                        {
                            var f = Directory.EnumerateFiles(item,"*.*", new EnumerationOptions { RecurseSubdirectories = true });
                            fi.AddRange(f);
                        }
                        
                    }
                    else
                    {
                        fi.Add(item);
                    }
                }
                fi.ForEach(e => Replace(e, state, prefix));
                return 0;
            });
        }

        public void Replace(string path, TerraformState state,string tag)
        {
            var file = File.ReadAllText(path);
            var matches = Regex.Matches(file, tag+"([0-9a-z_.]*)");
            var replacements = new List<Tuple<string, string>>();
            if (matches.Count == 0) return;
            Console.WriteLine("Replacing " + path);
            foreach (Match item in matches)
            {
                var itemToReplace = item.Groups[1].Value;
                var res = state.GetResourceByPath(itemToReplace);
                replacements.Add(new Tuple<string, string>(tag + itemToReplace, res));
            }
            foreach(var rep in replacements)
            {
                file = file.Replace(rep.Item1, rep.Item2);
            }
            File.WriteAllText(path, file);
        }

    }
}
