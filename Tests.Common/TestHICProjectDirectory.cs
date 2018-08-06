using System.IO;
using System.Xml;
using CatalogueLibrary;

namespace Tests.Common
{
    public class TestHICProjectDirectory :IHICProjectDirectory{
        private readonly XmlDocument _configurationData;

        public TestHICProjectDirectory(XmlDocument configurationData)
        {
            _configurationData = configurationData;
        }

        public DirectoryInfo ForLoading { get; private set; }
        public DirectoryInfo ForArchiving { get; private set; }
        public DirectoryInfo ForErrors { get; private set; }
        public DirectoryInfo Cache { get; private set; }
        public DirectoryInfo RootPath { get; private set; }
        public DirectoryInfo DataPath { get; private set; }
        public DirectoryInfo ExecutablesPath { get; private set; }
        public FileInfo FTPDetails { get; private set; }
        public bool Test { get; private set; }
        public XmlNodeList GetTagFromConfigurationDataXML(string tagName)
        {
            return _configurationData.GetElementsByTagName(tagName);
        }

        public bool HasTagInConfigurationDataXML(string tagName)
        {
            var toReturn =_configurationData.GetElementsByTagName(tagName);

            return toReturn != null && toReturn.Count>0;
        }

        public bool IsDesignTime { get; private set; }
    }
}
