using System;
using System.Text;
using CatalogueLibrary.Data;

namespace CatalogueManager.LogViewer
{
    public class LogViewerFilter
    {
        public bool IsEmpty { get { return Run == null && Table == null && Task == null; } }

        public int? Task { get; set; }

        public int? Run { get; set; }

        public int? Table { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if(Task != null)
                sb.Append("DataLoadTask=" + Task);

            if (Run != null)
                sb.Append("DataLoadRun=" + Run);

            if (Table != null)
                sb.Append("TableLoadRun=" + Table);

            if (sb.Length == 0)
                return "No filter";

            return sb.ToString();
        }
    }
}