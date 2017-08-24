using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections.Providers
{
    public class ExpandAllTreeNodesMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        private readonly IMapsDirectlyToDatabaseTable _databaseObject;

        public ExpandAllTreeNodesMenuItem(IActivateItems activator , IMapsDirectlyToDatabaseTable databaseObject)
        {
            _activator = activator;
            _databaseObject = databaseObject;
            Text = "Expand All Nodes";
            Image = CatalogueIcons.ExpandAllNodes;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(_databaseObject,100));
        }

    }
}