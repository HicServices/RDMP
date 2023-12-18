using Microsoft.Data.SqlClient;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;

public class CHIRedactionHelpers
{

    private readonly IBasicActivateItems _activator;
    private readonly ICatalogue _catalogue;


    public CHIRedactionHelpers(IBasicActivateItems activator, ICatalogue catalogue)
    {
        _activator = activator;
        _catalogue = catalogue;
    }

    public void Redact(DataRow itemToRedact)//[foundChiValue,columnName,pkValue,pkColumnName,replacementIndex]
    {
        var foundChi = itemToRedact.ItemArray[0].ToString();
        var column = itemToRedact.ItemArray[2].ToString();
        var catalogueItem = _catalogue.CatalogueItems.Where(ci => ci.Name == column).First();
        var name = catalogueItem.ColumnInfo.Name;
        var pkValue = itemToRedact.ItemArray[3].ToString();
        var replacementIdex = int.Parse(itemToRedact.ItemArray[5].ToString());
        var table = name;
        table = ReplaceLastOccurrence(table, $".[{column}]", "");
        var pkColumn = itemToRedact.ItemArray[4].ToString().Replace(table, "").Replace(".", "");
        var rc = new RedactedCHI(_catalogue.CatalogueRepository, foundChi, replacementIdex, table, pkValue, pkColumn, $"[{column}]");
        rc.SaveToDatabase();

        var redactedString = new string('#', foundChi.Length);
        var fetchSQL = $"select TOP 1 {column} from {table} where {pkColumn} = '{pkValue}' and charindex('{foundChi}',{column},{replacementIdex}) >0";
        var existingResultsDT = new DataTable();
        var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name == $"{table}.[{column}]").First();
        var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
        using (var con = (SqlConnection)catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetConnection())
        {
            con.Open();
            var da = new SqlDataAdapter(new SqlCommand(fetchSQL, con));
            da.Fill(existingResultsDT);
            if (existingResultsDT.Rows.Count > 0 && existingResultsDT.Rows[0].ItemArray.Length > 0)
            {
                var currentContext = existingResultsDT.Rows[0].ItemArray[0].ToString();
                var newContext = currentContext.Replace(foundChi, redactedString);
                var updateSQL = $"update {table} set {column}='{newContext}' where {pkColumn} = '{pkValue}' and {column}='{currentContext}'";
                var updateCmd = new SqlCommand(updateSQL, con);
                updateCmd.ExecuteNonQuery();
            }
        }
    }


    private static string ReplaceLastOccurrence(string source, string find, string replace)
    {
        int place = source.LastIndexOf(find);

        if (place == -1)
            return source;

        return source.Remove(place, find.Length).Insert(place, replace);
    }
}
