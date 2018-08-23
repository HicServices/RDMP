using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ColumnInfoMenu : RDMPContextMenuStrip
    {
        public ColumnInfoMenu(RDMPContextMenuStripArgs args, ColumnInfo columnInfo) : base(args, columnInfo)
        {
            Items.Add("View Extract", null, (s,e)=> _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo,ViewType.TOP_100)));
            //create right click context menu
            Items.Add("View Aggreggate", null, (s, e) => _activator.ViewDataSample(new ViewColumnInfoExtractUICollection(columnInfo, ViewType.Aggregate)));
            
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null,columnInfo.TableInfo));

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandAddJoinInfo(_activator, columnInfo.TableInfo));

            Add(new ExecuteCommandAnonymiseColumnInfo(_activator, columnInfo));
            
            Add(new ExecuteCommandFindUsages(_activator,columnInfo));
        }
    }
}
