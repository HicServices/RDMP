using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary
{
    public class DodgyNamedTableAndColumnsChecker:ICheckable
    {
        private readonly CatalogueRepository _repository;
        public DodgyNamedTableAndColumnsChecker(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (TableInfo t in _repository.GetAllObjects<TableInfo>())
                if (!TableInfoImporter.IsValidEntityName(t.GetRuntimeName()))
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo " + t.GetRuntimeName() + "(ID=" + t.ID + ")) has an invalid name",
                        CheckResult.Fail, null));


            foreach (ColumnInfo column in _repository.GetAllObjects<ColumnInfo>())
                if (!TableInfoImporter.IsValidEntityName(column.GetRuntimeName()))
                    notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo " + column.GetRuntimeName() + "(ID=" + column.ID+ ")) has an invalid name",
                        CheckResult.Fail, null));

            foreach (Catalogue c in _repository.GetAllCatalogues(true))
            {
                string reasonInvalid;
                if (!Catalogue.IsAcceptableName(c.Name, out reasonInvalid))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Catalogue " + c + " has an invalid name:" + reasonInvalid, CheckResult.Fail));
            }
        }
    }
}
