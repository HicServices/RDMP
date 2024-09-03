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

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public static class RegexRedactionHelper
    {

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

        public static string GetRedactionValue(string value, ColumnInfo column, DataRow m, List<CatalogueItem> _cataloguePKs,RegexRedactionConfiguration _redactionConfiguration, DataTable redactionsToSaveTable, DataTable pksToSave, DataTable redactionUpates)
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
                if (lengthDiff < 1)
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

        public static void SaveRedactions(ICatalogueRepository catalogueRepo, DiscoveredTable pksToSave, DiscoveredTable redactionsToSaveTable)
        {
            var sql = $@"
                INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue)
                SELECT RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue FROM {redactionsToSaveTable.GetFullyQualifiedName()};
                UPDATE t1
		        SET t1.RegexRedaction_ID = t2.id
		        FROM {pksToSave.GetFullyQualifiedName()} as t1
		        INNER JOIN {redactionsToSaveTable.GetFullyQualifiedName()} AS t2
		        ON  (t1.RegexRedaction_ID +1) = t2.ID
		        WHERE (t1.RegexRedaction_ID +1) = t2.ID;
			    INSERT INTO RegexRedactionKey
			    select RegexRedaction_ID,ColumnInfo_ID,Value  FROM {pksToSave.GetFullyQualifiedName()};
            ";
            try
            {
                (catalogueRepo as TableRepository).Insert(sql, null);
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void DoJoinUpdate(ColumnInfo column, DiscoveredTable _discoveredTable, DiscoveredServer _server, DataTable redactionUpates, DiscoveredColumn[] _discoveredPKColumns)
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
                cmd.ExecuteNonQuery();
            }
            conn.Close();
            redactionTable.Drop();
        }
    }
}
