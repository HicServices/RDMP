using System;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRefreshObject:BasicUICommandExecution,IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandRefreshObject(IActivateItems activator, DatabaseEntity databaseEntity) : base(activator)
        {
            _databaseEntity = databaseEntity;

            if(_databaseEntity == null)
                SetImpossible("No DatabaseEntity was specified");
        }

        public override void Execute()
        {
            base.Execute();

            _databaseEntity.RevertToDatabaseState();
            Publish(_databaseEntity);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.arrow_refresh;
        }
    }
}