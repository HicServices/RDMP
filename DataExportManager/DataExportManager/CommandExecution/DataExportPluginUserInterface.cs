using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using DataExportLibrary.Data;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using ReusableLibraryCode;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution
{
    public class DataExportPluginUserInterface:PluginUserInterface
    {
        
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
            var factory = new AtomicCommandUIFactory(ItemActivator.CoreIconProvider);

            if (cata != null)
            {
                return new[]
                {
                    factory.CreateMenuItem(new ExecuteCommandChangeExtractability(ItemActivator, cata))
                
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
