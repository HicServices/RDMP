using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewSample : BasicUICommandExecution,IAtomicCommand
    {
        private readonly AggregateConfiguration _aggregate;

        public ExecuteCommandViewSample(IActivateItems activator, AggregateConfiguration aggregate):base(activator)
        {
            _aggregate = aggregate;

            if(_aggregate.IsCohortIdentificationAggregate && _aggregate.GetCohortIdentificationConfigurationIfAny() == null)
                SetImpossible("Cohort Identification Aggregate is an orphan (it's cic has been deleted)");

            UseTripleDotSuffix = true;
        }

        public override void Execute()
        {
            base.Execute();

            var cic = _aggregate.GetCohortIdentificationConfigurationIfAny();

            var collection = new ViewAggregateExtractUICollection(_aggregate);

            //if it has a cic with a query cache AND it uses joinables.  Since this is a TOP 100 select * from dataset the cache on CHI is useless only patient index tables used by this query are useful if cached
            if (cic != null && cic.QueryCachingServer_ID != null && _aggregate.PatientIndexJoinablesUsed.Any())
            {
                switch (MessageBox.Show("Use Query Cache when building query?", "Use Configured Cache", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        collection.UseQueryCache = true;
                        break;
                    case DialogResult.No:
                        collection.UseQueryCache = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Activator.ViewDataSample(collection);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Execute);
        }
    }
}