using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace LoadModules.Generic.DataFlowSources.SubComponents
{
    public class FlatFileLine
    {
        public int LineNumber { get; private set; }
        public string[] Cells { get; set; }
        public string RawRecord { get; set; }

        public FlatFileLine(int lineNumber, string[] cells, string rawRecord)
        {
            LineNumber = lineNumber;
            Cells = cells;
            RawRecord = rawRecord;
        }

        public FlatFileLine(ReadingContext context)
        {
            LineNumber = context.RawRow;
            Cells = context.Record;
            RawRecord = context.RawRecord;
        }

        public string this[int i]
        {
            get { return Cells[i]; }
        }
    }
}
