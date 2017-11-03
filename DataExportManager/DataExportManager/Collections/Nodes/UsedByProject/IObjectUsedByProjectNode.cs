using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.Collections.Nodes.UsedByProject
{
    public interface IObjectUsedByProjectNode:IMasqueradeAs
    {
        Project Project { get; }

        RDMPConcept UnderlyingObjectConceptualType { get; }
        object ObjectBeingUsed { get; }
    }
}
