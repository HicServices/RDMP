using System.Drawing;
using System.IO;
using CatalogueLibrary.Data;
using CatalogueLibrary.DublinCore;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportInDublinCoreFormat : BasicUICommandExecution,IAtomicCommand
    {
        private readonly DublinCoreDefinition _definition;
        private FileInfo _toExport;
        readonly DublinCoreTranslater _translater = new DublinCoreTranslater();

        public ExecuteCommandExportInDublinCoreFormat(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _definition = _translater.GenerateFrom(catalogue);
        }

        public override void Execute()
        {
            base.Execute();

            if ((_toExport = _toExport??SelectSaveFile("Dublin Core Xml|*.xml")) == null)
                return;

            using (var stream = File.OpenWrite(_toExport.FullName))
                _definition.WriteXml(stream);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}