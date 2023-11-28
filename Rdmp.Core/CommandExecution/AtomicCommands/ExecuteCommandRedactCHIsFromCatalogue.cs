using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data;
using YamlDotNet.Serialization;
using System.IO;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.Data;
using static NPOI.HSSF.Util.HSSFColor;
using Rdmp.Core.Curation.Data.Defaults;
using TB.ComponentModel;
using FAnsi.Discovery.TableCreation;
using HICPlugin.Curation.Data;
using Rdmp.Core.DataFlowPipeline;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRedactCHIsFromCatalogue : BasicCommandExecution, IAtomicCommand
{

    private ICatalogue _catalouge;
    private IBasicActivateItems _activator;
    private readonly Dictionary<string, List<string>> _allowLists = new();
    public int redactionCount = 0;

    public ExecuteCommandRedactCHIsFromCatalogue(IBasicActivateItems activator, [DemandsInitialization("The catalogue to search")] ICatalogue catalogue, string allowListLocation = null) : base(activator)
    {
        _catalouge = catalogue;
        _activator = activator;
        if (!string.IsNullOrWhiteSpace(allowListLocation))
        {
            var allowListFileContent = File.ReadAllText(allowListLocation);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(allowListFileContent);
            foreach (var (cat, columns) in yamlObject)
            {
                _allowLists.Add(cat, columns);
            }
        }
    }
    private void handleFoundCHI(string foundChi, string table, string column, string columnValue)
    {
        Console.WriteLine("Found CHI!");
        redactionCount++;
        var rc = new RedactedCHI(_activator.RepositoryLocator.CatalogueRepository, foundChi, ExecuteCommandIdentifyCHIInCatalogue.WrapCHIInContext(foundChi,columnValue,20),$"{table}.{column}");
        rc.SaveToDatabase();
        var redactedValue = columnValue.Replace(foundChi, $"REDACTED_CHI_{rc.ID}");
        //TODO can be smarted about how we wrote tothe db, can share a connection etc
        var sql = $"UPDATE {table} SET {column}='{redactedValue}' where {column}='{columnValue}'";
        var server = _catalouge.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
        var conn = server.GetConnection();
        conn.Open();
        using (var cmd = server.GetCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }


    public override void Execute()
    {
        base.Execute();
        List<string> columnAllowList = new();
        if (_allowLists.TryGetValue("RDMP_ALL", out var _extractionSpecificAllowances))
            columnAllowList.AddRange(_extractionSpecificAllowances);
        if (_allowLists.TryGetValue(_catalouge.Name, out var _catalogueSpecificAllowances))
            columnAllowList.AddRange(_catalogueSpecificAllowances.ToList());
        foreach (var item in _catalouge.CatalogueItems)
        {
            if (columnAllowList.Contains(item.Name)) continue;

            var column = item.ColumnInfo.Name;
            int idxOfLastSplit = column.LastIndexOf('.');
            string table = column[..idxOfLastSplit];
            var columnName = column[(idxOfLastSplit + 1)..];
            var server = _catalouge.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
            var sql = $"SELECT {columnName} from {table}";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var cmd = server.GetCommand(sql, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(cmd);
                da.Fill(dt);
            }
            dt.EndLoadData();
            foreach (DataRow row in dt.Rows)
            {

                var value = row[dt.Columns[0].ColumnName].ToString();
                var potentialCHI = CHIColumnFinder.GetPotentialCHI(value);
                if (!string.IsNullOrWhiteSpace(potentialCHI))
                {
                    handleFoundCHI(potentialCHI, table,columnName,value);
                }
            }
        }
    }
}
