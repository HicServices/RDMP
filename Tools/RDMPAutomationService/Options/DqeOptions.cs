using CommandLine;

namespace RDMPAutomationService.Options
{
    [Verb("dqe", HelpText = "Runs the Data Quality Engine")]
    class DqeOptions:StartupOptions
    {
        [Value(0, HelpText = @"Commands: 'run' the dqe", Required = true)]
        public DQECommands Command { get; set; }

        [Option('c',"Catalogue",HelpText = "ID of the Catalogue to run the DQE on",Required = true)]
        public int Catalogue{ get; set; }
    }

    internal enum DQECommands
    {
        None,
        run
    }
}