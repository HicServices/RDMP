using System;
using System.Text;

namespace HIC.Logging
{
    /// <summary>
    /// Decides which records to fetch from the hierarchical logging database including row filter for specific
    /// run, table loaded etc.
    /// </summary>
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

        public string GetWhereSql(LoggingTables table)
        {
            switch (table)
            {
                case LoggingTables.None:
                    return "";
                case LoggingTables.DataLoadTask:
                    return Task.HasValue ? "WHERE ID =" + Task.Value:"";
                case LoggingTables.DataLoadRun:
                    if (Run.HasValue)
                        return "WHERE ID =" + Run.Value;
                    if (Task.HasValue)
                        return "WHERE dataLoadTaskID = " + Task.Value;
                    return "";
                case LoggingTables.ProgressLog:
                case LoggingTables.FatalError:
                case LoggingTables.TableLoadRun:
                    if (Run.HasValue)
                        return "WHERE dataLoadRunID =" + Run.Value;
                    return "";
                case LoggingTables.DataSource:
                    if (Table.HasValue)
                        return "WHERE tableLoadRunID =" + Table.Value;
                    return "";
                default:
                    throw new ArgumentOutOfRangeException("table");
            }
        }
    }
}