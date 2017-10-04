using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Ionic.Zip;
using WebDAVClient;
using WebDAVClient.Model;

namespace WebdavAutodowloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new WebdavReleaseEngineSettings();
            var isValid = Parser.Default.ParseArgumentsStrict(args, options);
            
            if (isValid)
            {
                var downloader = new AutoDownloader(options);
                downloader.Run();
            }
            Console.ReadLine();
        }
    }

    internal class AutoDownloader
    {
        private readonly WebdavReleaseEngineSettings options;

        public AutoDownloader(WebdavReleaseEngineSettings options)
        {
            this.options = options;
        }

        public void Run()
        {
            var file = GetFirstUnprocessed();
            if (file == null)
            {
                Console.WriteLine("No file to load...");
                return;
            }

            Console.WriteLine("Processing {0}" + file.DisplayName);

            var zipFilePath = DownloadToDestination(file);

            UnzipToReleaseFolder(zipFilePath);

            File.AppendAllText("processed.txt", file.Href + "\r\n");
        }

        private Item GetFirstUnprocessed()
        {
            var client = new Client(new NetworkCredential { UserName = options.Username, Password = options.Password });
            client.Server = options.Endpoint;
            client.BasePath = options.BasePath;

            var remoteFolder = client.GetFolder(options.RemoteFolder).Result;

            if (remoteFolder == null)
                return null;

            var files = client.List(remoteFolder.Href).Result;
            var enumerable = files as Item[] ?? files.ToArray();

            Console.WriteLine("Found {0} files", enumerable.Count());

            var alreadyProcessed = File.ReadAllLines("processed.txt");

            var latest = enumerable.Where(f => f.DisplayName.Contains("Release") && !alreadyProcessed.Contains(f.Href)).OrderBy(f => f.LastModified).FirstOrDefault();

            return latest;
        }

        private string DownloadToDestination(Item file)
        {
            var client = new Client(new NetworkCredential { UserName = options.Username, Password = options.Password });
            client.Server = options.Endpoint;
            client.BasePath = options.BasePath;

            using (var fileStream = File.Create(Path.Combine(options.LocalDestination, file.DisplayName)))
            {
                var content = client.Download(file.Href).Result;
                content.CopyTo(fileStream);
            }

            Console.WriteLine("Downloaded to {0}", Path.Combine(options.LocalDestination, file.DisplayName));

            return Path.Combine(options.LocalDestination, file.DisplayName);
        }

        private void UnzipToReleaseFolder(string zipFilePath)
        {
            var filename = Path.GetFileNameWithoutExtension(zipFilePath);
            Debug.Assert(filename != null, "filename != null");
            var linkProj = Regex.Match(filename, "Proj-(\\d+)").Groups[1].Value;

            var destination = Path.Combine(options.LocalDestination, "Project " + linkProj, filename);

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.Password = options.ZipPassword;
                zip.ExtractAll(destination);
            }

            Console.WriteLine("Unzipped all to {0}", destination);
        }
    }
}
