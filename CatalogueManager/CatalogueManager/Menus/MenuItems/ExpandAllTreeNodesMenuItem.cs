using System;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Menus.MenuItems
{
    public class ExpandAllTreeNodesMenuItem : RDMPToolStripMenuItem
    {
        private readonly IMapsDirectlyToDatabaseTable _databaseObject;

        public ExpandAllTreeNodesMenuItem(IActivateItems activator , IMapsDirectlyToDatabaseTable databaseObject):base(activator,"Expand All Nodes")
        {
            _activator = activator;
            _databaseObject = databaseObject;
            Image = CatalogueIcons.ExpandAllNodes;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(_databaseObject,100));
        }

    }
}