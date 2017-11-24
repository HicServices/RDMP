using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
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
        public LinkedColumnInfoNodeMenu(RDMPContextMenuStripArgs args, LinkedColumnInfoNode linkedColumnInfo)
            : base(args, null)
        {
            Items.Add("View Extract", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(linkedColumnInfo.ColumnInfo, ViewType.TOP_100)));
            //create right click context menu
            Items.Add("View Aggreggate", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(linkedColumnInfo.ColumnInfo, ViewType.Aggregate)));
            
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null, linkedColumnInfo.ColumnInfo.TableInfo));

            Items.Add(new ToolStripSeparator());

            Items.Add(new AddJoinInfoMenuItem(_activator, linkedColumnInfo.ColumnInfo.TableInfo));

            var convertToANO = new ToolStripMenuItem("Configure ANO Transform", _activator.CoreIconProvider.GetImage(RDMPConcept.ANOColumnInfo), (s, e) => _activator.ActivateConvertColumnInfoIntoANOColumnInfo(linkedColumnInfo.ColumnInfo));

            string reason;
            convertToANO.Enabled = linkedColumnInfo.ColumnInfo.CouldSupportConvertingToANOColumnInfo(out reason);
            Items.Add(convertToANO);

            AddCommonMenuItems();
        }
    }
}