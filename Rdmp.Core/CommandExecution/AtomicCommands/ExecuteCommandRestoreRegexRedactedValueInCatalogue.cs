using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver.Core.Servers;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRestoreRegexRedactedValueInCatalogue : BasicCommandExecution, IAtomicCommand
    {

        private readonly RegexRedaction _redaction;
        private readonly IBasicActivateItems _activator;

        public ExecuteCommandRestoreRegexRedactedValueInCatalogue(IBasicActivateItems activator, RegexRedaction redaction)
        {
            _activator = activator;
            _redaction = redaction;
        }


        public override void Execute()
        {
            base.Execute();
            var memoryRepo = new MemoryCatalogueRepository();
            var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(_redaction.ColumnInfo_ID);
            var pks = _redaction.RedactionKeys.Select(pk => _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(pk.ColumnInfo_ID));
            var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
            var server = catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
            DiscoveredTable discoveredTable = columnInfo.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
            DiscoveredColumn[] discoveredColumns = discoveredTable.DiscoverColumns();
            var qb = new QueryBuilder(null, null, null);
            qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, columnInfo));
            foreach (var rk in _redaction.RedactionKeys)
            {
                var pkColumnInfo = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(rk.ColumnInfo_ID);
                var matchValue = $"'{rk.Value}'";
                if (pkColumnInfo.Data_type == "datetime2" || pkColumnInfo.Data_type == "datetime")
                {
                    var x = DateTime.Parse(rk.Value);
                    var format = "yyyy-MM-dd HH:mm:ss:fff";
                    matchValue = $"'{x.ToString(format)}'";
                }
                qb.AddCustomLine($"{pkColumnInfo.Name} = {matchValue}", QueryComponent.WHERE);
            }

            var sql = qb.SQL;
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var cmd = server.GetCommand(sql, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(cmd);
                da.Fill(dt);
            }
            if (dt.Rows.Count > 1)
            {
                throw new Exception("More than 1 matching redaction for this configuration. Something has gone wrong...");
            }
            string newValue = dt.Rows[0][0].ToString();
            if (newValue.IndexOf(_redaction.ReplacementValue, _redaction.StartingIndex, _redaction.ReplacementValue.Length) == _redaction.StartingIndex)
            {
                newValue = newValue.Remove(_redaction.StartingIndex, _redaction.ReplacementValue.Length).Insert(_redaction.StartingIndex, _redaction.RedactedValue);
                var updateHelper = server.GetQuerySyntaxHelper().UpdateHelper;
                var sqlLines = new List<CustomLine>
                {
                    new CustomLine($"t1.{columnInfo.GetRuntimeName()} = '{newValue}'", QueryComponent.SET)
                };
                foreach (var rk in _redaction.RedactionKeys)
                {
                    var pkColumnInfo = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(rk.ColumnInfo_ID);
                    var matchValue = $"'{rk.Value}'";
                    if (pkColumnInfo.Data_type == "datetime2" || pkColumnInfo.Data_type == "datetime")
                    {
                        var x = DateTime.Parse(rk.Value);
                        var format = "yyyy-MM-dd HH:mm:ss:fff";
                        matchValue = $"'{x.ToString(format)}'";
                    }
                    sqlLines.Add(new CustomLine($"t1.{pkColumnInfo.GetRuntimeName()} = {matchValue}", QueryComponent.WHERE));
                    sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pkColumnInfo.GetRuntimeName()), QueryComponent.JoinInfoJoin));
                }
                var updateSql = updateHelper.BuildUpdate(discoveredTable, discoveredTable, sqlLines);
                var conn = server.GetConnection();
                using (var cmd = server.GetCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                _redaction.DeleteInDatabase();
            }
            else
            {
                throw new Exception("Original redaction cannot be replaced");
            }
        }
    }
}
