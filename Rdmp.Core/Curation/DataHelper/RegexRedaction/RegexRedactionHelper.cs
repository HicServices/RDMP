using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using MongoDB.Driver.Core.Servers;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Rdmp.Core.Repositories;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public static class RegexRedactionHelper
    {

        //public static QueryBuilder SQLLikeQuery(ColumnInfo columnInfo, CatalogueItem[] pkCatalogueItems, RegexRedactionConfiguration regexConfiguration,int? readLimit)
        //{
        //    var memoryRepo = new MemoryCatalogueRepository();
        //    var qb = new QueryBuilder(null, null, null);
        //    qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, columnInfo));
        //    foreach(var pk in pkCatalogueItems)
        //    {
        //        qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, pk.ColumnInfo));
        //    }
        //    qb.AddCustomLine($"{columnInfo.GetRuntimeName()} LIKE '%{regexConfiguration.RegexPattern}%'", QueryComponent.WHERE);
        //    if (readLimit is not null)
        //    {
        //        qb.TopX = (int)readLimit;
        //    }

        //    return qb;
        //}

        public static DataTable GenerateRedactionsDataTable()
        {
            DataTable redactionsToSaveTable = new DataTable();
            redactionsToSaveTable.Columns.Add("RedactionConfiguration_ID");
            redactionsToSaveTable.Columns.Add("ColumnInfo_ID");
            redactionsToSaveTable.Columns.Add("startingIndex");
            redactionsToSaveTable.Columns.Add("ReplacementValue");
            redactionsToSaveTable.Columns.Add("RedactedValue");
            return redactionsToSaveTable;
        }

        public static DataTable GeneratePKDataTable()
        {
            DataTable pkDataTable = new DataTable();
            pkDataTable.Columns.Add("RegexRedaction_ID");
            pkDataTable.Columns.Add("ColumnInfo_ID");
            pkDataTable.Columns.Add("Value");
            pkDataTable.Columns.Add("ID", typeof(int));
            return pkDataTable;
        }

        public static string ConvertPotentialDateTimeObject(string value, string currentColumnType)
        {
            var matchValue = $"'{value}'";
            if (currentColumnType == "datetime2" || currentColumnType == "datetime")
            {
                var x = DateTime.Parse(value);
                var format = "yyyy-MM-dd HH:mm:ss:fff";
                matchValue = $"'{x.ToString(format)}'";
            }
            return matchValue;
        }

        public static string GetRedactionValue(string value, ColumnInfo column, DataRow m, List<CatalogueItem> _cataloguePKs, RegexRedactionConfiguration _redactionConfiguration, DataTable redactionsToSaveTable, DataTable pksToSave, DataTable redactionUpates)
        {

            Dictionary<ColumnInfo, string> pkLookup = Enumerable.Range(0, _cataloguePKs.Count).ToDictionary(i => _cataloguePKs[i].ColumnInfo, i => m[i + 1].ToString());
            var matches = Regex.Matches(value, _redactionConfiguration.RegexPattern);
            var offset = 0;
            foreach (var match in matches)
            {
                var foundMatch = match.ToString();
                var startingIndex = value.IndexOf(foundMatch);
                string replacementValue = _redactionConfiguration.RedactionString;

                var lengthDiff = (float)foundMatch.Length - replacementValue.Length;
                if (lengthDiff < 0)
                {
                    throw new Exception($"Redaction string '{_redactionConfiguration.RedactionString}' is longer than found match '{foundMatch}'.");
                }
                if (lengthDiff > 0)
                {
                    var start = (int)Math.Floor(lengthDiff / 2);
                    var end = (int)Math.Ceiling(lengthDiff / 2);
                    replacementValue = replacementValue.PadLeft(start + replacementValue.Length, '<');
                    replacementValue = replacementValue.PadRight(end + replacementValue.Length, '>');
                }
                value = value[..startingIndex] + replacementValue + value[(startingIndex + foundMatch.Length)..];
                redactionsToSaveTable.Rows.Add([_redactionConfiguration.ID, column.ID, startingIndex, replacementValue, foundMatch]);
                foreach (var pk in pkLookup)
                {
                    pksToSave.Rows.Add([redactionUpates.Rows.Count + offset, pk.Key.ID, pk.Value]);
                }
                offset++;
            }

            return value;
        }

        public static void Redact(ColumnInfo column, DataRow match, List<CatalogueItem> _cataloguePKs, RegexRedactionConfiguration _redactionConfiguration, DataTable redactionsToSaveTable, DataTable pksToSave, DataTable redactionUpates)
        {

            var redactedValue = GetRedactionValue(match[0].ToString(), column, match, _cataloguePKs, _redactionConfiguration, redactionsToSaveTable, pksToSave, redactionUpates);
            match[0] = redactedValue;
            redactionUpates.ImportRow(match);
        }

        public static void SaveRedactions(ICatalogueRepository catalogueRepo, DiscoveredTable pksToSave, DiscoveredTable redactionsToSaveTable, DiscoveredServer _server, int timeout = 30000)
        {
            //the update isn't working? and do we need the +1?

            var sql = $@"
                DECLARE @output TABLE (id1 int, inc int IDENTITY(1,1))
                INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue) OUTPUT inserted.id as id1 INTO @output
                SELECT RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue FROM {redactionsToSaveTable.GetFullyQualifiedName()};
				
				DECLARE @IDMATCHER TABLE (RegexRedaction_ID int,ColumnInfo_ID int ,Value varchar(max),ID int , id1 int , inc int)
				insert into @IDMATCHER(RegexRedaction_ID, ColumnInfo_ID,Value,ID,id1,inc)
				select RegexRedaction_ID, ColumnInfo_ID,Value,ID,id1,inc
				FROM {pksToSave.GetFullyQualifiedName()} as t1
				JOIN @output as t2 ON t1.RegexRedaction_ID+1 = t2.inc
				where t1.RegexRedaction_ID+1 = t2.inc;

				update @IDMATCHER
				set RegexRedaction_ID = id1
				where RegexRedaction_ID+1 = inc;

                INSERT INTO RegexRedactionKey(RegexRedaction_ID,ColumnInfo_ID,Value)
			    select RegexRedaction_ID,ColumnInfo_ID,Value  FROM @IDMATCHER;
            ";
           
                (catalogueRepo as TableRepository).Insert(sql, null, timeout);
        }

        public static void DoJoinUpdate(ColumnInfo column, DiscoveredTable _discoveredTable, DiscoveredServer _server, DataTable redactionUpates, DiscoveredColumn[] _discoveredPKColumns, int timeout = 30000)
        {
            var redactionTable = _discoveredTable.Database.CreateTable("TEMP_RedactionUpdates", redactionUpates);
            var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;

            var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{column.GetRuntimeName()} = t2.{column.GetRuntimeName()}", QueryComponent.SET)
        };
            foreach (var pk in _discoveredPKColumns)
            {
                sqlLines.Add(new CustomLine($"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}", QueryComponent.WHERE));
                sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pk.GetRuntimeName()), QueryComponent.JoinInfoJoin));

            }
            var sql = updateHelper.BuildUpdate(_discoveredTable, redactionTable, sqlLines);
            var conn = _server.GetConnection();
            conn.Open();
            using (var cmd = _server.GetCommand(sql, conn))
            {
                cmd.CommandTimeout = timeout;
                cmd.ExecuteNonQuery();
            }
            conn.Close();
            redactionTable.Drop();
        }
    }
}
