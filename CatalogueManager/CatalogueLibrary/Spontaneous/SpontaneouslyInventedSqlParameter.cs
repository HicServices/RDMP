using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Spontaneous
{
    public class SpontaneouslyInventedSqlParameter : SpontaneousObject, ISqlParameter
    {
        public string ParameterSQL { get; set; }
        public string Value { get; set; }
        
        public string Comment { get; set; }

        public SpontaneouslyInventedSqlParameter(string declarationSql,string value, string comment)
        {
            ParameterSQL = declarationSql;
            Value = value;
            Comment = comment;
        }

        public string ParameterName { get
        {
            return RDMPQuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);
        }}

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            //I am my own owner! mwahahaha
            return this;
        }
    }
}