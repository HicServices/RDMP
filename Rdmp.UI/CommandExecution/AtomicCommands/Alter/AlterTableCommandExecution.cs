using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
