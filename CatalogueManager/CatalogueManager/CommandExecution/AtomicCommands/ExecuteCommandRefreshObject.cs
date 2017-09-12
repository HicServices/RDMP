using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRefreshObject:BasicCommandExecution,IAtomicCommand
    {
        private IActivateItems _activator;
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandRefreshObject(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _activator = activator;
            _databaseEntity = databaseEntity;

            if(_databaseEntity == null)
                SetImpossible("No DatabaseEntity was specified");
        }

        public override void Execute()
        {
            base.Execute();

            _databaseEntity.RevertToDatabaseState();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_databaseEntity));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.arrow_refresh;
        }
    }
}