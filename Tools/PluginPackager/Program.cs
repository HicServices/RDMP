using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using RDMPStartup;
using RDMPStartup.PluginManagement;
using ReusableLibraryCode.Checks;

namespace PluginPackager
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new PluginPackagerProgramOptions();
            
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
                return -1;

            if (options.Items.Count != 2)
            {
                Console.WriteLine(options.GetUsage());
                return -2;
            }

            FileInfo f = new FileInfo(options.Items[0]);
            if (!f.Exists)
            {
                Console.WriteLine("Could not find dll");
                return -58;
            }

            Packager p = new Packager(f, options.Items[1],options.SkipSourceCodeCollection);
            p.PackageUpFile(new ThrowImmediatelyCheckNotifier());
           
            if(!string.IsNullOrWhiteSpace(options.Server))
            {
                var builder = new SqlConnectionStringBuilder() {DataSource = options.Server, InitialCatalog = options.Database,IntegratedSecurity = true};

                CatalogueRepository.SuppressHelpLoading = true;
                var processor = new PluginProcessor(new ThrowImmediatelyCheckNotifier(), new CatalogueRepository(builder));
                processor.ProcessFileReturningTrueIfIsUpgrade(new FileInfo(options.Items[1]));
                
            }
            
            return 0;
        }
    }
}
