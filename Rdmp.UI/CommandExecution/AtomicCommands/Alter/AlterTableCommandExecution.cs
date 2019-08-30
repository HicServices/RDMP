// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Alter
{
    public abstract class AlterTableCommandExecution :BasicUICommandExecution
    {
        protected TableInfo TableInfo;
        protected DiscoveredTable Table;

        protected AlterTableCommandExecution(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            TableInfo = tableInfo;
            Table = TableInfo.Discover(DataAccessContext.InternalDataProcessing);
                        
            if (!Table.Exists())
            {
                SetImpossible("Table does not exist");
                return;
            }

            if(Table.TableType != TableType.Table)
            {
                SetImpossible("Table is a " + Table.TableType);
                return;
            }
        }
        
        protected bool Synchronize()
        {
            var sync = new TableInfoSynchronizer(TableInfo);
            return sync.Synchronize(new AcceptAllCheckNotifier());
        }
    }

}
