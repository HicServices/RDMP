using CsvHelper;

namespace LoadModules.Generic.DataFlowSources.SubComponents
{
    public class FlatFileLine
    {
        public int LineNumber { get; private set; }
        public string[] Cells { get; set; }
        public string RawRecord { get; set; }

        public FlatFileLine(ReadingContext context)
        {
            LineNumber = context.RawRow;
            Cells = context.Record;
            RawRecord = context.RawRecord;

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
