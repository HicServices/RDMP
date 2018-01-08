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
    /// <summary>
    /// Identifies Tables and Columns referenced by RDMP which have potentially problematic names.  For example columns with spaces in them or that have reserved keywords in
    /// thier names.  RDMP is pretty robust when it comes to such columns (e.g. always fully qualifying references etc) so it's not a dealbreaker but this class at least lets
    /// you find them so you can think about fixing it as a public service.
    /// </summary>
    public class DodgyNamedTableAndColumnsChecker:ICheckable
    {
        private readonly CatalogueRepository _repository;
        public DodgyNamedTableAndColumnsChecker(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public void Check(ICheckNotifier notifier)
        {
            var allTables = _repository.GetAllObjects<TableInfo>().ToDictionary(k=>k.ID);
            var allColumns = _repository.GetAllObjects<ColumnInfo>();

            foreach (TableInfo t in allTables.Values)
                if (!TableInfoImporter.IsValidEntityName(t.GetRuntimeName(),t.GetQuerySyntaxHelper()))
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo " + t.GetRuntimeName() + "(ID=" + t.ID + ")) has an invalid name",
                        CheckResult.Fail, null));


            foreach (ColumnInfo column in allColumns)
            {
                var syntax = allTables[column.TableInfo_ID].GetQuerySyntaxHelper();
                
                if (!TableInfoImporter.IsValidEntityName(column.GetRuntimeName(),syntax))
                    notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo " + column.GetRuntimeName() + "(ID=" + column.ID+ ")) has an invalid name",
                        CheckResult.Fail, null));
            }

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
