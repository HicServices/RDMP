using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using DataExportManager.CommandExecution.AtomicCommands;
using ReusableLibraryCode;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution
{
    public class DataExportPluginUserInterface:PluginUserInterface
    {
        private AtomicCommandUIFactory _atomicCommandFactory;

        public DataExportPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
            
        }

        public override object[] GetChildren(object model)
        {
            return null;
        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(DatabaseEntity databaseEntity)
        {
            var cata = databaseEntity as Catalogue;

            if (cata != null)
            {
                _atomicCommandFactory = new AtomicCommandUIFactory(ItemActivator.CoreIconProvider);

                return new[]
                {
                    _atomicCommandFactory.CreateMenuItem(new ExecuteCommandChangeExtractability(ItemActivator, cata))
                
                };
            }


            return null;
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            return null;
        }
    }
}
