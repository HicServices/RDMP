using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingFile:BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;

        public CatalogueFolder TargetFolder { get; set; }

        public FileInfo File { get; private set; }

        private void CheckFile()
        {
            if(File == null)
                return;

            if(!(
                File.Extension.Equals(".csv") ||
                File.Extension.Equals(".xls") ||
                File.Extension.Equals(".xlsx")
                ))
                SetImpossible("Only CSV or XLS files can be imported as New Catalogues");
        }

        public ExecuteCommandCreateNewCatalogueByImportingFile(IActivateItems activator, FileInfo file = null)
        {
            _activator = activator;
            File = file;
            CheckFile();
        }

        
        public override void Execute()
        {
            base.Execute();

            new CreateNewCatalogueByImportingFileUI(_activator, this).ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
        }

        public override string GetCommandHelp()
        {
            return "Creates a NEW Dataset and associated extractable Catalogue by importing an existing file." +
                   "\r\n" +
                   "Note: you cannot use this to import data into an existing Dataset";
        }
    }
}