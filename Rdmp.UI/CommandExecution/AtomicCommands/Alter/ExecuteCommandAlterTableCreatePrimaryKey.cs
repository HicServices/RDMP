// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.DataAccess;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Alter
{
    public class ExecuteCommandAlterTableCreatePrimaryKey : AlterTableCommandExecution
    {
        public ExecuteCommandAlterTableCreatePrimaryKey(IActivateItems activator, TableInfo tableInfo):base(activator,tableInfo)
        {
            if(IsImpossible)
                return;

            if(Table.DiscoverColumns().Any(c=>c.IsPrimaryKey))
            {
                SetImpossible("Table already has a primary key, try synchronizing the TableInfo");
                return;
            }
        }

        public override void Execute()
        {
            base.Execute();

            Synchronize();

            if (SelectMany(TableInfo.ColumnInfos, out ColumnInfo[] selected))
            {
                var cts = new CancellationTokenSource();

                var task = Task.Run(() =>
                    Table.CreatePrimaryKey(selected.Select(c => c.Discover(DataAccessContext.DataLoad)).ToArray()));
                
                Activator.Wait("Creating Primary Key...",task, cts);

                if(task.IsFaulted)
                    ExceptionViewer.Show(task.Exception);
            }
                
            
            Synchronize();

            Publish(TableInfo);
        }
    }
}