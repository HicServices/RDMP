using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewCatalogueFromTableInfo : BasicUICommandExecution,IAtomicCommand
    {
        private TableInfo _tableInfo;

        public ExecuteCommandCreateNewCatalogueFromTableInfo(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _tableInfo = tableInfo;

            if(activator.CoreChildProvider.AllCatalogues.Any(c=>c.Name.Equals(tableInfo.GetRuntimeName())))
                SetImpossible("There is already a Catalogue called '" + tableInfo.GetRuntimeName() + "'");
        }


        public override void Execute()
        {
            base.Execute();

            var ui = new ConfigureCatalogueExtractabilityUI(Activator, _tableInfo, "Existing Table", null);
            ui.ShowDialog();
            var cata = ui.CatalogueCreatedIfAny;

            if (cata != null)
            {
                Publish(cata);
                Emphasise(cata);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Shortcut);
        }
    }
}