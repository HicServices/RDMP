using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportManager.Collections.Nodes;
using ReusableUIComponents.Icons.IconProvision;

namespace CohortManager
{
    public class CohortManagerPluginUserInterface:PluginUserInterface
    {
        public CohortManagerPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override object[] GetChildren(object model)
        {
            return null;
        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(DatabaseEntity databaseEntity)
        {
            return null;
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            return null;
        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            var cicAssocNode = o as ProjectCohortIdentificationConfigurationAssociationsNode;

            if (cicAssocNode != null)
                return
                    GetMenuArray(
                        new ExecuteCommandCreateNewCohortIdentificationConfiguration(ItemActivator).SetTarget(cicAssocNode.Project)
                        );

            return base.GetAdditionalRightClickMenuItems(o);
        }
    }
}
