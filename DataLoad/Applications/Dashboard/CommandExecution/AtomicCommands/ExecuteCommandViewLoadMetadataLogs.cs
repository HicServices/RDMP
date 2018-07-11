using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Dashboard.CatalogueSummary.LoadEvents;
using ReusableLibraryCode.Icons.IconProvision;

namespace Dashboard.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewLoadMetadataLogs:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private LoadMetadata _loadmetadata;

        [ImportingConstructor]
        public ExecuteCommandViewLoadMetadataLogs(IActivateItems activator, LoadMetadata loadMetadata): base(activator)
        {
            SetTarget(loadMetadata);
        }
        
        public ExecuteCommandViewLoadMetadataLogs(IActivateItems activator) : base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "View the hierarchical audit log of all executions of the data load configuration";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Logging);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _loadmetadata = (LoadMetadata) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();
            Activator.Activate<AllLoadEventsUI, LoadMetadata>(_loadmetadata);
        }
    }
}
