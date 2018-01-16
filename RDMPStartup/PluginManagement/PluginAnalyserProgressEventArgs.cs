using CatalogueLibrary.Data;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Records the progress made in evaluating a dll in PluginAnalyser
    /// </summary>
    public class PluginAnalyserProgressEventArgs
    {
        public PluginAnalyserProgressEventArgs(int progress, int maxProgress, LoadModuleAssembly lma)
        {
            Progress = progress;
            ProgressMax = maxProgress;
            CurrentAssemblyBeingProcessed = lma;
        }

        public int Progress { get; set; }
        public int ProgressMax { get; set; }
        public LoadModuleAssembly CurrentAssemblyBeingProcessed { get; set; }

    }
}