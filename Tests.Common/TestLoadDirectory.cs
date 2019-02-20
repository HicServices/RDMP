// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Xml;
using CatalogueLibrary;

namespace Tests.Common
{
    public class TestLoadDirectory :ILoadDirectory{
        private readonly XmlDocument _configurationData;

        public TestLoadDirectory(XmlDocument configurationData)
        {
            _configurationData = configurationData;
        }

        public DirectoryInfo ForLoading { get; private set; }
        public DirectoryInfo ForArchiving { get; private set; }
        public DirectoryInfo Cache { get; private set; }
        public DirectoryInfo RootPath { get; private set; }
        public DirectoryInfo DataPath { get; private set; }
        public DirectoryInfo ExecutablesPath { get; private set; }
        public FileInfo FTPDetails { get; private set; }

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
