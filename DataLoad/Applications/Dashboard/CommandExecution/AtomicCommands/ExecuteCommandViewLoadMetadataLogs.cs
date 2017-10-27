using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Dashboard.CatalogueSummary.LoadEvents;
using ReusableUIComponents.Icons.IconProvision;

namespace Dashboard.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewLoadMetadataLogs:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private LoadMetadata _loadmetadata;

        public ExecuteCommandViewLoadMetadataLogs(IActivateItems activator) : base(activator)
        {
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
