using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class LinkedColumnInfoNodeMenu : RDMPContextMenuStrip
    {
        public LinkedColumnInfoNodeMenu(IActivateItems activator, LinkedColumnInfoNode linkedColumnInfo, RDMPCollectionCommonFunctionality collection)
            : base(activator, null, collection)
        {
            Items.Add("View Extract", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(linkedColumnInfo.ColumnInfo, ViewType.TOP_100)));
            //create right click context menu
            Items.Add("View Aggreggate", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(linkedColumnInfo.ColumnInfo, ViewType.Aggregate)));


            Items.Add(new AddLookupMenuItem(activator, "Configure new Lookup Table Relationship", null, linkedColumnInfo.ColumnInfo.TableInfo));

            Items.Add(new ToolStripSeparator());

            Items.Add(new AddJoinInfoMenuItem(activator, linkedColumnInfo.ColumnInfo.TableInfo));

            var convertToANO = new ToolStripMenuItem("Configure ANO Transform", activator.CoreIconProvider.GetImage(RDMPConcept.ANOColumnInfo), (s, e) => activator.ActivateConvertColumnInfoIntoANOColumnInfo(linkedColumnInfo.ColumnInfo));

            string reason;
            convertToANO.Enabled = linkedColumnInfo.ColumnInfo.CouldSupportConvertingToANOColumnInfo(out reason);
            Items.Add(convertToANO);

            AddCommonMenuItems();
        }
    }
}