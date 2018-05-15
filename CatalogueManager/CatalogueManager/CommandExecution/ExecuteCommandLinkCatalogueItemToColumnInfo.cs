using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandLinkCatalogueItemToColumnInfo : BasicUICommandExecution, IAtomicCommand
    {
        private readonly CatalogueItem _catalogueItem;
        private ColumnInfo _columnInfo;


        public ExecuteCommandLinkCatalogueItemToColumnInfo(IActivateItems activator, CatalogueItem catalogueItem): base(activator)
        {
            _catalogueItem = catalogueItem;

            if (_catalogueItem.ColumnInfo_ID != null)
                SetImpossible("ColumnInfo is already set");
        }

        public ExecuteCommandLinkCatalogueItemToColumnInfo(IActivateItems activator, ColumnInfoCommand cmd, CatalogueItem catalogueItem) : base(activator)
        {
            _catalogueItem = catalogueItem;
            
            if (catalogueItem.ColumnInfo_ID != null)
                SetImpossible( "CatalogueItem " + catalogueItem + " is already associated with a different ColumnInfo");

            if(cmd.ColumnInfos.Length >1)
            {
                SetImpossible("Only one ColumnInfo can be associated with a CatalogueItem at a time");
                return;
            }

            _columnInfo = cmd.ColumnInfos[0];
        }

        public override void Execute()
        {
            base.Execute();

            if (_columnInfo == null)
                _columnInfo = SelectOne<ColumnInfo>(Activator.RepositoryLocator.CatalogueRepository);

            if (_columnInfo == null)
                return;

            _catalogueItem.SetColumnInfo(_columnInfo);
                    
            //if it did not have a name before
            if (_catalogueItem.Name.StartsWith("New CatalogueItem"))
            {
                //give it one
                _catalogueItem.Name = _columnInfo.GetRuntimeName();
                _catalogueItem.SaveToDatabase();
            }

            //Either way refresh the catalogue item
            Publish(_catalogueItem);
        }

        public override string GetCommandName()
        {
            return "Set Column Info" + (_catalogueItem.ColumnInfo_ID == null ? "(Currently MISSING)" : "");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Problem);
        }
    }
}