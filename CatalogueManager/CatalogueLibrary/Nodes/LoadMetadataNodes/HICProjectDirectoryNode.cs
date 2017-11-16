using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class HICProjectDirectoryNode: IDirectoryInfoNode
    {
        public LoadMetadata LoadMetadata { get; set; }

        public HICProjectDirectoryNode(LoadMetadata loadMetadata)
        {
            LoadMetadata = loadMetadata;
        }

        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles); } }
        

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles) ? "???" : LoadMetadata.LocationOfFlatFiles;
        }

        public DirectoryInfo GetDirectoryInfoIfAny()
        {
            if (string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles ))
                return null;

            return new DirectoryInfo(LoadMetadata.LocationOfFlatFiles);
        }
    }
}
