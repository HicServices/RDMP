using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DublinCore;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandImportDublinCoreFormat : BasicUICommandExecution,IAtomicCommand
    {
        private Catalogue _target;
        private FileInfo _toImport;

        public ExecuteCommandImportDublinCoreFormat(IActivateItems activator, Catalogue catalogue):base(activator)
        {
            _target = catalogue;
        }

        public override void Execute()
        {
            base.Execute();

            if ((_toImport = _toImport?? SelectOpenFile("Dublin Core Xml|*.xml")) == null)
                return;

            var dc = new DublinCoreDefinition();
            var doc = XDocument.Load(_toImport.FullName);
            dc.LoadFrom(doc.Root);

            _target.FromDublinCore(dc);
            _target.SaveToDatabase();
            
            Publish(_target);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}