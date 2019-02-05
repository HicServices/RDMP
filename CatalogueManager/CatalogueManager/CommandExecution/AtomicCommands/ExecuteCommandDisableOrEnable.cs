using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDisableOrEnable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IDisableable _target;

        public ExecuteCommandDisableOrEnable(IActivateItems itemActivator, IDisableable target):base(itemActivator)
        {
            _target = target;

            var container = _target as CohortAggregateContainer;

            //don't let them disable the root container
            if(container != null && container.IsRootContainer() && !container.IsDisabled)
                SetImpossible("You cannot disable the root container of a cic");

            var aggregateConfiguration = _target as AggregateConfiguration;
            if(aggregateConfiguration != null)
                if(!aggregateConfiguration.IsCohortIdentificationAggregate)
                    SetImpossible("Only cohort identification aggregates can be disabled");
                else
                    if(aggregateConfiguration.IsJoinablePatientIndexTable() && !aggregateConfiguration.IsDisabled)
                        SetImpossible("Joinable Patient Index Tables cannot be disabled");
        }

        public override void Execute()
        {
            base.Execute();

            _target.IsDisabled = !_target.IsDisabled;
            _target.SaveToDatabase();
            Publish((DatabaseEntity)_target);
        }

        public override string GetCommandName()
        {
            return _target.IsDisabled ? "Enable" : "Disable";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}