using System.IO;

namespace CatalogueLibrary
{
    /// <summary>
    /// Defines a rigid file structure in which there is a \Data\ diretory \ForLoading directory \ForArchiving\ etc.  This structure is used to drive the DLE and allows 
    /// for standardisation of structure within a dataset directory (i.e. we always load from ForLoading and then move them after loading into ForArchiving).  
    /// </summary>
    public interface IHICProjectDirectory
    {
        DirectoryInfo ForLoading { get; }
        DirectoryInfo ForArchiving { get; }
        DirectoryInfo ForErrors { get; }
        DirectoryInfo Cache { get; }
        DirectoryInfo RootPath { get; }
        DirectoryInfo DataPath { get; }
        DirectoryInfo ExecutablesPath { get; }
        FileInfo FTPDetails { get; }
        
        bool Test { get; }
    }
}