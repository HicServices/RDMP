using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace RDMPAutomationService.Options
{
    [Verb("service", HelpText = "Install or uninstall the RDMPAutomationService as a windows service")]
    class ServiceOptions
    {
        [Option("i",Default = false, HelpText = "Install the windows service")]
        public bool Install { get; set; }

        [Option("u", Default = false, HelpText = "Uninstall the windows service")]
        public bool Uninstall { get; set; }
    }
}
