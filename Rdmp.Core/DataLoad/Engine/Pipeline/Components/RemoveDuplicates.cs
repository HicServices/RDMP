// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components;

/// <summary>
///     PipelineComponent which removes 100% duplicate rows from a DataTable during Pipeline execution based on row hashes.
/// </summary>
public class RemoveDuplicates : IPluginDataFlowComponent<DataTable>
{
    private readonly Stopwatch sw = new();
    private int totalRecordsProcessed;
    private int totalDuplicatesFound;
    private readonly Dictionary<int, List<DataRow>> _uniqueHashesSeen = new();

    /// <summary>
    ///     Turns off notify messages about number of duplicates found/replaced
    /// </summary>
    public bool NoLogging { get; set; }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        sw.Start();

        var toReturn = toProcess.Clone();

        //now sort rows
        foreach (DataRow row in toProcess.Rows)
        {
            totalRecordsProcessed++;
            var hashOfItems = GetHashCode(row.ItemArray);

            if (_uniqueHashesSeen.TryGetValue(hashOfItems, out var collisions))
            {
                //GetHashCode on ItemArray of row has been seen before but it could be a collision so call Enumerable.SequenceEqual just in case.
                if (collisions.Any(r => r.ItemArray.SequenceEqual(row.ItemArray)))
                {
                    totalDuplicatesFound++;
                    continue; //it's a duplicate
                }

                collisions.Add(row);
            }
            else
            {
                //it's not a duplicate hashcode so add it to the return array and the record of everything we have seen so far (in order that we do not run across issues across batches)
                _uniqueHashesSeen.Add(hashOfItems, new List<DataRow>(new[] { row }));
            }

            toReturn.Rows.Add(row.ItemArray);
        }

        sw.Stop();

        if (!NoLogging)
        {
            listener.OnProgress(this,
                new ProgressEventArgs("Evaluating For Duplicates",
                    new ProgressMeasurement(totalRecordsProcessed, ProgressType.Records), sw.Elapsed));
            listener.OnProgress(this,
                new ProgressEventArgs("Discarding Duplicates",
                    new ProgressMeasurement(totalDuplicatesFound, ProgressType.Records), sw.Elapsed));
        }

        return toReturn;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
    }

    /// <summary>
    ///     Gets the hash code for the contents of the array since the default hash code
    ///     for an array is unique even if the contents are the same.
    /// </summary>
    /// <remarks>
    ///     See Jon Skeet (C# MVP) response in the StackOverflow thread
    ///     http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
    /// </remarks>
    /// <param name="array">The array to generate a hash code for.</param>
    /// <returns>The hash code for the values in the array.</returns>
    public static int GetHashCode(object[] array)
    {
        // if non-null array then go into unchecked block to avoid overflow
        if (array != null)
            unchecked
            {
                var hash = 17;

                // get hash code for all items in array
                foreach (var item in array) hash = hash * 23 + (item != null ? item.GetHashCode() : 0);

                return hash;
            }

        // if null, hash code is zero
        return 0;
    }
}