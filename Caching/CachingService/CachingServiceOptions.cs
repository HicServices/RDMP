using CommandLine;

namespace CachingService
{
    public class CachingServiceOptions
    {
        [Option('c', "console", HelpText = "Runs the service as a console application", DefaultValue = false)]
        public bool Console { get; set; }

        [Option('s', "connection-string", HelpText = "Connection string for connecting to the catalogue database", Required = true)]
        public string ConnectionString { get; set; }
    }
}