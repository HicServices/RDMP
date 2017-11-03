using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingCohortIdentificationConfiguration : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public CohortIdentificationConfiguration CohortIdConfig { get; set; }

        public ExecuteCommandEditExistingCohortIdentificationConfiguration(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            CohortIdConfig = (CohortIdentificationConfiguration) target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "This will take you to the Cohort Identification Configurations list and allow you to Edit and Run the selected cohort.\r\n" +
                    "You must choose an item from the list before proceeding.";

        }

        public override void Execute()
        {
            if (CohortIdConfig == null)
                SetImpossible("You must choose a Cohort Identification Configuration to edit.");

            base.Execute();
            Activator.WindowArranger.SetupEditAnything(this, CohortIdConfig);
        }
    }
}