using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataExportManager.CohortUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandShowSummaryOfCohorts : BasicUICommandExecution,IAtomicCommand
    {
        private readonly string _commandName;
        private readonly ExtractableCohort[] _onlyCohorts;

        public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator)
            : base(activator)
        {
            _commandName = "Show Detailed Summary of Cohorts";
        }

        public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator,CohortSourceUsedByProjectNode projectSource) : base(activator)
        {
            _commandName = "Show Detailed Summary of Project Cohorts";

            if (projectSource.IsEmptyNode)
                SetImpossible("Node is empty");
            else
                _onlyCohorts = projectSource.CohortsUsed.Select(u => u.ObjectBeingUsed).ToArray();
        }

        [ImportingConstructor]
        public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator, ExternalCohortTable externalCohortTable) : base(activator)
        {
            _commandName = "Show Detailed Summary of Cohorts";
            _onlyCohorts = activator.CoreChildProvider.GetChildren(externalCohortTable).OfType<ExtractableCohort>().ToArray();
        }

        public override string GetCommandHelp()
        {
            return "Show information about the cohort lists stored in your cohort database (number of patients etc)";
        }

        public override string GetCommandName()
        {
            return _commandName;
        }

        public override void Execute()
        {
            var extractableCohortCollection = new ExtractableCohortCollection();
            extractableCohortCollection.RepositoryLocator = Activator.RepositoryLocator;
            Activator.ShowWindow(extractableCohortCollection, true);

            if (_onlyCohorts != null)
                extractableCohortCollection.SetupFor(_onlyCohorts);
            else
                extractableCohortCollection.SetupForAllCohorts(Activator);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AllCohortsNode);
        }
    }
}