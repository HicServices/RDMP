// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataExport.Data.DataTables;
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

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.MakeProjectSpecificCatalogueNormalAgain;
        }
    }
}