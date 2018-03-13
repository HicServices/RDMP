using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace PluginPackager
{
    /// <summary>
    /// See obsolete message
    /// </summary>
    [Obsolete("This class was moved here out of unreferenced CommitAssembly.exe csproj")]
    public class PluginManifestChecker
    {

        public static int ProcessFile(ICatalogueRepository uploadTarget, FileInfo toCommit)
        {
            if (toCommit.Extension == ".zip")
            {
                string workingDirectory = Path.Combine(toCommit.DirectoryName, "PackageContents");

                ZipFile.ExtractToDirectory(toCommit.FullName, workingDirectory);
                try
                {
                    string manifest = Directory.GetFiles("PluginManifest.txt").SingleOrDefault();
                    if (manifest == null)
                        throw new FileNotFoundException("Could not find a file called PluginManifest.txt in the zip file " + toCommit.FullName);

                    var versionAsString = File.ReadAllLines(manifest).Single(l => l.Contains("CatalogueLibraryVersion")).Split(':')[1];
                    var version = new Version(versionAsString);
                    Version catalogueDatabaseVersion = uploadTarget.GetVersion();

                    if (!catalogueDatabaseVersion.Equals(version))
                        throw new Exception("The plugin was built against version " + version + " but the LIVE Catalogue Database is at version " + catalogueDatabaseVersion);

                    foreach (var file in Directory.GetFiles(workingDirectory, "*.dll"))
                        ProcessFile(uploadTarget, new FileInfo(file));
                }
                finally
                {
                    //make sure we always delete the working directory
                    Directory.Delete(workingDirectory, true);
                }
                return 0;
            }

            if (LoadModuleAssembly.IsDllProhibited(toCommit))
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
