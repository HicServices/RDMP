using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportManager.CohortUI;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    class ExecuteCommandShowDetailedSummaryOfAllCohorts:BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandShowDetailedSummaryOfAllCohorts(IActivateItems activator) : base(activator)
        {

        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.AllCohortsNode;
        }

        public override void Execute()
        {
            base.Execute();

            var extractableCohortCollection = new ExtractableCohortCollection();
            extractableCohortCollection.RepositoryLocator = Activator.RepositoryLocator;
            Activator.ShowWindow(extractableCohortCollection, true);

            extractableCohortCollection.SetupForAllCohorts(Activator);
        }
    }
}
