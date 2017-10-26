using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandUnlockLockable : BasicCommandExecution,IAtomicCommand
    {
        private IActivateItems _activator;
        private ILockable _lockable;

        public ExecuteCommandUnlockLockable(IActivateItems activator, ILockable lockable)
        {
            if(!lockable.LockedBecauseRunning)
                SetImpossible("Lockable is not locked");
            
            _activator = activator;
            _lockable = lockable;

        }

        public override void Execute()
        {
            base.Execute();

            
            _lockable.Unlock();

            var entity = _lockable as DatabaseEntity;

            if(entity != null)
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(entity));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.lock_break;
        }
    }
}