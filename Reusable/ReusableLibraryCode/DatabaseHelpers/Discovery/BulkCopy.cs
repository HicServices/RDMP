using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class BulkCopy:IBulkCopy
    {
        protected readonly IManagedConnection Connection;
        protected readonly DiscoveredTable TargetTable;
        protected readonly DiscoveredColumn[] TargetTableColumns;


        /// <summary>
        /// When calling GetMapping if there are DataColumns in the input table that you are trying to bulk insert that are not matched
        /// in the destination table then the default behaviour is to throw a KeyNotFoundException.  Set this to false to ignore that
        /// behaviour.  This will result in loosing data from your DataTable.
        /// 
        /// <para>Defaults to false</para>
        /// </summary>
        public bool AllowUnmatchedInputColumns { get; private set; }

        protected BulkCopy(DiscoveredTable targetTable, IManagedConnection connection)
        {
            TargetTable = targetTable;
            Connection = connection;
            TargetTableColumns = TargetTable.DiscoverColumns(connection.ManagedTransaction);
            AllowUnmatchedInputColumns = false;
        }


        public virtual int Timeout { get; set; }
        
        public virtual void Dispose()
        {
            Connection.Dispose();
        }

        public abstract int Upload(DataTable dt);

        /// <summary>
        /// Returns a case insensitive mapping between columns in your DataTable that you are trying to upload and the columns that actually exist in the destination 
        /// table.  
        /// <para>This overload gives you a list of all unmatched destination columns, these should be given null/default automatically by your database API</para>
        /// <para>Throws <exception cref="KeyNotFoundException"> if there are unmatched input columns unless <see cref="AllowUnmatchedInputColumns"/> is true.</exception></para>
        /// </summary>
        /// <param name="inputColumns"></param>
        /// <param name="unmatchedColumnsInDestination"></param>
        /// <returns></returns>
        protected Dictionary<DataColumn, DiscoveredColumn> GetMapping(IEnumerable<DataColumn> inputColumns, out DiscoveredColumn[] unmatchedColumnsInDestination)
        {
            Dictionary<DataColumn, DiscoveredColumn> mapping = new Dictionary<DataColumn, DiscoveredColumn>();

            foreach (DataColumn colInSource in inputColumns)
            {
                var match = TargetTableColumns.SingleOrDefault(c => c.GetRuntimeName().Equals(colInSource.ColumnName, StringComparison.CurrentCultureIgnoreCase));

                if (match == null)
                {
                    if (!AllowUnmatchedInputColumns)
                        throw new KeyNotFoundException("Column " + colInSource.ColumnName + " appears in pipeline but not destination table (" + TargetTable + ")");

                    //user is ignoring the fact there are unmatched items in DataTable!
                }
                else
                    mapping.Add(colInSource, match);
            }

            //unmatched columns in the destination is fine, these usually get populated with the default column values or nulls
            unmatchedColumnsInDestination = TargetTableColumns.Except(mapping.Values).ToArray();

            return mapping;
        }

        /// <summary>
        /// Returns a case insensitive mapping between columns in your DataTable that you are trying to upload and the columns that actually exist in the destination 
        /// table.  
        /// <para>Throws <exception cref="KeyNotFoundException"> if there are unmatched input columns unless <see cref="AllowUnmatchedInputColumns"/> is true.</exception></para>
        /// </summary>
        /// <param name="inputColumns"></param>
        /// <returns></returns>
        protected Dictionary<DataColumn,DiscoveredColumn> GetMapping(IEnumerable<DataColumn> inputColumns)
        {
            DiscoveredColumn[] whoCares;
            return GetMapping(inputColumns, out whoCares);
        }
    }
}