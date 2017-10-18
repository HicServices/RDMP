using System.Diagnostics;
using System.Linq;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Nodes;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMakeCatalogueExtractable : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly CatalogueCommand _sourceCatalogueCommand;
        private readonly ExtractableDataSetsNode _targetExtractableDataSetsNode;

        public ExecuteCommandMakeCatalogueExtractable(IActivateItems activator, CatalogueCommand sourceCatalogueCommand, ExtractableDataSetsNode targetExtractableDataSetsNode)
        {
            _activator = activator;
            _sourceCatalogueCommand = sourceCatalogueCommand;
            _targetExtractableDataSetsNode = targetExtractableDataSetsNode;

            if(_targetExtractableDataSetsNode.ExtractableDataSets.Any(e=>e.Catalogue_ID == _sourceCatalogueCommand.Catalogue.ID))
                SetImpossible("Catalogue '" + _sourceCatalogueCommand.Catalogue + "' is already Extractable");
        }

        public override void Execute()
        {
            base.Execute();

            var newDataset = new ExtractableDataSet(_targetExtractableDataSetsNode.DataExportRepository,_sourceCatalogueCommand.Catalogue);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(newDataset));

        }
    }
}