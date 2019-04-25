// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.CommandExecution.AtomicCommands;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMakeCatalogueProjectSpecific : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;
        private Project _project;

        [ImportingConstructor]
        public ExecuteCommandMakeCatalogueProjectSpecific(IActivateItems itemActivator,Catalogue catalogue, Project project):this(itemActivator)
        {
            SetCatalogue(catalogue);
            _project = project;
        }
        public ExecuteCommandMakeCatalogueProjectSpecific(IActivateItems itemActivator): base(itemActivator)
        {
            UseTripleDotSuffix = true;
        }

        public override string GetCommandHelp()
        {
            return "Restrict use of the dataset only to extractions of the specified Project";
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

        public override Image GetImage(IIconProvider iconProvider)
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