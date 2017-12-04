using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using ReusableUIComponents.Icons.IconProvision;

namespace MyExamplePlugin
{
    public class MyPluginUserInterface:PluginUserInterface
    {
        public MyPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
        }
        
        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            if (o is Catalogue)
                return new[] { new ToolStripMenuItem("Hello World", null, (s, e) => MessageBox.Show("Hello World")) };

            return null;
        }

    }
}