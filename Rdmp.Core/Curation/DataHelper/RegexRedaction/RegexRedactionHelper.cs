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

        public static void SaveRedactions(IBasicActivateItems _activator, DataTable pksToSave, DataTable redactionsToSaveTable)
        {
            var max = 1000;
            //there will be an equal or greater number of PKs, so use that and take as many redactions as we can
            if (pksToSave.Rows.Count > max)
            {
                var read = 0;
                var minColIndex = 0;
                while (read < pksToSave.Rows.Count)
                {
                    var pkRows = pksToSave.AsEnumerable().Skip(read).Take(max);
                    var colIndex = int.Parse(pkRows.Last()[0].ToString());
                    var redactionRows = redactionsToSaveTable.AsEnumerable().Skip(minColIndex).Take(colIndex - minColIndex + 1);
                    DoInsert(_activator, redactionRows.AsEnumerable(), pkRows.AsEnumerable(), minColIndex);
                    minColIndex = colIndex;
                    read += max;
                }
            }
            else
            {
                DoInsert(_activator, redactionsToSaveTable.AsEnumerable(), pksToSave.AsEnumerable(), 0);
            }
        }

        public static void DoInsert(IBasicActivateItems _activator, IEnumerable<DataRow> redactionRows, IEnumerable<DataRow> pkRows, int offset)
        {
            string redactionString = string.Join(
                   @",
            ", redactionRows.Select(r => $"({r[0]},{r[1]},{r[2]},'{r[3]}','{r[4]}')"));
            string keysString = string.Join(
               @",
            ", pkRows.Select(r => $"({r[0]},{r[1]},'{r[2]}')"));

            var sql = $@"
        DECLARE @output TABLE (id int, inc int IDENTITY(1,1))
        INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue) OUTPUT inserted.id INTO @output
        VALUES
           {redactionString};
        DECLARE @redactionKeys TABLE (id int IDENTITY(1,1),RegexRedaction_ID int,ColumnInfo_ID int , Value varchar(max));
		INSERT INTO @redactionKeys(RegexRedaction_ID,ColumnInfo_ID,Value)
        VALUES
           {keysString};
		UPDATE t1
		SET t1.RegexRedaction_ID = t2.id
		FROM @redactionKeys as t1
		INNER JOIN @output AS t2
		ON  (t1.RegexRedaction_ID -{offset} +1) = t2.inc
		WHERE (t1.RegexRedaction_ID -{offset} +1) = t2.inc;
			INSERT INTO RegexRedactionKey
			select RegexRedaction_ID,ColumnInfo_ID,Value  FROM @redactionKeys;
			select * from RegexRedactionKey
        ";
            try
            {
                (_activator.RepositoryLocator.CatalogueRepository as TableRepository).Insert(sql, null);
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
