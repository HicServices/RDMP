using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Mutilators.Dilution
{
    public interface IDilutionOperation:ICheckable
    {
        IPreLoadDiscardedColumn ColumnToDilute { set; }

        string GetMutilationSql();
    }
}
