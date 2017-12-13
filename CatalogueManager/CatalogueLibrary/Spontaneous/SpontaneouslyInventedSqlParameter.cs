using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) implementation of ISqlParameter.  This class is used extensively when there is a need to inject new ISqlParameters into an ISqlQueryBuilder
    /// at runtime (or a ParameterManager).  The most common use case for this is merging two or more ISqlParameters that have the exact same declaration/value into a single
    /// new one (which will be SpontaneouslyInventedSqlParameter to prevent changes to the originals).
    /// </summary>
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