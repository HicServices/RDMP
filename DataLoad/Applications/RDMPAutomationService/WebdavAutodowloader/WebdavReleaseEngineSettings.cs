using System;
using System.Text;
using CommandLine;
using WebdavAutodowloader.Properties;

namespace WebdavAutodowloader
{
    public class WebdavReleaseEngineSettings
    {
        [Option('f', "folder", Required = false, DefaultValue = "RDMP-Releases", HelpText = "Remote folder to watch for new files")]
        public string RemoteFolder { get; set; }

        [Option('z', "zip-password", Required = false, DefaultValue = "d4t4!!", HelpText = "Password used for zip files")]
        public string ZipPassword { get; set; }

        [Option('d', "destination", Required = false, DefaultValue = "C:\\TEMP", HelpText = "Local folder to decompress files in")]
        public string LocalDestination { get; set; }

        [Option('e', "endpoint", Required = false, DefaultValue = "https://hiccloud1.dundee.ac.uk/", HelpText = "Remote webdav endpoint")]
        public string Endpoint { get; set; }

        [Option('b', "basepath", Required = false, DefaultValue = "/remote.php/webdav/HICCLOUD/", HelpText = "Remote webdav basepath")]
        public string BasePath { get; set; }

        [Option('u', "username", Required = true, HelpText = "Webdav username")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Webdav password")]
        public string Password { get; set; }

        public WebdavReleaseEngineSettings()
        {
            //RemoteFolder = remoteFolder;
            //ZipPassword = zipPassword;
            //Endpoint = Settings.Default.Endpoint;
            //BasePath = Settings.Default.BasePath;
            //Username = Settings.Default.Username;
            //Password = Encoding.ASCII.GetString(Convert.FromBase64String(Settings.Default.Password));
        }
    }
}