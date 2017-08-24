using System.Runtime.InteropServices;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandLinkCatalogueItemToColumnInfo : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly ColumnInfoCommand _cmd;
        private readonly CatalogueItem _targetModel;

        public ExecuteCommandLinkCatalogueItemToColumnInfo(IActivateItems activator, ColumnInfoCommand cmd, CatalogueItem targetModel)
        {
            _activator = activator;
            _cmd = cmd;
            _targetModel = targetModel;
            
            if (targetModel.ColumnInfo_ID != null)
                SetImpossible( "CatalogueItem " + targetModel + " is already associated with a different ColumnInfo");
        }

        public override void Execute()
        {
            base.Execute();

            var columnInfo = _cmd.ColumnInfo;
            _targetModel.SetColumnInfo(columnInfo);
                    
            //if it did not have a name before
            if (_targetModel.Name.StartsWith("New CatalogueItem"))
            {
                //give it one
                _targetModel.Name = columnInfo.GetRuntimeName();
                _targetModel.SaveToDatabase();
            }

            //Either way refresh the catalogue item
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_targetModel));
        }
    }
}