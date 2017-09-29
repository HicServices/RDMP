using System;
using System.Text;
using CommandLine;
using WebdavAutodowloader.Properties;

namespace WebdavAutodowloader
{
    public class WebdavReleaseEngineSettings
    {
        [Option('f', "folder", Required = false, DefaultValue = "Leandro", HelpText = "Remote folder to watch for new files")]
        public string RemoteFolder { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "pippolo", HelpText = "Password used for zip files")]
        public string ZipPassword { get; set; }

        [Option('d', "destination", Required = false, DefaultValue = "C:\\TEMP", HelpText = "Local folder to decompress files in")]
        public string LocalDestination { get; set; }

        public string Endpoint { get; set; }

        public string BasePath { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public WebdavReleaseEngineSettings()
        {
            //RemoteFolder = remoteFolder;
            //ZipPassword = zipPassword;
            Endpoint = Settings.Default.Endpoint;
            BasePath = Settings.Default.BasePath;
            Username = Settings.Default.Username;
            Password = Encoding.ASCII.GetString(Convert.FromBase64String(Settings.Default.Password));
        }
    }
}