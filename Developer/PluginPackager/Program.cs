using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

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
            var tempDir = p.PackageUpFile();

            if (tempDir != null)
            {
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C choice /C Y /N /D Y /T 3 & rmdir /s /q \"" + tempDir.FullName + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info); 
            }
            
            return 0;
        }
    }
}
