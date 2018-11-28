using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ParametersNodeMenu : RDMPContextMenuStrip
    {
        public ParametersNodeMenu(RDMPContextMenuStripArgs args, ParametersNode parameterNode): base(args, parameterNode)
        {
            var filter = parameterNode.Collector as ExtractionFilter;

            if (filter != null)
                Items.Add(new ToolStripMenuItem("Add New 'Known Good Value(s) Set'", GetImage(RDMPConcept.ExtractionFilterParameterSet, OverlayKind.Add), (s, e) => AddParameterValueSet(filter)));
        }

        private void AddParameterValueSet(ExtractionFilter filter)
        {
            var parameterSet = new ExtractionFilterParameterSet(RepositoryLocator.CatalogueRepository,filter);
            parameterSet.CreateNewValueEntries();
            Publish(filter);
            Activate(parameterSet);
        }
    }
}
