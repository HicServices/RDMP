using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Menus;

public class DatasetMenu: RDMPContextMenuStrip
{

    public DatasetMenu(RDMPContextMenuStripArgs args, Dataset dataset): base(args, dataset)
    {
        Add(new ExecuteCommandDeleteDataset(_activator,dataset));
    }
}
