using System.Drawing;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CohortManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandFreezeCohortIdentificationConfiguration : BasicUICommandExecution,IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _cic;
        private readonly bool _desiredFreezeState;

        public ExecuteCommandFreezeCohortIdentificationConfiguration(IActivateItems activator, CohortIdentificationConfiguration cic, bool desiredFreezeState):base(activator)
        {
            _cic = cic;
            _desiredFreezeState = desiredFreezeState;
        }

        public override string GetCommandName()
        {
            return _desiredFreezeState ? "Freeze Configuration" : "Unfreeze Configuration";
        }

        public override void Execute()
        {
            base.Execute();

            if (_desiredFreezeState)
                _cic.Freeze();
            else
                _cic.Unfreeze();

            Publish(_cic);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.FrozenCohortIdentificationConfiguration;
        }
    }
}