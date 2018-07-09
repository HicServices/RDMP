using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMakeCatalogueProjectSpecific : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;
        private Project _project;

        [ImportingConstructor]
        public ExecuteCommandMakeCatalogueProjectSpecific(IActivateItems itemActivator,Catalogue catalogue, Project project):base(itemActivator)
        {
            SetCatalogue(catalogue);
            _project = project;
        }
        public ExecuteCommandMakeCatalogueProjectSpecific(IActivateItems itemActivator): base(itemActivator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

            if(_catalogue == null) 
                SetCatalogue(SelectOne<Catalogue>(Activator.RepositoryLocator.CatalogueRepository));

            if(_project == null)
                _project = SelectOne<Project>(Activator.RepositoryLocator.DataExportRepository);

            if(_project == null)
                return;
            
            var eds = Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).Single();

            IExtractionConfiguration alreadyInConfiguration = eds.ExtractionConfigurations.FirstOrDefault(ec => ec.Project_ID != _project.ID);

            if(alreadyInConfiguration != null)
                throw new Exception("Cannot make " + _catalogue + " Project Specific because it is already a part of ExtractionConfiguration " + alreadyInConfiguration + " (Project=" + alreadyInConfiguration.Project +") and possibly others");

            eds.Project_ID = _project.ID;
            foreach (ExtractionInformation ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            {
                ei.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                ei.SaveToDatabase();
            }
            eds.SaveToDatabase();

            Publish(_catalogue);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ProjectCatalogue;
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is Catalogue)
                SetCatalogue((Catalogue) target);

            if (target is Project)
                _project = (Project) target;

            return this;
        }

        private void SetCatalogue(Catalogue catalogue)
        {
            ResetImpossibleness();

            _catalogue = catalogue;

            if (catalogue == null)
            {
                SetImpossible("Catalogue cannot be null");
                return;
            }

            var status = _catalogue.GetExtractabilityStatus(Activator.RepositoryLocator.DataExportRepository);

            if (status.IsProjectSpecific)
                SetImpossible("Catalogue is already Project Specific");

            if (!status.IsExtractable)
                SetImpossible("Catalogue must first be made Extractable");

            var ei = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            if (!ei.Any())
                SetImpossible("Catalogue has no extractable columns");

            if (ei.Count(e => e.IsExtractionIdentifier) != 1)
                SetImpossible("Catalogue must have exactly 1 IsExtractionIdentifier column");

            if (ei.Any(e => e.ExtractionCategory != ExtractionCategory.Core && e.ExtractionCategory != ExtractionCategory.ProjectSpecific))
                SetImpossible("All existing ExtractionInformations must be ExtractionCategory.Core");
        }
    }
}