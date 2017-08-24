using System.Windows.Forms;
using BrightIdeasSoftware;
using RDMPObjectVisualisation.Copying;

namespace CatalogueManager.Collections.Providers.Copying
{
    public class CopyPasteProvider
    {
        private TreeListView _tree;

        public void RegisterEvents(TreeListView tree)
        {
            _tree = tree;
            _tree.KeyUp += TreeOnKeyUp;
        }

        private void TreeOnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == Keys.C && keyEventArgs.Control)
            {
                RDMPCommandFactory commandFactory = new RDMPCommandFactory();

                var command = commandFactory.Create(_tree.SelectedObject);

                if (command != null)
                    Clipboard.SetDataObject(command);
            }
            
        }
    }
}
