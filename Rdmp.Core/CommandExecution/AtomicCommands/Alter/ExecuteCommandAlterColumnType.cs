// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Alter;

/// <summary>
///     Changes the datatype of the database column in the live database
/// </summary>
public class ExecuteCommandAlterColumnType : BasicCommandExecution
{
    private readonly ColumnInfo columnInfo;
    private readonly string _datatype;

    public ExecuteCommandAlterColumnType(IBasicActivateItems activator, ColumnInfo columnInfo, string datatype = null) :
        base(activator)
    {
        this.columnInfo = columnInfo;
        _datatype = datatype;

        if (columnInfo.TableInfo.IsView)
            SetImpossible("Column is part of a view so cannot be altered");
        if (columnInfo.TableInfo.IsTableValuedFunction)
            SetImpossible("Column is part of a table valued function so cannot be altered");
    }

    public override void Execute()
    {
        base.Execute();

        var col = columnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var fansiType = col.DataType;
        var oldSqlType = fansiType.SQLType;
        var newSqlType = _datatype;

        if (newSqlType == null && !TypeText("New Data Type", "Type", 50, oldSqlType, out newSqlType))
            return;

        if (string.IsNullOrWhiteSpace(newSqlType))
            return;

        try
        {
            fansiType.AlterTypeTo(newSqlType);
        }
        catch (Exception ex)
        {
            ShowException($"Failed to Alter Type of column '{col.GetFullyQualifiedName()}'", ex);
            return;
        }

        columnInfo.Data_type = newSqlType;
        columnInfo.SaveToDatabase();

        var archive = col.Table.Database.ExpectTable($"{col.Table}_Archive");

        if (archive.Exists())
            try
            {
                var archiveCol = archive.DiscoverColumn(col.GetRuntimeName());
                if (archiveCol.DataType.SQLType.Equals(oldSqlType))
                    try
                    {
                        archiveCol.DataType.AlterTypeTo(newSqlType);
                    }
                    catch (Exception ex)
                    {
                        ShowException($"Failed to Alter Archive Column '{archiveCol.GetFullyQualifiedName()}'", ex);
                        return;
                    }
            }
            catch (Exception)
            {
                //maybe the archive is broken? corrupt or someone just happens to have a Table called that?
                return;
            }

        Publish(columnInfo.TableInfo);
    }
}