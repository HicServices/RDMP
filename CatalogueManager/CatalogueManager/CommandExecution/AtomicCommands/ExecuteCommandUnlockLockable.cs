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
    internal class ExecuteCommandUnlockLockable : BasicUICommandExecution,IAtomicCommand
    {
        private ILockable _lockable;

        public ExecuteCommandUnlockLockable(IActivateItems activator, ILockable lockable) : base(activator)
        {
            if(!lockable.LockedBecauseRunning)
                SetImpossible("Lockable is not locked");
            
            _lockable = lockable;

        }

        public override void Execute()
        {
            base.Execute();

            
            _lockable.Unlock();

            var entity = _lockable as DatabaseEntity;

            if(entity != null)
                Publish(entity);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.lock_break;
        }
    }
}