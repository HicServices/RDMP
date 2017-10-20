using System.Linq;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddDatasetsToConfiguration : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly ExtractableDataSetCommand _sourceExtractableDataSetCommand;
        private readonly ExtractionConfiguration _targetExtractionConfiguration;

        private readonly IExtractableDataSet[] _toadd;

        public ExecuteCommandAddDatasetsToConfiguration(IActivateItems activator,ExtractableDataSetCommand sourceExtractableDataSetCommand, ExtractionConfiguration targetExtractionConfiguration)
        {
            _activator = activator;
            _sourceExtractableDataSetCommand = sourceExtractableDataSetCommand;
            _targetExtractionConfiguration = targetExtractionConfiguration;


            var alreadyInConfiguration = _targetExtractionConfiguration.GetAllExtractableDataSets().ToArray();

            _toadd = _sourceExtractableDataSetCommand.ExtractableDataSets.Except(alreadyInConfiguration).ToArray();

            if(!_toadd.Any())
                SetImpossible("ExtractionConfiguration already contains this dataset(s)");


            
        }

        public override void Execute()
        {
            base.Execute();

            foreach (var ds in _toadd)
                _targetExtractionConfiguration.AddDatasetToConfiguration(ds);

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_targetExtractionConfiguration));
        }
    }
}