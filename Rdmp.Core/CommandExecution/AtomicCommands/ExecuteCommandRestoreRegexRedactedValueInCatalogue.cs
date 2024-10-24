// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
                var matchValue = RegexRedactionHelper.ConvertPotentialDateTimeObject(rk.Value, pkColumnInfo.Data_type);
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
                    new($"t1.{columnInfo.GetRuntimeName()} = '{newValue}'", QueryComponent.SET)
                };
                foreach (var rk in _redaction.RedactionKeys)
                {
                    var pkColumnInfo = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(rk.ColumnInfo_ID);
                    var matchValue = RegexRedactionHelper.ConvertPotentialDateTimeObject(rk.Value, pkColumnInfo.Data_type);
                    sqlLines.Add(new CustomLine($"t1.{pkColumnInfo.GetRuntimeName()} = {matchValue}", QueryComponent.WHERE));
                    sqlLines.Add(new CustomLine($"t1.{columnInfo.GetRuntimeName()} = '{dt.Rows[0][0]}'", QueryComponent.WHERE));
                    sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pkColumnInfo.GetRuntimeName()), QueryComponent.JoinInfoJoin));
                }
                var updateSql = updateHelper.BuildUpdate(discoveredTable, discoveredTable, sqlLines);
                var conn = server.GetConnection();
                using (var cmd = server.GetCommand(updateSql, conn))
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
