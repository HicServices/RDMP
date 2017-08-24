using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableUIComponents.Annotations;

namespace CatalogueLibrary.Data
{
    public interface ISqlParameter : ISaveable
    {
        string ParameterName { get; }
        string ParameterSQL { get; set; }
        string Value { get; set; }
        string Comment { get; set; }

        IMapsDirectlyToDatabaseTable GetOwnerIfAny();
    }
}
