using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using RDMPStartup;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ColumnInfoMenu : RDMPContextMenuStrip
    {
        private readonly ColumnInfo _columnInfo;

        public ColumnInfoMenu(RDMPContextMenuStripArgs args, ColumnInfo columnInfo)
            : base(args, columnInfo)
        {
            _columnInfo = columnInfo;

            Items.Add("View Extract", null, (s,e)=> _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo,ViewType.TOP_100)));
            //create right click context menu
            Items.Add("View Aggreggate", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo, ViewType.Aggregate)));
            
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null,columnInfo.TableInfo));

            Items.Add(new ToolStripSeparator());

            Items.Add(new AddJoinInfoMenuItem(_activator, columnInfo.TableInfo));

            var convertToANO = new ToolStripMenuItem("Configure ANO Transform", _activator.CoreIconProvider.GetImage(RDMPConcept.ANOColumnInfo), (s, e) => _activator.ActivateConvertColumnInfoIntoANOColumnInfo(columnInfo));

            string reason;
            convertToANO.Enabled = _columnInfo.CouldSupportConvertingToANOColumnInfo(out reason);
            Items.Add(convertToANO);
        }
    }
}
