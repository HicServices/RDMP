using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace WebdavAutodowloader
{
    public class Options
    {
        [Option('f', "folder", Required = true, HelpText = "Remote folder to watch for new files")]
        public string RemoteFolder { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "pippolo", HelpText = "Password used for zip files")]
        public string ZipPassword { get; set; }
    }
}