using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewLoadProgress : BasicUICommandExecution,IAtomicCommand
    {
        private readonly LoadMetadata _loadMetadata;

        public ExecuteCommandCreateNewLoadProgress(IActivateItems activator, LoadMetadata loadMetadata) : base(activator)
        {
            _loadMetadata = loadMetadata;
        }

        public override string GetCommandHelp()
        {
            return "Defines that the data load configuration has too much data to load in one go and that it must be loaded in date based batches (e.g. load 2001-01-01 to 2001-01-31)";
        }

        public override void Execute()
        {
            base.Execute();

            var lp = new LoadProgress((ICatalogueRepository) _loadMetadata.Repository, _loadMetadata);
            Publish(_loadMetadata);
            Emphasise(lp);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadProgress, OverlayKind.Add);
        }
    }
}