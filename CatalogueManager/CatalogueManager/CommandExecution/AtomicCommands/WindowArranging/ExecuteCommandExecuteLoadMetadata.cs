using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandExecuteLoadMetadata : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public LoadMetadata LoadMetadata{ get; set; }

        [ImportingConstructor]
        public ExecuteCommandExecuteLoadMetadata(IActivateItems activator,LoadMetadata loadMetadata)
            : base(activator)
        {
            LoadMetadata = loadMetadata;


        }
        public ExecuteCommandExecuteLoadMetadata(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadMetadata);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            LoadMetadata = (LoadMetadata)target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "Run the data load configuration through RAW=>STAGING=>LIVE";
        }

        public override string GetCommandName()
        {
            return "Execute Load";
        }

        public override void Execute()
        {
            if (LoadMetadata == null)
                SetImpossible("You must choose a LoadMetadata.");

            base.Execute();
            Activator.WindowArranger.SetupEditLoadMetadata(this, LoadMetadata);
        }
    }
}