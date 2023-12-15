// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.Data;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRevertRedactedCHI : BasicCommandExecution, IAtomicCommand
{
    private readonly RedactedCHI _redactedCHI;
    private readonly IBasicActivateItems _activator;

    public ExecuteCommandRevertRedactedCHI(IBasicActivateItems activator, [DemandsInitialization("Redacted CHI to Revert")] RedactedCHI redaction) : base(activator)
    {
        _redactedCHI = redaction;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        //todo i think there is some duplication here with other files, may be able to use a helper
        var redactedString = new string('#', _redactedCHI.PotentialCHI.Length);
        var fetchSQL = $"select {_redactedCHI.ColumnName} from {_redactedCHI.TableName} where {_redactedCHI.PKColumnName} = '{_redactedCHI.PKValue}' and charindex('{redactedString}',{_redactedCHI.ColumnName},{_redactedCHI.ReplacementIndex}) >0";
        var existingResultsDT = new DataTable();
        var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name == $"{_redactedCHI.TableName}.{_redactedCHI.ColumnName}").First();
        var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
        using (var con = (SqlConnection)catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetConnection())
        {
            con.Open();
            var sqlCommand = new SqlCommand(fetchSQL, con);
            var da = new SqlDataAdapter(sqlCommand);
            da.Fill(existingResultsDT);
            sqlCommand.Dispose();
            da.Dispose();
            if (existingResultsDT.Rows.Count > 0 && existingResultsDT.Rows[0].ItemArray.Length > 0)
            {
                var currentContext = existingResultsDT.Rows[0].ItemArray[0].ToString();
                var newContext = currentContext.Replace(redactedString, _redactedCHI.PotentialCHI);
                var updateSQL = $"update {_redactedCHI.TableName} set {_redactedCHI.ColumnName}='{newContext}' where {_redactedCHI.PKColumnName} = '{_redactedCHI.PKValue}' and {_redactedCHI.ColumnName}='{currentContext}'";
                var updateCmd = new SqlCommand(updateSQL, con);
                updateCmd.ExecuteNonQuery();
                updateCmd.Dispose();
            }
            _redactedCHI.DeleteInDatabase();
        }
        existingResultsDT.Dispose();
    }
}
