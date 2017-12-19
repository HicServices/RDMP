using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRename : BasicCommandExecution,IAtomicCommand
    {
        private string _newValue;
        private readonly RefreshBus _refreshBus;
        private readonly INamed _nameable;
        private bool _explicitNewValuePassed;

        public ExecuteCommandRename(RefreshBus refreshBus, INamed nameable)
        {
            _refreshBus = refreshBus;
            _nameable = nameable;
        }

        public ExecuteCommandRename(RefreshBus refreshBus, INamed nameable, string newValue):this(refreshBus,nameable)
        {
            _newValue = newValue;
            _explicitNewValuePassed = true;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();

            if (!_explicitNewValuePassed)
            {
                var dialog = new TypeTextOrCancelDialog("Rename " + _nameable.GetType().Name, "Name", 2000, _nameable.Name);
                if (dialog.ShowDialog() == DialogResult.OK)
                    _newValue = dialog.ResultText;
                else
                    return;
            }

            _nameable.Name = _newValue;
            EnsureNameIfCohortIdentificationAggregate();

            _nameable.SaveToDatabase();
            _refreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity)_nameable));

        }

        private void EnsureNameIfCohortIdentificationAggregate()
        {
            //handle Aggregates that are part of cohort identification
            var aggregate = _nameable as AggregateConfiguration;
            if (aggregate != null)
            {
                var cic = aggregate.GetCohortIdentificationConfigurationIfAny();

                if (cic != null)
                    cic.EnsureNamingConvention(aggregate);
            }
        }
    }
}