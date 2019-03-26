using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    public interface IExtractableDataSetPackage : INamed, IMapsDirectlyToDatabaseTable
    {
        string Creator { get; }
    }
}