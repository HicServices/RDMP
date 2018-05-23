using System;
using System.Data.SqlClient;
using System.IO;
using CatalogueLibrary.Repositories;
using CommandLine;
using RDMPStartup.PluginManagement;
using ReusableLibraryCode.Checks;

namespace PluginPackager
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<PluginPackagerProgramOptions>(args).MapResult(RunOptionsAndReturnExitCode, errs => 1);
        }

        private static int RunOptionsAndReturnExitCode(PluginPackagerProgramOptions opts)
        {
            FileInfo f = new FileInfo(opts.SolutionFile);
            if (!f.Exists)
            {
                Console.WriteLine("Could not find Solution File");
                return -58;
            }

            Packager p = new Packager(f, opts.ZipFileName, opts.SkipSourceCodeCollection);
            p.PackageUpFile(new ThrowImmediatelyCheckNotifier());

            if (!string.IsNullOrWhiteSpace(opts.Server))
            {
                var builder = new SqlConnectionStringBuilder() { DataSource = opts.Server, InitialCatalog = opts.Database, IntegratedSecurity = true };

                CatalogueRepository.SuppressHelpLoading = true;
                var processor = new PluginProcessor(new ThrowImmediatelyCheckNotifier(), new CatalogueRepository(builder));
                processor.ProcessFileReturningTrueIfIsUpgrade(new FileInfo(opts.ZipFileName));
            }

            return 0;
        }
    }
}
