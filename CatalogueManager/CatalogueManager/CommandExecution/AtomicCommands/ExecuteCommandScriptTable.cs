using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandScriptTable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly TableInfo _tableInfo;

        public ExecuteCommandScriptTable(IActivateItems activator, TableInfo tableInfo):base(activator)
        {
            _tableInfo = tableInfo;
        }

        public override string GetCommandHelp()
        {
            return "Scripts table structure to Clipboard (without dependencies)";
        }

        public override void Execute()
        {
            Clipboard.SetText(_tableInfo.Discover(DataAccessContext.InternalDataProcessing).ScriptTableCreation(false,false,false));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }
    }
}