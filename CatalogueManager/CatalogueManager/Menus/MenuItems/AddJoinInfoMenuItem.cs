using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class AddJoinInfoMenuItem : ToolStripMenuItem
    {
        private IActivateItems _activator;
        private readonly TableInfo _tableInfo;

        public AddJoinInfoMenuItem(IActivateItems activator, TableInfo tableInfo)
        {
            _activator = activator;
            _tableInfo = tableInfo;
            Text = "Configure JoinInfo where '" + tableInfo + "' is a Primary Key Table";
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.JoinInfo, OverlayKind.Add);

        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _activator.ActivateJoinInfoConfiguration(this, _tableInfo);
        }
    }
}