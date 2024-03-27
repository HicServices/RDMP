// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
/// Data load component for loading Microsoft Excel files into RAW tables.  This class relies on pipeline source component ExcelDataFlowSource for the actual
/// reading and handles only the rationalisation of columns read vs RAW columns available.
/// </summary>
public class ExcelAttacher : FlatFileAttacher
{
    private ExcelDataFlowSource _hostedSource;
    private DataTable _dataTable;
    private FileInfo _fileToLoad;

    [DemandsInitialization(ExcelDataFlowSource.WorkSheetName_DemandDescription)]
    public string WorkSheetName { get; set; }

    [DemandsInitialization(ExcelDataFlowSource.AddFilenameColumnNamed_DemandDescription)]
    public string AddFilenameColumnNamed { get; set; }

    [DemandsInitialization(
        "Forces specific overridden headers to be for columns, this is a comma separated string that will effectively replace the column headers found in the excel file.  The number of headers MUST match the number in the original file.  This option should be used when you have a excel file with stupid names that you want to rationalise into sensible database column names")]
    public string ForceReplacementHeaders { get; set; }

    [DemandsInitialization(
        "By default ALL columns in the source MUST match exactly (by name) the set of all columns in the destination table.  If you enable this option then it is allowable for there to be extra columns in the destination that are not populated (because they are not found in the flat file).  This does not let you discard columns from the source! (all source columns must have mappings but destination columns with no matching source are left null)")]
    public bool AllowExtraColumnsInTargetWithoutComplainingOfColumnMismatch { get; set; }

    [DemandsInitialization("Which Row the data you want to read starts on. Set this value to row containing the header information. The First row is 1.")]
    public int RowOffset { get; set; } = 0;//First row 1;

    [DemandsInitialization("Which column the data you want to read starts on. Accepts both letters & numbers, starts at 'A'/0. ")]
    public string ColumnOffset { get; set; } = "A";//A=0,B=1...

    private bool _haveServedData = false;


    private int ConvertColumnOffsetToInt()
    {
        if (ColumnOffset is null) return 0;
        if(int.TryParse(ColumnOffset,out var result)) return result;
        if (ColumnOffset.Length == 1 && char.IsLetter(ColumnOffset[0])) return char.ToUpper(ColumnOffset[0]) - 65;//would be 64, but we index from zero here
        throw new Exception("Column offset is not a valid number or letter");
    }


    public override void Check(ICheckNotifier notifier)
    {
        ConvertColumnOffsetToInt();
        base.Check(notifier);
    }

    protected override void OpenFile(FileInfo fileToLoad, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        _haveServedData = false;
        _fileToLoad = fileToLoad;
        _hostedSource = new ExcelDataFlowSource
        {
            WorkSheetName = WorkSheetName,
            AddFilenameColumnNamed = AddFilenameColumnNamed
        };

        _hostedSource.PreInitialize(new FlatFileToLoad(fileToLoad), listener);
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to start processing {fileToLoad.FullName}"));

        var columnTranslation = ConvertColumnOffsetToInt();

        string[] replacementHeadersSplit = { };
        if (!string.IsNullOrEmpty(ForceReplacementHeaders))
        {
            replacementHeadersSplit = ForceReplacementHeaders.Split(',')
                .Select(h => string.IsNullOrWhiteSpace(h) ? h : h.Trim()).ToArray();
        }

        _dataTable = _hostedSource.GetChunk(listener, cancellationToken, RowOffset, columnTranslation, replacementHeadersSplit);
      
        //all data should now be exhausted
        if (_hostedSource.GetChunk(listener, cancellationToken, RowOffset, columnTranslation) != null)
            throw new Exception(
                "Hosted source served more than 1 chunk, expected all the data to be read from the Excel file in one go");
    }

    protected override int IterativelyBatchLoadDataIntoDataTable(DataTable loadTarget, int maxBatchSize,
        GracefulCancellationToken cancellationToken)
    {
        if (!_haveServedData)
        {
            foreach (DataRow dr in _dataTable.Rows)
                try
                {
                    var targetRow = loadTarget.Rows.Add();

                    //column names must be the same!
                    foreach (DataColumn column in loadTarget.Columns)
                        if (_dataTable.Columns.Contains(column.ColumnName))
                        {
                            if (dr[column.ColumnName] == null ||
                                string.IsNullOrWhiteSpace(dr[column.ColumnName].ToString()))
                                targetRow[column.ColumnName] = DBNull.Value;
                            else
                                targetRow[column.ColumnName] = dr[column.ColumnName]; //copy values into the destination
                        }
                        else if
                            (AllowExtraColumnsInTargetWithoutComplainingOfColumnMismatch) //it is an extra destination column, see if that is allowed
                        {
                            targetRow[column.ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            throw new Exception(
                                $"Could not find column {column.ColumnName} in the source table we loaded from Excel, this should have been picked up earlier in GenerateColumnNameMismatchErrors");
                        }
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Could not import values into RAW DataTable structure (from Excel DataTable structure):{string.Join(",", dr.ItemArray)}",
                        e);
                }

            _haveServedData = true;

            return _dataTable.Rows.Count;
        }

        return 0;
    }


    private void GenerateColumnNameMismatchErrors(List<string> columnsExcelButNotInDataTable,
        List<string> columnsInDataTableButNotInExcel)
    {
        //if there are unmatched columns in the flat file
        if (columnsExcelButNotInDataTable.Any() ||

            //or there are unmatched columns in the destination (and we are not happy just leaving those as null)
            (columnsInDataTableButNotInExcel.Any() && !AllowExtraColumnsInTargetWithoutComplainingOfColumnMismatch))
            throw new Exception(
                $"Mismatch between RAW table {TableName} and Excel file \"{_fileToLoad.FullName}\":{Environment.NewLine}Columns in Excel file but not in DataTable {TableName}:{Environment.NewLine}{columnsExcelButNotInDataTable.Aggregate("", (s, n) => $"{s}{n},").TrimEnd(',')}{Environment.NewLine}Columns in DataTable but not in Excel file \"{_fileToLoad.FullName}\":{Environment.NewLine}{columnsInDataTableButNotInExcel.Aggregate("", (s, n) => $"{s}{n},").TrimEnd(',')}{Environment.NewLine}{Environment.NewLine}"
            );
    }

    protected override void CloseFile()
    {
    }


    protected override void ConfirmFlatFileHeadersAgainstDataTable(DataTable loadTarget, IDataLoadJob job)
    {
        var colsInTarget = loadTarget.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
        var colsInSource = _dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();


        GenerateColumnNameMismatchErrors(
            colsInSource.Except(colsInTarget).ToList(),
            colsInTarget.Except(colsInSource).ToList());
    }
}