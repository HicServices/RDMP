// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Sharing.Refactoring;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAlterTableName : BasicUICommandExecution
    {
        private TableInfo _tableInfo;
        private SelectSQLRefactorer _refactorer;
        private DiscoveredTable _tbl;

        public ExecuteCommandAlterTableName(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _tableInfo = tableInfo;

            if (_tableInfo.IsTableValuedFunction)
            {
                SetImpossible("Table valued functions cannot be renamed");
                return;
            }

            _tbl = _tableInfo.Discover(DataAccessContext.DataLoad);

            if (!_tbl.Exists())
            {
                SetImpossible("Table '" + _tbl.GetFullyQualifiedName() + "' did not exist");
                return;
            }

            _refactorer = new SelectSQLRefactorer();

            if (!_refactorer.IsRefactorable(_tableInfo))
            {
                SetImpossible("Cannot rename table because " + _refactorer.GetReasonNotRefactorable(_tableInfo));
                return;
            }
        }

        public override void Execute()
        {
            base.Execute();

            if (TypeText("Rename Table (in database)", "New Name:", 500, _tableInfo.GetRuntimeName(), out string newName, true))
            {
                //rename the underlying table
                _tbl.Rename(newName);

                var newNameFullyQualified = _tbl.Database.ExpectTable(newName, _tableInfo.Schema).GetFullyQualifiedName();
                _refactorer.RefactorTableName(_tableInfo, newNameFullyQualified);
            }
        }
    }
}