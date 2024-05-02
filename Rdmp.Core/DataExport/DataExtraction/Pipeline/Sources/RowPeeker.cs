// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;

/// <summary>
///     Class for selectively reading from a <see cref="DbDataCommandDataFlowSource" /> until a condition
///     is met (and preserving the unmatching DataRow so it is not missed later).
/// </summary>
internal class RowPeeker
{
    private DataRow _peekedRecord;

    /// <summary>
    ///     Returns a DataTable with the peeked record, you should use the return value.  Returns peeked row
    ///     even if the <paramref name="chunk" /> is null.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="clearPeek">True to clear the peeked records</param>
    /// <returns></returns>
    public DataTable AddPeekedRowsIfAny(DataTable chunk, bool clearPeek = true)
    {
        //if we have a peeked record
        if (_peekedRecord != null)
            try
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

                var newRow = chunk.NewRow();
                // We "clone" the row
                newRow.ItemArray = _peekedRecord.ItemArray;
                //add the peeked record
                chunk.Rows.InsertAt(newRow, 0);
            }
            finally
            {
                //clear the peek
                if (clearPeek)
                    _peekedRecord = null;
            }

        return chunk;
    }

    /// <summary>
    ///     Reads records one at a time from the <paramref name="source" /> until a record is read where the
    ///     <paramref name="equalityFunc" /> returns false.  Rows read (that passed <paramref name="equalityFunc" />)
    ///     are added to <paramref name="chunk" /> while the failing row becomes the new peeked row.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="equalityFunc"></param>
    /// <param name="chunk"></param>
    public void AddWhile(IDbDataCommandDataFlowSource source, Func<DataRow, bool> equalityFunc, DataTable chunk)
    {
        if (_peekedRecord != null)
            throw new Exception(
                "Cannot AddWhile when there is an existing peeked record, call AddPeekedRowsIfAny to drain the Peek");
        chunk.BeginLoadData();
        //while we are still successfully reading rows and those rows have the same release id
        while (source.ReadOneRow() is { } r)
            if (equalityFunc(r))
                //add it to the current chunk
            {
                chunk.ImportRow(r);
            }
            else
            {
                //match was failure on this new record (but the data is not exhausted).  So the peek becomes this new record
                _peekedRecord = r;
                break;
            }

        chunk.EndLoadData();
    }
}