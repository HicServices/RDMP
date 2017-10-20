using System;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewLoadPeriodically : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly LoadMetadata _loadMetadata;

        public ExecuteCommandCreateNewLoadPeriodically(IActivateItems activator, LoadMetadata loadMetadata)
        {
            _activator = activator;
            _loadMetadata = loadMetadata;
            if(loadMetadata.LoadProgresses.Any())
                SetImpossible("Load already has a LoadProgress");
            
            if( loadMetadata.LoadPeriodically != null) 
                SetImpossible("There is already a LoadPeriodically associated with this LoadMetadata");

        }

        public override void Execute()
        {
            base.Execute();

            var lp = new LoadPeriodically((ICatalogueRepository) _loadMetadata.Repository, _loadMetadata, 100);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_loadMetadata));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadPeriodically, OverlayKind.Add);
        }
    }
}