using System;
using System.Linq;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace SQLScriptGeneratorApp
{
    class Program
    {
        static void Main()
        {
            var fileName = Path.GetFullPath(@"..\..\..\NorthwindBackup.sql");
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB; Database=Northwind; Integrated Security=true;";
            var databaseName = "Northwind";
            var schemaName = "dbo";

            if (File.Exists(fileName))
                File.Delete(fileName);

            try
            {
                var server    = new Smo.Server(new ServerConnection(new SqlConnection(connectionString)));
                var options   = new Smo.ScriptingOptions();
                var databases = server.Databases[databaseName];

                options.FileName                = fileName;
                options.EnforceScriptingOptions = true;
                options.WithDependencies        = true;
                options.IncludeHeaders          = true;
                options.ScriptDrops             = false;
                options.AppendToFile            = true;
                options.ScriptSchema            = true;
                options.ScriptData              = true;
                options.Indexes                 = true;

                var tables     = databases.Tables.Cast<Smo.Table>().Where(i => i.Schema == schemaName);
                var views      = databases.Views.Cast<Smo.View>().Where(i => i.Schema == schemaName);
                var procedures = databases.StoredProcedures.Cast<Smo.StoredProcedure>().Where(i => i.Schema == schemaName);

                Console.WriteLine("SQL Script Generator");

                Console.WriteLine("\nTable Scripts:");
                foreach (Smo.Table table in tables)
                {
                    databases.Tables[table.Name, schemaName].EnumScript(options);
                    Console.WriteLine(table.Name);
                }

                options.ScriptData       = false;
                options.WithDependencies = false;

                Console.WriteLine("\nView Scripts:");
                foreach (Smo.View view in views)
                {
                    databases.Views[view.Name, schemaName].Script(options);
                    Console.WriteLine(view.Name);
                }

                Console.WriteLine("\nStored Procedure Scripts:");
                foreach (Smo.StoredProcedure procedure in procedures)
                {
                    databases.StoredProcedures[procedure.Name, schemaName].Script(options);
                    Console.WriteLine(procedure.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured: {ex.Message}");
            }
        }
    }
}