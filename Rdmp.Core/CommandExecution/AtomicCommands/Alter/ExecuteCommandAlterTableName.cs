// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Sharing.Refactoring;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Alter;

/// <summary>
///     Renames a table in the live database
/// </summary>
public class ExecuteCommandAlterTableName : AlterTableCommandExecution
{
    public ExecuteCommandAlterTableName(IBasicActivateItems activator, ITableInfo tableInfo) : base(activator,
        tableInfo)
    {
        if (IsImpossible)
            return;

        if (!SelectSQLRefactorer.IsRefactorable(TableInfo))
            SetImpossible($"Cannot rename table because {SelectSQLRefactorer.GetReasonNotRefactorable(TableInfo)}");
    }

    public override void Execute()
    {
        base.Execute();

        if (TypeText("Rename Table (in database)", "New Name:", 500, TableInfo.GetRuntimeName(), out var newName, true))
        {
            //rename the underlying table
            Table.Rename(newName);

            var newNameFullyQualified = Table.Database.ExpectTable(newName, TableInfo.Schema).GetFullyQualifiedName();
            SelectSQLRefactorer.RefactorTableName(TableInfo, newNameFullyQualified);
        }

        Publish(TableInfo);
    }
}