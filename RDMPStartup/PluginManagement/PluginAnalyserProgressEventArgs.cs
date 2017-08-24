using CatalogueLibrary.Data;

namespace RDMPStartup.PluginManagement
{
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