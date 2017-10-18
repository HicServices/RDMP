using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CohortManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly CohortIdentificationConfiguration _configuration;
        private CatalogueCommand _catalogue;

        public ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(IActivateItems activator, CohortIdentificationConfiguration configuration)
        {
            _activator = activator;
            _configuration = configuration;
        }

        public ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(IActivateItems activator, CohortIdentificationConfiguration configuration,CatalogueCommand catalogue):this(activator,configuration)
        {
            _catalogue = catalogue;
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
            {
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.RepositoryLocator.CatalogueRepository.GetAllCatalogues(),false,false);
                if (dialog.ShowDialog() == DialogResult.OK)
                    _catalogue = new CatalogueCommand((Catalogue) dialog.Selected);
                else
                    return;
            }
            AggregateConfigurationCommand aggregateCommand = _catalogue.GenerateAggregateConfigurationFor(_configuration);

            var joinableCommandExecution = new ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(_activator, aggregateCommand,_configuration);
            joinableCommandExecution.Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PatientIndexTable, OverlayKind.Add);
        }
    }
}