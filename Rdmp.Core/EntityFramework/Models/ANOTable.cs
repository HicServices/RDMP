using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ANOTable")]
    public class ANOTable: DatabaseObject, ICheckable
    {
        [Key]
        public override int ID { get; set; }
        public string TableName { get; set; }
        public int Server_ID { get; set; }
        public int NumberOfIntegersToUseInAnonymousRepresentation { get; set; }
        public int NumberOfCharactersToUseInAnonymousRepresentation { get; set; }
        public string Suffix { get; set; }

        [NoMappingToDatabase]
        public ExternalDatabaseServer Server => CatalogueDbContext.GetObjectByID<ExternalDatabaseServer>(Server_ID);
        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }
        public string GetRuntimeDataType(LoadStage stage)
        {
            return "TODO";
        }

        public override string ToString() => TableName;
        public DiscoveredTable GetPushedTable()
        {
            if (!Server.WasCreatedBy(new ANOStorePatcher()))
                throw new Exception($"ANOTable's Server '{Server}' is not an ANOStore.  ANOTable was '{this}'");

            var tables = DataAccessPortal
                .ExpectDatabase(Server, DataAccessContext.DataLoad)
                .DiscoverTables(false);

            return tables.SingleOrDefault(t => t.GetRuntimeName().Equals(TableName));
        }
        public void PushToANOServerAsNewTable(string identifiableDatatype, ICheckNotifier notifier,
      DbConnection forceConnection = null, DbTransaction forceTransaction = null)
        {
            var server = DataAccessPortal.ExpectServer(Server, DataAccessContext.DataLoad);

            //matches varchar(100) and has capture group 100
            var regexGetLengthOfCharType = new Regex(@".*char.*\((\d*)\)");
            var match = regexGetLengthOfCharType.Match(identifiableDatatype);

            //if user supplies varchar(100) and says he wants 3 ints and 3 chars in his anonymous identifiers he will soon run out of combinations

            if (match.Success)
            {
                var length = Convert.ToInt32(match.Groups[1].Value);

                if (length >
                    NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"You asked to create a table with a datatype of length {length}({identifiableDatatype}) but you did not allocate an equal or greater number of anonymous identifier types (NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation={NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation})",
                            CheckResult.Warning));
            }

            var con = forceConnection ?? server.GetConnection(); //use the forced connection or open a new one

            try
            {
                if (forceConnection == null)
                    con.Open();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"Could not connect to ano server {Server}", CheckResult.Fail,
                    e));
                return;
            }

            //if table name is ANOChi there are 2 columns Chi and ANOChi in it
            var anonymousColumnName = TableName;
            var identifiableColumnName = TableName["ANO".Length..];

            var anonymousDatatype =
                $"varchar({NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation + "_".Length + Suffix.Length})";


            var sql =
                $"CREATE TABLE {TableName}{Environment.NewLine} ({Environment.NewLine}{identifiableColumnName} {identifiableDatatype} NOT NULL,{Environment.NewLine}{anonymousColumnName} {anonymousDatatype}NOT NULL";

            sql += $@",
CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED 
(
        {identifiableColumnName} ASC
),
CONSTRAINT AK_{TableName} UNIQUE({anonymousColumnName})
)";


            using (var cmd = server.GetCommand(sql, con))
            {
                cmd.Transaction = forceTransaction;

                notifier.OnCheckPerformed(new CheckEventArgs($"Decided appropriate create statement is:{cmd.CommandText}",
                    CheckResult.Success));
                try
                {
                    cmd.ExecuteNonQuery();

                    if (forceConnection == null) //if we opened this ourselves
                        con.Close(); //shut it
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"Failed to successfully create the anonymous/identifier mapping Table in the ANO database on server {Server}",
                            CheckResult.Fail, e));
                    return;
                }
            }

            try
            {
                if (forceTransaction ==
                    null) //if there was no transaction then this has hit the LIVE ANO database and is for real, so save the ANOTable such that it is synchronized with reality
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Saving state because table has been pushed",
                        CheckResult.Success));
                    /*SaveToDatabase*/
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to save state after table was successfully? pushed to ANO server", CheckResult.Fail, e));
            }
        }
        public void DeleteANOTableInANOStore()
        {
            //RevertToDatabaseState();

            var s = Server;
            if (string.IsNullOrWhiteSpace(s.Name) || string.IsNullOrWhiteSpace(s.Database) ||
                string.IsNullOrWhiteSpace(TableName))
                return;

            var tbl = GetPushedTable();

            if (tbl?.Exists() == true)
                if (!tbl.IsEmpty())
                    throw new Exception(
                        $"Cannot delete ANOTable because it references {TableName} which is a table on server {Server} which contains rows, deleting this reference would leave that table as an orphan, we can only delete when there are 0 rows in the table");
                else
                    tbl.Drop();
        }

    }
}
