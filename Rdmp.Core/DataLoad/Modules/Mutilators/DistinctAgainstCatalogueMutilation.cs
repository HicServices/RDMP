using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

//<!-- new stuff-->
//select* from rdmp248_new as t1
//where not exists(select*
//from rdmp248 as t2
//where t1.chi = t2.chi)

//<!-- updates
//select* from rdmp248_new as t1
//inner join rdmp248 as t2
//on t2.chi = t1.chi
//where t1.count != t2.count or t1.hic_drugID != t2.hic_drugID #do all of the columns

public class DistinctAgainstCatalogueMutilation : IMutilateDataTables
{
    private DiscoveredDatabase _db;
    private LoadStage _stage;

    public void Check(ICheckNotifier notifier)
    {
        //throw new NotImplementedException();
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        //throw new NotImplementedException();
        _db = dbInfo;
        _stage = loadStage;
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    //what happens if the catalogue table is on a different server?

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var catalogues = job.LoadMetadata.GetAllCatalogues();
        var tableInfos = catalogues.SelectMany(c => c.CatalogueItems).Select(c => c.ColumnInfo.TableInfo).ToList();
        var tables = _db.DiscoverTables(false);
        foreach ( var table in tables)
        {
            var matchingTables = tableInfos.Where(t => t.GetRuntimeName() == table.GetRuntimeName()).Distinct();
            if(matchingTables.Count() > 1)
            {
                throw new Exception("too Many tables");
            }
            if (!matchingTables.Any())
            {
                throw new Exception("no table found!");
            }
            var matchingTable = matchingTables.First();

            var tableColumnNames = table.DiscoverColumns().Select(c => c.GetRuntimeName());

            var pks = matchingTable.ColumnInfos.Where(ci => ci.IsPrimaryKey);
            var nonPks = matchingTable.ColumnInfos.Where(ci => !ci.IsPrimaryKey).Select(c => c.GetRuntimeName()).Where(c=> tableColumnNames.Contains(c));
            var pkMatchClause = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}"));
            var nonPksNoMatch = String.Join(" AND ", nonPks.Select(pk => $"t1.{pk} != t2.{pk}"));
            var newAndUpdateSql = $@"
            DELETE from {table.GetFullyQualifiedName()}
			where NOT exists(
            SELECT * FROM {table.GetFullyQualifiedName()} as t1
            WHERE NOT EXISTS( SELECT *
            FROM {matchingTable.GetFullyQualifiedName()} as t2
            WHERE {pkMatchClause} )
            UNION
            SELECT t1.* FROM {table.GetFullyQualifiedName()} as t1
            INNER JOIN {matchingTable.GetFullyQualifiedName()} as t2
            on {pkMatchClause}
            where {nonPksNoMatch})
            ";
            var conn = _db.Server.GetConnection();
            conn.Open();
            using var cmd = _db.Server.GetCommand(newAndUpdateSql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        throw new NotImplementedException();
    }
}
