using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingExistingDataTable:BasicUICommandExecution,IAtomicCommand
    {
        private readonly bool _autoImport;

        public ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(IActivateItems activator,bool autoImport) : base(activator)
        {
            _autoImport = autoImport;
        }

        public override void Execute()
        {
            base.Execute();

            var importTable = new ImportSQLTable(Activator,_autoImport);
            importTable.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Import);
        }

        public override string GetCommandHelp()
        {
            return "Creates a New Catalogue by associating it " +
                   "\r\n" +
                   "with an existing Dataset Table in your database";
        }
    }
}