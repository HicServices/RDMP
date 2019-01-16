using CsvHelper;

namespace LoadModules.Generic.DataFlowSources.SubComponents
{

    /// <summary>
    /// Point in time record of a line read from CsvHelper including ReadingContext information such as <see cref="LineNumber"/>.  Can include multiple lines
    /// of the underlying file if there is proper qualifying quotes and newlines in the csv e.g. when including free text columns.
    /// </summary>
    public class FlatFileLine
    {
        /// <summary>
        /// The RAW file line number that this line reflects.  Where a record spans multiple lines (e.g. when it has newlines in quote qualified fields) it 
        /// seems to be the last line number in the record
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// The values as interpreted by CsvHelper for the current line
        /// </summary>
        public string[] Cells { get; set; }

        /// <summary>
        /// The absolute text as it appears in the flat file being read for this 'line'
        /// </summary>
        public string RawRecord { get; set; }

        /// <summary>
        /// The state of the CSVReader when the line was read
        /// </summary>
        public ReadingContext ReadingContext { get; set; }

        public FlatFileLine(ReadingContext context)
        {
            LineNumber = context.RawRow;
            Cells = context.Record;
            RawRecord = context.RawRecord;
            ReadingContext = context;

            //Doesn't seem to be correct:  StartPosition = context.RawRecordStartPosition;
        }

        

        public string this[int i]
        {
            get { return Cells[i]; }
        }

        public string GetLineDescription()
        {
            return string.Format("line {0}",LineNumber);
        }
    }
}
