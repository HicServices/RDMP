using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCloneConfiguration : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateItems activator;
        private CohortIdentificationConfiguration _cic;
        private Project _project;

        public ExecuteCommandCloneConfiguration(IActivateItems activator) : base(activator)
        {
            this.activator = activator;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is CohortIdentificationConfiguration)
                _cic = (CohortIdentificationConfiguration)target;

            if (target is Project)
                _project = (Project)target;

            return this;
        }

        public override void Execute()
        {
            base.Execute();
            if (MessageBox.Show(
                    "This will create a 100% copy of the entire CohortIdentificationConfiguration including all datasets, " +
                    "filters, parameters and set operations. Are you sure this is what you want?",
                    "Confirm Cloning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var checks = new PopupChecksUI("Cloning " + _cic, false);
                var clone = _cic.CreateClone(checks);

                if (_project != null) // clone the association
                    new ProjectCohortIdentificationConfigurationAssociation(
                                    Activator.RepositoryLocator.DataExportRepository,
                                    _project,
                                    clone);

                //Load the clone up
                Publish(clone);
                if (_project != null)
                    Emphasise(_project);
                else
                    Emphasise(clone);
            }
        }
    }
}