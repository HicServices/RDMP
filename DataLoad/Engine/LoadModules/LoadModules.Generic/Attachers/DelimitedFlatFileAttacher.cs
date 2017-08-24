using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CsvHelper;
using CsvHelper.Configuration;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using LoadModules.Generic.DataFlowSources;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Attachers
{
    public abstract class DelimitedFlatFileAttacher : FlatFileAttacher
    {
        protected DelimitedFlatFileDataFlowSource _source;
        
        [DemandsInitialization(DelimitedFlatFileDataFlowSource.ForceHeaders_DemandDescription)]
        public string ForceHeaders {
            get { return _source.ForceHeaders; }
            set { _source.ForceHeaders = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.UnderReadBehaviour_DemandDescription)]
        public BehaviourOnUnderReadType UnderReadBehaviour
        {
            get { return _source.UnderReadBehaviour; }
            set { _source.UnderReadBehaviour = value; }
        }
        
        [DemandsInitialization(DelimitedFlatFileDataFlowSource.IgnoreQuotes_DemandDescription)]
        public bool IgnoreQuotes
        {
            get { return _source.IgnoreQuotes; }
            set { _source.IgnoreQuotes = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.IgnoreBlankLines_DemandDescription)]
        public bool IgnoreBlankLines {
            get { return _source.IgnoreBlankLines; }
            set { _source.IgnoreBlankLines = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.ForceHeadersReplacesFirstLineInFile_Description)]
        public bool ForceHeadersReplacesFirstLineInFile
        {
            get { return _source.ForceHeadersReplacesFirstLineInFile; }
            set { _source.ForceHeadersReplacesFirstLineInFile = value; }
        }
        private GracefulCancellationToken cancellationToken = new GracefulCancellationToken();

          protected DelimitedFlatFileAttacher(char separator)
          {
              SetupSource(separator);

          }

        private void SetupSource(char separator)
        {
            _source = new DelimitedFlatFileDataFlowSource();
            _source.Separator = separator.ToString();
            _source.StronglyTypeInput = false;
            _source.StronglyTypeInputBatchSize = 0;
        }

        private IDataLoadEventListener _listener;

        protected override int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int maxBatchSize)
        {
            _source.MaxBatchSize = maxBatchSize;
            _source.SetDataTable(dt);
            _source.GetChunk(_listener, cancellationToken);

            return dt.Rows.Count;
        }

        protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener)
        {
            _source.StronglyTypeInput = false;
            _source.StronglyTypeInputBatchSize = 0;
            _listener = listener;
            _source.PreInitialize(new FlatFileToLoad(fileToLoad), listener);
        }

        protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
        {
            //automatically handled by SetDataTable
        }

        protected override void CloseFile()
        {
            _source.Dispose(_listener, null);
        }
    }
}
