using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using DataExportLibrary.Data.DataTables;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingFile:BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private Project _project;

        public CatalogueFolder TargetFolder { get; set; }

        public FileInfo File { get; private set; }

        private void CheckFile()
        {
            if(File == null)
                return;

            if(!File.Exists)
                SetImpossible("File does not exist");
        }

        public ExecuteCommandCreateNewCatalogueByImportingFile(IActivateItems activator, FileInfo file = null) : base(activator)
        {
            File = file;
            UseTripleDotSuffix = true;
            CheckFile();
        }

        public ExecuteCommandCreateNewCatalogueByImportingFile(IActivateItems activator, FileCollectionCommand file) : base(activator)
        {
             if(file.Files.Length != 1)
             {
                 SetImpossible("Only one file can be imported at once");
                 return;
             }

            File = file.Files[0];
            UseTripleDotSuffix = true;
            CheckFile();
        }


        public override void Execute()
        {
            base.Execute();

            var dialog = new CreateNewCatalogueByImportingFileUI(Activator, this);

            if (_project != null)
                dialog.SetProjectSpecific(_project);
            
            dialog.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if(target is Project)
                _project = (Project)target;

            return this;
        }

        public override string GetCommandHelp()
        {
            return "Creates a NEW Dataset and associated extractable Catalogue by importing an existing file." +
                   "\r\n" +
                   "Note: you cannot use this to import data into an existing Dataset";
        }

    }
}