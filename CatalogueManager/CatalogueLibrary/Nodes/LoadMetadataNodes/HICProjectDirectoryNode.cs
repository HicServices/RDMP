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
    public class HICProjectDirectoryNode
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

        public bool CanActivate()
        {
            return !string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles) && Directory.Exists(LoadMetadata.LocationOfFlatFiles);
        }

        public void Activate()
        {
            if(CanActivate())
                Process.Start(LoadMetadata.LocationOfFlatFiles);
        }
               
    }
}
