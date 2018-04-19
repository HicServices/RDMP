using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMakeCatalogueProjectSpecific : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;
        private Project _project;

        public ExecuteCommandMakeCatalogueProjectSpecific(IActivateItems itemActivator):base(itemActivator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

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
            if(target is Catalogue)
            {
                _catalogue = (Catalogue)target;
                var status = _catalogue.GetExtractabilityStatus(Activator.RepositoryLocator.DataExportRepository);
            
                if(status.IsProjectSpecific)
                    SetImpossible("Catalogue is already Project Specific");

                if(!status.IsExtractable)
                    SetImpossible("Catalogue must first be made Extractable");

                var ei = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
                if(!ei.Any())
                    SetImpossible("Catalogue has no extractable columns");

                if (ei.Count(e=>e.IsExtractionIdentifier) != 1)
                    SetImpossible("Catalogue must have exactly 1 IsExtractionIdentifier column");

                if (ei.Any(e => e.ExtractionCategory != ExtractionCategory.Core && e.ExtractionCategory != ExtractionCategory.ProjectSpecific))
                    SetImpossible("All existing ExtractionInformations must be ExtractionCategory.Core");
            }

            if (target is Project)
                _project = (Project) target;

            return this;
        }
    }
}