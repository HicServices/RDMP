using System.IO;
using System.Xml;

namespace CatalogueLibrary
{
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
        XmlNodeList GetTagFromConfigurationDataXML(string tagName);
        bool HasTagInConfigurationDataXML(string tagName);
    }
}