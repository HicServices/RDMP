using HICPlugin.Curation.Data;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRevertRedactedCHI : BasicCommandExecution, IAtomicCommand
{
    RedactedCHI _redactedCHI;
    IBasicActivateItems _activator;

    public ExecuteCommandRevertRedactedCHI(IBasicActivateItems activator, [DemandsInitialization("Redacted CHIto Revert")] RedactedCHI redaction) : base(activator)
    {
        _redactedCHI = redaction;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var splitidx = _redactedCHI.CHILocation.LastIndexOf('.');
        var table = _redactedCHI.CHILocation[..splitidx];
        var column = _redactedCHI.CHILocation[(splitidx + 1)..];
        var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name == _redactedCHI.CHILocation).First();
        var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
        var findSlq = $"select {column} from {table} where {column} like '%REDACTED_CHI_{_redactedCHI.ID}%';";
        var existingResultsDT = new DataTable();
        using (var con = (SqlConnection)catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetConnection()) 
        {
            con.Open();
            var da = new SqlDataAdapter(new SqlCommand(findSlq, con));
            da.Fill(existingResultsDT);
            if (existingResultsDT.Rows.Count > 0 && existingResultsDT.Rows[0].ItemArray.Length > 0)
            {
                var currentContext = existingResultsDT.Rows[0].ItemArray[0].ToString();
                var newContext = currentContext.Replace($"REDACTED_CHI_{_redactedCHI.ID}", _redactedCHI.PotentialCHI);
                var updateSQL = $"update {table} set {column}='{newContext}' where {column} = '{currentContext}'";
                var updateCmd = new SqlCommand(updateSQL, con);
                updateCmd.ExecuteNonQuery();
            }
            _redactedCHI.DeleteInDatabase();
        }
    }
}
