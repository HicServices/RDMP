using System;
using System.Data;
using DataLoadEngine.DataFlowPipeline.Sources;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    /// <summary>
    /// Class for selectively reading from a <see cref="DbDataCommandDataFlowSource"/> until a condition
    /// is met (and preserving the unmatching DataRow so it is not missed later).
    /// </summary>
    internal class RowPeeker
    {
        private DataRow _peekedRecord;

        /// <summary>
        /// Returns a DataTable with the peeked record, you should use the return value.  Returns peeked row
        /// even if the <paramref name="chunk"/> is null.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="clearPeek">True to clear the peeked records</param>
        /// <returns></returns>
        public DataTable AddPeekedRowsIfAny(DataTable chunk, bool clearPeek = true)
        {
            //if we have a peeked record
            if (_peekedRecord != null)
            {
                //if we are at the end of the batch
                if (chunk == null)
                {
                    //create a 1 row batch
                    var itemArray = _peekedRecord.ItemArray;
                    var tbl = _peekedRecord.Table;
                    tbl.Clear();
                    tbl.Rows.Add(itemArray);
                    return tbl;
                }

                DataRow newRow = chunk.NewRow();
                // We "clone" the row
                newRow.ItemArray = _peekedRecord.ItemArray;
                //add the peeked record
                chunk.Rows.InsertAt(newRow,0);

                //clear the peek
                _peekedRecord = null;
            }

            return chunk;
        }
        
        /// <summary>
        /// Reads records one at a time from the <paramref name="source"/> until a record is read where the
        /// <paramref name="equalityFunc"/> returns false.  Rows read (that passed <paramref name="equalityFunc"/>)
        /// are added to <paramref name="chunk"/> while the failing row becomes the new peeked row.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="equalityFunc"></param>
        /// <param name="chunk"></param>
        public void AddWhile(DbDataCommandDataFlowSource source, Func<DataRow,bool> equalityFunc,DataTable chunk)
        {
            if(_peekedRecord != null)
                throw new Exception("Cannot AddWhile when there is an existing peeked record, call AddPeekedRowsIfAny to drain the Peek");

            DataRow r;
                        
            //while we are still successfully reading rows and those rows have the same release id
            while((r = source.ReadOneRow()) != null)
                if(equalityFunc(r))
                    //add it to the current chunk
                    chunk.ImportRow(r);
                else
                    break;
            
            //we have the first record of the next dude
            if (r != null)
                _peekedRecord = r;
        }
    }
}