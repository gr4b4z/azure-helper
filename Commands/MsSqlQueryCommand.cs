using ConsoleApp1;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureHelper.Commands
{
    class MsSqlQueryCommand
    {
        public  void Execute(CommandLineApplication command)
        {
            command.Description = "Extecute sql query";
            command.HelpOption("-?|-h|--help");
            var sqlFileToExecute = command.Argument("sql", "Sql file to execute");

            var cs = command.Option("-c|--connectionstring", "Connection string", CommandOptionType.SingleValue);

            var tfstate = command.Option("-t|--tfstate", "Teraform state file", CommandOptionType.SingleValue);
            var res = command.Option("-k|--resourcekey", "Teraform database key", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                string connectionString = null;
                if (!cs.HasValue())
                {
                    connectionString =
                        new AzureSqlConnectionStringBuilder().Build(tfstate.HasValue() ? tfstate.Value() : null, res.Value());

                }
                else
                {
                    connectionString = cs.Value();
                }
                await new MsSqlExecutor(connectionString).ExecuteSqlAsync(File.ReadAllText(sqlFileToExecute.Value));
                return 0;
            });
        }
    }
}
