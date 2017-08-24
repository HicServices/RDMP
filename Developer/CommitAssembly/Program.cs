using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CommitAssembly
{
    public class Program
    {
        /// <summary>
        /// Takes two args:
        /// args[0] - mandatory - path of the dll to commit
        /// args[1] - optional - connection string for the catalogue to use
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {

            if(args.Length == 0)
            {
                Console.WriteLine("Please provide the path to the dll to commit e.g. 'CommitAssembly.exe *.dll'");
                return 1;
            }

            CatalogueRepository uploadTarget;

            try
            {
                uploadTarget = GetUploadTarget();
            }
            catch (Exception)
            {
                Console.WriteLine("Check and update database configurations failed so aborting with code 0 so as not to freak anyone out");
                return 0;
            }
            
            //still null?
            if (uploadTarget == null)
                return 0;

            //@"c:\*.dll"

            //Path.GetDirectoryName()
            if (args[0].Contains("*"))
            {
                string directoryName = Path.GetDirectoryName(args[0]);
                string pattern = Path.GetFileName(args[0]);

                //if it is something like *.dll with no path name use current directory "." otherwise use the directory name
                DirectoryInfo d = string.IsNullOrWhiteSpace(directoryName) ? new DirectoryInfo(".") : new DirectoryInfo(directoryName);
                
                if (!d.Exists)
                {
                    Console.WriteLine("Directory does not exist:" + directoryName);
                    return 1;
                }
                else
                {
                    if (!d.EnumerateFiles(pattern).Any())
                    {
                        Console.WriteLine("Pattern did not match any files:" + pattern);
                        return 1;
                    }

                    int toReturn = 0;

                    foreach (FileInfo file in d.EnumerateFiles(pattern))
                    {
                        int returnOfThisOne = ProcessFile(uploadTarget, file);

                        if (returnOfThisOne != 0)
                            toReturn = returnOfThisOne;
                    }
                    return toReturn;
                }
            }
            else
            {
                var file = new FileInfo(args[0]);
                if (!file.Exists)
                {
                    Console.WriteLine("Could not find file " + args[0]);
                    return 3;
                }
                return ProcessFile(uploadTarget,file);
            }
        }

        private static CatalogueRepository GetUploadTarget()
        {
            throw new Exception("Fix this once we've figured out the dependencies of the below objects and determined where things should live."); 
            /*
            var registry = new RegistryRepositoryFinder();


            if (registry.KeyExists && registry.CatalogueRepository != null)
                return registry.CatalogueRepository;
            else
            {
                Console.WriteLine(
                    "Registry did not contain connection settings so uploading to DatabaseTest target instead (unit test database)");

                try
                {
                    DatabaseTests testDatabase = new DatabaseTests();
                    testDatabase.SetUpConnectionConfiguration();

                    return testDatabase.CatalogueRepository;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to get legit values from DatabaseTests, perhaps you have not properly configured Tests.Common.dll.config in Tests.Common.csproj?");
                    Console.WriteLine("Because we cannot access the server, I will nuke the registry settings because they are clearly wrong:");
                    throw;
                }
            }*/
        }

        private static int ProcessFile(CatalogueRepository uploadTarget, FileInfo toCommit)
        {
            if (toCommit.Extension == ".zip")
            {
                string workingDirectory = Path.Combine(toCommit.DirectoryName, "PackageContents");
                
                ZipFile.ExtractToDirectory(toCommit.FullName,workingDirectory);
                try
                {
                    string manifest = Directory.GetFiles("PluginManifest.txt").SingleOrDefault();
                    if(manifest == null)
                        throw new FileNotFoundException("Could not find a file called PluginManifest.txt in the zip file "+toCommit.FullName);

                    var versionAsString = File.ReadAllLines(manifest).Single(l => l.Contains("CatalogueLibraryVersion")).Split(':')[1];
                    var version = new Version(versionAsString);
                    Version catalogueDatabaseVersion = uploadTarget.GetVersion();

                    if(!catalogueDatabaseVersion.Equals(version))
                        throw new Exception("The plugin was built against version " + version + " but the LIVE Catalogue Database is at version " + catalogueDatabaseVersion );

                    foreach (var file in Directory.GetFiles(workingDirectory,"*.dll"))
                        ProcessFile(uploadTarget,new FileInfo(file));
                }
                finally
                {
                    //make sure we always delete the working directory
                    Directory.Delete(workingDirectory, true);
                }
                return 0;
            }
            
            if(LoadModuleAssembly.IsDllProhibited(toCommit))
                return 0;

            // No CatalogueConnectionString configured
            if (uploadTarget == null)
                return 4;

            // For now we don't want to exit with an error if the database isn't available because this messes up post-build scripts
            // on the test server (and requiring a database in order to successfully build isn't particularly nice anyway)
            try
            {
                DbConnection con = new SqlConnection(uploadTarget.ConnectionString);
                con.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }

            try
            {
                throw new NotImplementedException();
                //var lma = new LoadModuleAssembly(uploadTarget, toCommit, true);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 2;
            }
        }
    }
}
