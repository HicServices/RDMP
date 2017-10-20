using System;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandImportNewCopyOfFilterIntoContainer : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private FilterCommand _filterCommand;
        private IContainer _targetContainer;

        public ExecuteCommandImportNewCopyOfFilterIntoContainer(IActivateItems activator,FilterCommand filterCommand, IContainer targetContainer)
        {
            _activator = activator;
            _filterCommand = filterCommand;
            _targetContainer = targetContainer;

            //if source catalogue is known
            if(_filterCommand.SourceCatalogueIfAny != null)
            {
                var targetCatalogue = targetContainer.GetCatalogueIfAny();
                
                if(targetCatalogue != null)
                    if(!_filterCommand.SourceCatalogueIfAny.Equals(targetCatalogue))
                        SetImpossible("Cannot Import Filter from '" + _filterCommand.SourceCatalogueIfAny + "' into '" + targetCatalogue +"'");

            }
        }

        public override void Execute()
        {
            base.Execute();

            var wizard = new FilterImportWizard();
            IFilter newFilter = wizard.Import(_targetContainer, _filterCommand.Filter);
            if (newFilter != null)
            {
                _targetContainer.AddChild(newFilter);
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity) _targetContainer));
            }
        }
    }
}