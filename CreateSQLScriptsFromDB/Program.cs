using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.IO;
using System.Collections.Specialized;
using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.Management.Smo;

namespace CreateSQLScriptsFromDB
{
    class Program
    {
        const string folder = "SqlScripts";
        const string list_of_stored_procs = "list of stored procedures.txt";
        const string list_of_views = "list of views.txt";

        static async Task Main(string[] args)
        {
            //using IHost host = CreateHostBuilder(args).Build();
            // Application code should start here.

            ScriptDatabaseObjects();

            //ScriptDatabaseObjectDemo();
            //await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);


        public static async void ScriptDatabaseObjects()
        {
            var server = new Server("localhost");
            var databse = server.Databases["LifelongLearning"];

            var scriptOptionsDrop = new ScriptingOptions()
            {
                ScriptDrops = true,
                IncludeIfNotExists = true,
                AnsiPadding = false,

                //WithDependencies = true,
                //IncludeHeaders = true,
                //ScriptSchema = true,
                //Indexes = true,
                //ClusteredIndexes = true,

                //FileName = "test.sql",
                //ScriptBatchTerminator = true,
                //NoCommandTerminator = false,
                //ToFileOnly = false,
                //AppendToFile = true,
                //EnforceScriptingOptions = true,

                //Default = true,
                //DriAll = true,
            };

            var scriptOptionsCreate = new ScriptingOptions()
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

            PrepareOutputFolder();

            await ScriptStoredProcedureObjectsToFiles(databse, scriptOptionsDrop, scriptOptionsCreate);
            await ScriptViewObjectsToFiles(databse, scriptOptionsDrop, scriptOptionsCreate);

        }

        private static void PrepareOutputFolder()
        {
            Directory.CreateDirectory(folder);
            DirectoryInfo di = new DirectoryInfo(folder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
        }

        private static async Task ScriptStoredProcedureObjectsToFiles(
            Database database, 
            ScriptingOptions scriptOptionsDrop, 
            ScriptingOptions scriptOptionsCreate)
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

        private static async Task ScriptViewObjectsToFiles(
            Database database,
            ScriptingOptions scriptOptionsDrop,
            ScriptingOptions scriptOptionsCreate)

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

        public static void ScriptDatabaseObjectDemo()
        {
            //var sb = new StringBuilder();

            var server = new Server("localhost");
            var databse = server.Databases["LifelongLearning"];

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

            foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure sp in databse.StoredProcedures)
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
