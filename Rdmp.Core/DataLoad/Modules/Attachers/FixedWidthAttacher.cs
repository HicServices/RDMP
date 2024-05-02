// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Data load component for loading 'fixed width' files into RAW tables.  Fixed width files are those where there are
///     no separators in the file and columns
///     are instead denoted by the character position in the line e.g. 'first 10 characters of a line are patient
///     identifier, next 8 are date of birth etc'. In
///     such a file all lines should be equal length and whitespace should be included in field values to ensure this.
///     Fixed width files are common in ancient
///     lab systems and places where large volumes of data are outputted.
///     <para>
///         To use this attacher you will need a 'FormatFile' which describes the length/type of each field (See
///         FixedWidthFormatFile).
///     </para>
///     <para>
///         The width of the file MUST match exactly the width of the data table being loaded - although the table may
///         contain varchar columns in which case the
///         max width specified on the varchar will be assumed as the width of the flat file column e.g. varchar(5) will be
///         mapped to column width of 5
///     </para>
/// </summary>
public class FixedWidthAttacher : FlatFileAttacher
{
    [DemandsInitialization(
        @"The location of a HIC formatted Fixed width file descriptor, e.g. \\ares\unit\HIC Data\DATA Sets\z_TEMPLATE_data_folder\Data\ExampleFixedWidthFormatFile.csv"
    )]
    public FileInfo PathToFormatFile { get; set; }

    private DataTable _flatFile;

    protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        bHaveAlreadySubmittedData = false;

        var format = new FixedWidthFormatFile(PathToFormatFile);
        _flatFile = format.GetDataTableFromFlatFile(fileToLoad);
    }

    protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
    {
        //complain about unmatched columns
        foreach (DataColumn col in _flatFile.Columns)
            if (!loadTarget.Columns
                    .Contains(col
                        .ColumnName)) //We use notify error here rather than throwing an Exception because there could be many dodgy /misnamed columns so tell the user about all of them
                job.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error,
                        $"Format file ({PathToFormatFile.FullName}) indicated there would be a header called '{col.ColumnName}' but the column did not appear in the RAW database table (Columns in RAW were {string.Join(",", loadTarget.Columns.Cast<DataColumn>().Select(c => c.ColumnName))})"));
    }

    private bool bHaveAlreadySubmittedData;


    /// <summary>
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="maxBatchSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override int IterativelyBatchLoadDataIntoDataTable(DataTable destination, int maxBatchSize,
        GracefulCancellationToken cancellationToken)
    {
        if (bHaveAlreadySubmittedData)
            return 0;

        //copy data from the flat file data table into the destination data table and let parent do the rest
        foreach (DataRow row in _flatFile.Rows)
        {
            var dataRow = destination.Rows.Add();

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

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }
}