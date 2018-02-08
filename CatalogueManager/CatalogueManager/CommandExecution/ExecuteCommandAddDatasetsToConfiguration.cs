using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddDatasetsToConfiguration : BasicUICommandExecution
    {
        private readonly ExtractionConfiguration _targetExtractionConfiguration;

        private IExtractableDataSet[] _toadd;

        public ExecuteCommandAddDatasetsToConfiguration(IActivateItems activator,ExtractableDataSetCommand sourceExtractableDataSetCommand, ExtractionConfiguration targetExtractionConfiguration) 
            : this(activator,targetExtractionConfiguration)
        {
            SetExtractableDataSets(sourceExtractableDataSetCommand.ExtractableDataSets);
            
        }

        public ExecuteCommandAddDatasetsToConfiguration(IActivateItems itemActivator, ExtractableDataSet extractableDataSet, ExtractionConfiguration targetExtractionConfiguration)
            : this(itemActivator,targetExtractionConfiguration)
        {
            SetExtractableDataSets(extractableDataSet);
        }

        private ExecuteCommandAddDatasetsToConfiguration(IActivateItems itemActivator, ExtractionConfiguration targetExtractionConfiguration) : base(itemActivator)
        {
            _targetExtractionConfiguration = targetExtractionConfiguration;

            if (_targetExtractionConfiguration.IsReleased)
                SetImpossible("Extraction is Frozen because it has been released and is readonly, try cloning it instead");
        }

        private void SetExtractableDataSets(params IExtractableDataSet[] toAdd)
        {
            var alreadyInConfiguration = _targetExtractionConfiguration.GetAllExtractableDataSets().ToArray();
            _toadd = toAdd.Except(alreadyInConfiguration).ToArray();

            if(!_toadd.Any())
                SetImpossible("ExtractionConfiguration already contains this dataset(s)");
        }

        public override void Execute()
        {
            base.Execute();

            foreach (var ds in _toadd)
                _targetExtractionConfiguration.AddDatasetToConfiguration(ds);

            Publish(_targetExtractionConfiguration);
        }
    }
}