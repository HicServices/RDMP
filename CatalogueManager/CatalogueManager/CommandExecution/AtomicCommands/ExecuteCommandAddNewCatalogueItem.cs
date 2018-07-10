using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

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

        public string GetCommandHelp()
        {
            return "Creates a new virtual column in the dataset, this is the first stage to making a new column extractable or defining a new extraction transform";
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
                    Emphasise(ci,int.MaxValue);
                }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
        }
    }
}