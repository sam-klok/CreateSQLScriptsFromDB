using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.IO;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Extensions.Configuration;  // to read appsettings.json

namespace CreateSQLScriptsFromDB
{
    class Program
    {
        const string folder = "SqlScripts";
        const string list_of_stored_procs = "list of stored procedures.txt";
        const string list_of_views = "list of views.txt";

        readonly static ScriptingOptions scriptOptionsDrop = new ScriptingOptions()
        {
            ScriptDrops = true,
            IncludeIfNotExists = true,
            AnsiPadding = false,
        };

        readonly static ScriptingOptions scriptOptionsCreate = new ScriptingOptions()
        {
            //WithDependencies = true,  // will add create table before create view
            IncludeHeaders = false,  /****** Object:  View [dbo].[v_people]    Script Date: 9/30/2021 6:03:10 PM ******/
            ScriptSchema = true,
            ScriptData = false,
            Indexes = true,
            ClusteredIndexes = true,
            FullTextIndexes = true,

            // GO
            FileName = "test.sql",
            ScriptBatchTerminator = true,
            NoCommandTerminator = false,
            //ToFileOnly = true,  // it makes script empty
            AppendToFile = true,
            EnforceScriptingOptions = true,
            AllowSystemObjects = true,
            Permissions = true,
            DriAllConstraints = true,
            SchemaQualify = true,
            AnsiFile = true,
            //AnsiPadding = false,
        };

        static async Task Main(string[] args)
        {
            // reading configuration from appsettings.json file
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration configuration = configurationBuilder.Build();

            string serverName = configuration["DatabaseSettings:Server"];
            string databaseName = configuration["DatabaseSettings:Database"];

            ScriptDatabaseObjects(serverName, databaseName);
            //ScriptDatabaseObjectDemo(serverName, databaseName);

        }

        public static async void ScriptDatabaseObjects(string serverName, string databaseName)
        {
            PrepareOutputFolder();

            var server = new Server(serverName);
            var database = server.Databases[databaseName];

            await ScriptStoredProcedureObjectsToFiles(database);
            await ScriptViewObjectsToFiles(database);

        }

        private static void PrepareOutputFolder()
        {
            Directory.CreateDirectory(folder);
            DirectoryInfo di = new DirectoryInfo(folder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
        }

        private static async Task ScriptStoredProcedureObjectsToFiles(Database database) 
        {
            string[] lines = File.ReadAllLines(list_of_stored_procs);

            foreach (string dbObjectName in lines)
            {
                var script = new StringBuilder();

                foreach (StoredProcedure sp in database.StoredProcedures)
                {
                    if (sp.ToString() == dbObjectName)
                    {
                        Console.WriteLine(sp);

                        script.AppendLine("USE LifelongLearning");
                        script.AppendLine("GO");
                        script.AppendLine();

                        //StringCollection scripts = sp.Script(scriptOptionsDrop);
                        //foreach (string s in scripts)
                        //    script.AppendLine(s);
                        //script.AppendLine("GO");
                        //script.AppendLine();

                        /* Generating CREATE command */
                        StringCollection scripts = sp.Script(scriptOptionsCreate);
                        foreach (string s in scripts)
                        {
                            if (s.Contains("CREATE ", StringComparison.OrdinalIgnoreCase))
                            {
                                string s2 = s.Replace("CREATE ", "ALTER ", StringComparison.OrdinalIgnoreCase);
                                script.AppendLine(s2);
                            }
                            else
                            {
                                script.AppendLine(s);
                            }

                            // it's a hack because scripting of the "GO" not working well
                            if (s == "SET ANSI_NULLS ON" || s == "SET QUOTED_IDENTIFIER ON")
                                script.AppendLine("GO");
                        }

                        script.AppendLine("GO");

                        string fileName = dbObjectName + ".sql";
                        await File.WriteAllTextAsync(folder + "\\" + fileName, script.ToString());
                    }
                }
            }
        }

        private static async Task ScriptViewObjectsToFiles(Database database)

        {
            string[] lines = File.ReadAllLines(list_of_views);

            foreach (string dbObjectName in lines)
            {
                var script = new StringBuilder();

                foreach (View view in database.Views)
                {
                    if (view.ToString() == dbObjectName)
                    {
                        Console.WriteLine(view);

                        script.AppendLine("USE LifelongLearning");
                        script.AppendLine("GO");
                        script.AppendLine();

                        // 1st part of script
                        //StringCollection scripts = view.Script(scriptOptionsDrop);
                        //foreach (string s in scripts)
                        //    script.AppendLine(s);

                        //script.AppendLine("GO"); 
                        //script.AppendLine();

                        // 2nd part - Generating CREATE command 
                        StringCollection scripts = view.Script(scriptOptionsCreate);
                        foreach (string s in scripts)
                        {
                            if (s.Contains("CREATE ", StringComparison.OrdinalIgnoreCase))
                            {
                                string s2 = s.Replace("CREATE ", "ALTER ", StringComparison.OrdinalIgnoreCase);
                                script.AppendLine(s2);
                            }
                            else
                            {
                                script.AppendLine(s);
                            }

                            if (s == "SET ANSI_NULLS ON" || s == "SET QUOTED_IDENTIFIER ON")
                                script.AppendLine("GO");
                        }

                        script.AppendLine("GO");

                        string fileName = dbObjectName + ".sql";
                        await File.WriteAllTextAsync(folder + "\\" + fileName, script.ToString());
                    }
                }
            }
        }

        public static void ScriptDatabaseObjectDemo(string serverName, string databaseName)
        {
            var server = new Server(serverName);
            var database = server.Databases[databaseName];

            var scriptOptions = new ScriptingOptions() {
                ScriptDrops = true,
                IncludeIfNotExists = true,
                WithDependencies = true,
                IncludeHeaders = true,
                ScriptSchema = true,
                Indexes = true,
            };

            string script1 = "";
            string script2 = "";

            foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure sp in database.StoredProcedures)
            {
                if (sp.ToString() == "[dbo].[sp_get_people]")  // we need to provide full name
                {
                    Console.WriteLine(sp);

                    StringCollection scripts = sp.Script(scriptOptions);
                    foreach (string s in scripts)
                        script1 += s;

                    /* Generating CREATE TABLE command */
                    scripts = sp.Script();
                    foreach (string s in scripts)
                        script2 += s;

                    Console.WriteLine(script1);
                    Console.WriteLine(script2);
                }
            }

        }


    }
}
