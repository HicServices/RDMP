using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery
{
    interface IHasLoadDiagramState
    {
        LoadDiagramState State { get; }
    }
}

