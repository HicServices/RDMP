using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandMakeProjectSpecificCatalogueNormalAgain : BasicUICommandExecution,IAtomicCommand
    {
        private Catalogue _catalogue;
        private ExtractableDataSet _extractableDataSet;

        public ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(IActivateItems activator, Catalogue catalogue):base(activator)
        {
            _catalogue = catalogue;

            var dataExportRepository = activator.RepositoryLocator.DataExportRepository;
            if (dataExportRepository == null)
            {
                SetImpossible("Data Export functionality is not available");
                return;
            }

            _extractableDataSet = dataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(catalogue).SingleOrDefault();

            if (_extractableDataSet == null)
            {
                SetImpossible("Catalogue is not extractable");
                return;
            }

            if (_extractableDataSet.Project_ID == null)
            {
                SetImpossible("Catalogue is not a project specific Catalogue");
                return;
            }
        }

        public override string GetCommandHelp()
        {
            return "Take a dataset that was previously only usable with extractions of a specific project and make it free for use in any extraction project";
        }

        public override void Execute()
        {
            base.Execute();

            _extractableDataSet.Project_ID = null;
            _extractableDataSet.SaveToDatabase();

            foreach (var ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
            {
                ei.ExtractionCategory = ExtractionCategory.Core;
                ei.SaveToDatabase();
            }

            Publish(_catalogue);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.MakeProjectSpecificCatalogueNormalAgain;
        }
    }
}