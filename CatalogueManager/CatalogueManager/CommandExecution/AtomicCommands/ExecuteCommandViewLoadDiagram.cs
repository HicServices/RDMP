using System.Drawing;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewLoadDiagram :BasicUICommandExecution, IAtomicCommand
    {
        private readonly LoadMetadata _loadMetadata;

        public ExecuteCommandViewLoadDiagram(IActivateItems activator, LoadMetadata loadMetadata) : base(activator)
        {
            _loadMetadata = loadMetadata;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.LoadBubble;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.ActivateViewLoadMetadataDiagram(this, _loadMetadata);
        }
    }
}