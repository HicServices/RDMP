// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents.Dialogs;
using System;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Alter
{
    internal class ExecuteCommandAlterColumnType : BasicUICommandExecution, IAtomicCommand
    {
        private ColumnInfo columnInfo;

        public ExecuteCommandAlterColumnType(IActivateItems activator, ColumnInfo columnInfo) : base(activator)
        {
            this.columnInfo = columnInfo;
        }

        public override void Execute()
        {
            base.Execute();

            var col = columnInfo.Discover(DataAccessContext.InternalDataProcessing);
            var fansiType = col.DataType;
            string oldSqlType = fansiType.SQLType;


            if (TypeText("New Data Type", "Type", 50, oldSqlType, out string newSqlType, false))
            {
                try
                {
                    fansiType.AlterTypeTo(newSqlType);
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show("Failed to Alter Type", ex);
                    return;
                }

                columnInfo.Data_type = newSqlType;
                columnInfo.SaveToDatabase();

                var archive = col.Table.Database.ExpectTable(col.Table + "_Archive");

                if (archive.Exists())
                {
                    try
                    {
                        var archiveCol = archive.DiscoverColumn(col.GetRuntimeName());
                        if (archiveCol.DataType.SQLType.Equals(oldSqlType))
                        {
                            if (YesNo($"Alter Type in Archive '{ archive.GetFullyQualifiedName()}'?", "Alter Archive"))
                                try
                                {
                                    archiveCol.DataType.AlterTypeTo(newSqlType);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionViewer.Show("Failed to Alter Archive Column", ex);
                                    return;
                                }
                        }
                    }
                    catch (Exception)
                    {
                        //maybe the archive is broken? corrupt or someone just happens to have a Table called that?
                        return;
                    }

                }


            }

            Publish(columnInfo.TableInfo);

        }
    }
}