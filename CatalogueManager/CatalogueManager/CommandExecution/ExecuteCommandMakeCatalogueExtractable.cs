using System.Diagnostics;
using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Nodes;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMakeCatalogueExtractable : BasicUICommandExecution
    {
        private readonly CatalogueCommand _sourceCatalogueCommand;
        private readonly ExtractableDataSetsNode _targetExtractableDataSetsNode;

        public ExecuteCommandMakeCatalogueExtractable(IActivateItems activator, CatalogueCommand sourceCatalogueCommand, ExtractableDataSetsNode targetExtractableDataSetsNode) : base(activator)
        {
            _sourceCatalogueCommand = sourceCatalogueCommand;
            _targetExtractableDataSetsNode = targetExtractableDataSetsNode;

            if(_targetExtractableDataSetsNode.ExtractableDataSets.Any(e=>e.Catalogue_ID == _sourceCatalogueCommand.Catalogue.ID))
                SetImpossible("Catalogue '" + _sourceCatalogueCommand.Catalogue + "' is already Extractable");
        }

        public override void Execute()
        {
            base.Execute();

            var newDataset = new ExtractableDataSet(_targetExtractableDataSetsNode.DataExportRepository,_sourceCatalogueCommand.Catalogue);
            Publish(newDataset);

        }
    }
}