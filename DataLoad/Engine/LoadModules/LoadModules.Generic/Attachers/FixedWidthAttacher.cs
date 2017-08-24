using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Attachers
{
    [Description(
        "Populates a data table using the given flat file.  The flat file will have columns of fixed width.  The width of the file MUST match exactly the width of the data table being loaded - although the table may contain varchar columns in which case the max width specified on the varchar will be assumed as the width of the flat file column e.g. varchar(5) will be mapped to column width of 5"
        )]
    public class FixedWidthAttacher : FlatFileAttacher
    {

        [DemandsInitialization(
            @"The location of a HIC formatted Fixed width file descriptor, e.g. \\ares\unit\HIC Data\DATA Sets\z_TEMPLATE_data_folder\Data\ExampleFixedWidthFormatFile.csv"
            )]
        public FileInfo PathToFormatFile { get; set; }

        DataTable _flatFile;
        protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener)
        {
            bHaveAlreadySubmittedData = false;

            FixedWidthFormatFile format = new FixedWidthFormatFile(PathToFormatFile);
            _flatFile = format.GetDataTableFromFlatFile(fileToLoad);
        }

        protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
        {
            //complain about unmatched columns
            foreach (DataColumn col in _flatFile.Columns)
                if (!loadTarget.Columns.Contains(col.ColumnName))//We use notify error here rather than throwing an Exception because there could be many dodgy /misnamed columns so tell the user about all of them
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Error,
                            "Format file (" + PathToFormatFile.FullName + ") indicated there would be a header called '" +
                            col.ColumnName +
                            "' but the column did not appear in the RAW database table (Columns in RAW were " +
                            string.Join(",", loadTarget.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) + ")")); 
        }

        private bool bHaveAlreadySubmittedData = false;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="reader"></param>
        /// <param name="maxBatchSize"></param>
        /// <returns></returns>
        protected override int IterativelyBatchLoadDataIntoDataTable(DataTable destination, int maxBatchSize)
        {
            if (bHaveAlreadySubmittedData)
                return 0;

            //copy data from the flat file data table into the destination data table and let parent do the rest
            foreach (DataRow row in _flatFile.Rows)
            {
                DataRow dataRow = destination.Rows.Add();

                foreach (DataColumn column in _flatFile.Columns)
                    dataRow[column.ColumnName] = row[column.ColumnName];
            }

            //there is no more data to read, return the number of rows read but set bHaveAlreadySubmittedData to true so that we return 0 next time this method is called
            bHaveAlreadySubmittedData = true;
           
            return destination.Rows.Count;
        }

        protected override void CloseFile()
        {
            
        }

        public override void Check(ICheckNotifier notifier)
        {
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            
        }
    }
}