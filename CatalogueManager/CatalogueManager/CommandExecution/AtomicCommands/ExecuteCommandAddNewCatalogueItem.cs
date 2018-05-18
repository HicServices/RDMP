using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewCatalogueItem : BasicUICommandExecution,IAtomicCommand
    {
        private IActivateItems _activator;
        private Catalogue _catalogue;

        public ExecuteCommandAddNewCatalogueItem(IActivateItems activator, Catalogue catalogue):base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
        }

        public override void Execute()
        {
            base.Execute();
        
            MessageBox.Show("Select which column the new CatalogueItem will describe/extract", "Choose underlying Column");

            ColumnInfo colInfo;
            string text;

            if(SelectOne(_activator.CoreChildProvider.AllColumnInfos,out colInfo))
                if(TypeText("Name", "Type a name for the new CatalogueItem", 500, colInfo.GetRuntimeName(),out text))
                {
                    var ci = new CatalogueItem(_activator.RepositoryLocator.CatalogueRepository, _catalogue, "New CatalogueItem " + Guid.NewGuid());
                    ci.Name = text;
                    ci.SetColumnInfo(colInfo);
                    ci.SaveToDatabase();

                    Publish(_catalogue);   
                }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
        }
    }
}