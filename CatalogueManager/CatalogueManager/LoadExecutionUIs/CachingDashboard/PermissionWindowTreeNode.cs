using System.Windows.Forms;
using CatalogueLibrary.Data;

namespace CatalogueManager.LoadExecutionUIs.CachingDashboard
{
    public class PermissionWindowTreeNode : TreeNode
    {
        public IPermissionWindow PermissionWindow { get; private set; }

        public PermissionWindowTreeNode(IPermissionWindow permissionWindow)
        {
            PermissionWindow = permissionWindow;
            Text = permissionWindow.Name;
        }
    }
}