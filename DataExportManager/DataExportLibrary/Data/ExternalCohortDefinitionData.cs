using System;
using System.Data.SqlClient;
using ADOX;
using DataExportLibrary.Interfaces.Data;

namespace DataExportLibrary.Data
{
    public class ExternalCohortDefinitionData : IExternalCohortDefinitionData
    {
        public ExternalCohortDefinitionData(SqlDataReader r, string tableName)
        {
            ExternalProjectNumber = Convert.ToInt32(r["projectNumber"]);
            ExternalDescription = r["description"].ToString();
            ExternalVersion = Convert.ToInt32(r["version"]);
            ExternalCohortTableName = tableName;
            ExternalCohortCreationDate = ObjectToNullableDateTime(r["dtCreated"]);
        }

        public int ExternalProjectNumber { get; set; }
        public string ExternalDescription { get; set; }
        public int ExternalVersion { get; set; }
        public string ExternalCohortTableName { get; set; }
        public DateTime? ExternalCohortCreationDate { get; set; }

        public DateTime? ObjectToNullableDateTime(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return (DateTime)o;
        }
    }
}