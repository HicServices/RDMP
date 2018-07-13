using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewPermissionWindow : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private CacheProgress _cacheProgressToSetOnIfAny;

        public ExecuteCommandCreateNewPermissionWindow(IActivateItems activator):base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Add);
        }

        public override string GetCommandHelp()
        {
            return "Creates a new time window restriction on when loads can occur";
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _cacheProgressToSetOnIfAny = target as CacheProgress;
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Permission Window Name","Enter name for the PermissionWindow e.g. 'Nightly Loads'",1000);

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                
                string windowText = dialog.ResultText;
                var newWindow = new PermissionWindow(Activator.RepositoryLocator.CatalogueRepository);
                newWindow.Name = windowText;
                newWindow.SaveToDatabase();

                if(_cacheProgressToSetOnIfAny != null)
                    new ExecuteCommandSetPermissionWindow(Activator, _cacheProgressToSetOnIfAny).SetTarget(newWindow).Execute();

                Publish(newWindow);
                Activate(newWindow);
            }
        }
    }
}