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
        private readonly bool _allowImportAsCatalogue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="allowImportAsCatalogue">true to automatically create the catalogue without showing the UI</param>
        public ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(IActivateItems activator,bool allowImportAsCatalogue) : base(activator)
        {
            this._allowImportAsCatalogue = allowImportAsCatalogue;
        }

        public override void Execute()
        {
            base.Execute();

            var importTable = new ImportSQLTable(Activator,_allowImportAsCatalogue);
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

        public override string GetCommandName()
        {
            if (!_allowImportAsCatalogue)
                return "Create New TableInfo By Importing Existing Data Table";

            return base.GetCommandName();
        }
    }
}
