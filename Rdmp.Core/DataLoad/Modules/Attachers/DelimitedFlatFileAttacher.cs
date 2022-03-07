// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Globalization;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers
{
    /// <summary>
    /// See AnySeparatorFileAttacher
    /// </summary>
    public abstract class DelimitedFlatFileAttacher : FlatFileAttacher
    {
        protected DelimitedFlatFileDataFlowSource _source;
        
        [DemandsInitialization(DelimitedFlatFileDataFlowSource.ForceHeaders_DemandDescription)]
        public string ForceHeaders {
            get { return _source.ForceHeaders; }
            set { _source.ForceHeaders = value; }
        }
        
        [Obsolete("Do not use, no longer supported by CsvHelper")]
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

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.IgnoreColumns_Description)]
        public string IgnoreColumns
        {
            get { return _source.IgnoreColumns; }
            set { _source.IgnoreColumns = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.BadDataHandlingStrategy_DemandDescription,DefaultValue = BadDataHandlingStrategy.ThrowException)]
        public BadDataHandlingStrategy BadDataHandlingStrategy
        {
            get { return _source.BadDataHandlingStrategy; }
            set { _source.BadDataHandlingStrategy = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.IgnoreBadReads_DemandDescription)]
        public bool IgnoreBadReads
        {
            get { return _source.IgnoreBadReads; }
            set { _source.IgnoreBadReads = value; }
        }
        
        [DemandsInitialization(DelimitedFlatFileDataFlowSource.ThrowOnEmptyFiles_DemandDescription,DefaultValue = true)]
        public bool ThrowOnEmptyFiles
        {
            get { return _source.ThrowOnEmptyFiles; }
            set { _source.ThrowOnEmptyFiles = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.AttemptToResolveNewLinesInRecords_DemandDescription, DefaultValue = false)]
        public bool AttemptToResolveNewLinesInRecords
        {
            get { return _source.AttemptToResolveNewLinesInRecords; }
            set { _source.AttemptToResolveNewLinesInRecords = value; }
        }

        [DemandsInitialization(DelimitedFlatFileDataFlowSource.MaximumErrorsToReport_DemandDescription,DefaultValue = 100)]
        public int MaximumErrorsToReport
        {
            get { return _source.MaximumErrorsToReport; }
            set { _source.MaximumErrorsToReport = value; }
        }

        [DemandsInitialization(ExcelDataFlowSource.AddFilenameColumnNamed_DemandDescription)]
        public string AddFilenameColumnNamed { get; set; }

        [DemandsInitialization(Attacher.Culture_DemandDescription)]
        public override CultureInfo Culture { get => _source.Culture; set => _source.Culture = value; }

        [DemandsInitialization(Attacher.ExplicitDateTimeFormat_DemandDescription)]
        public override string ExplicitDateTimeFormat {get => _source.ExplicitDateTimeFormat; set => _source.ExplicitDateTimeFormat = value; }


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
            _source.Culture = Culture;
        }

        private IDataLoadEventListener _listener;
        private FileInfo _currentFile;

        protected override int IterativelyBatchLoadDataIntoDataTable(DataTable dt, int maxBatchSize,GracefulCancellationToken cancellationToken)
        {
            _source.MaxBatchSize = maxBatchSize;
            _source.SetDataTable(dt);
            _source.GetChunk(_listener, cancellationToken);

            //if we are adding a column to the data read which contains the file path
            if (!string.IsNullOrWhiteSpace(AddFilenameColumnNamed))
            {
                if(!dt.Columns.Contains(AddFilenameColumnNamed))
                    throw new FlatFileLoadException("AddFilenameColumnNamed is set to '" + AddFilenameColumnNamed + "' but the column did not exist in RAW");

                foreach (DataRow row in dt.Rows)
                    if (row[AddFilenameColumnNamed] == DBNull.Value)
                        row[AddFilenameColumnNamed] = _currentFile.FullName;
                
            }
            return dt.Rows.Count;
        }

        protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            _source.StronglyTypeInput = false;
            _source.StronglyTypeInputBatchSize = 0;
            _listener = listener;
            _source.PreInitialize(new FlatFileToLoad(fileToLoad), listener);
            _currentFile = fileToLoad;
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
