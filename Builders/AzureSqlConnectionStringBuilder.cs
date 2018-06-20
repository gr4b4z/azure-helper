using ConsoleApp1;
using System.Linq;

namespace AzureHelper.Commands
{
    class AzureSqlConnectionStringBuilder
    {
        public string Build(string tfstate, string res)
        {
            var state = new TerraformState(new StateFileContent(tfstate));
            var sqlServerResourceId = state.GetDependencies(res).First(z=>z.Contains("azurerm_sql_server"));
            var server = state.GetResourceByPath(sqlServerResourceId+".fully_qualified_domain_name");
            var user = state.GetResourceByPath(sqlServerResourceId + ".administrator_login");
            var pwd = state.GetResourceByPath(sqlServerResourceId + ".administrator_login_password");
            var databaseName = state.GetResourceByPath(res + ".name");

            return $"Server=tcp:{server},1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={user};Password={pwd};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }
    }
}
