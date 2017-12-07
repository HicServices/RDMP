using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableUIComponents.Annotations;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Class for persisting the Comment, type and value of an Sql Parameter (e.g. /*mycool variable*/ DECLARE @bob as Varchar(10); Set @bob = 'fish').  RDMP supports 
    /// parameter overriding and merging duplicate parameters etc during query building (See ParameterManager).
    /// </summary>
    public interface ISqlParameter : ISaveable
    {
        string ParameterName { get; }
        string ParameterSQL { get; set; }
        string Value { get; set; }
        string Comment { get; set; }

        IMapsDirectlyToDatabaseTable GetOwnerIfAny();
    }
}
