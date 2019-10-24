// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandCreateNewCohortFromCatalogue : CohortCreationCommandExecution
    {
        private ExtractionInformation _extractionIdentifierColumn;


        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator,ExtractionInformation extractionInformation) : this(activator)
        {
            if (!extractionInformation.IsExtractionIdentifier)
                SetImpossible("Column is not marked IsExtractionIdentifier");

            OverrideCommandName = "Create New Cohort From Column...";

            SetExtractionIdentifierColumn(extractionInformation);
        }

        public override string GetCommandHelp()
        {
            return "Creates a cohort using ALL of the patient identifiers in the referenced dataset";
        }

        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator, Catalogue catalogue): this(activator)
        {
            SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(catalogue));
        }

        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator): base(activator)
        {
            UseTripleDotSuffix = true;
        }

        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator, ExternalCohortTable externalCohortTable) : this(activator)
        {
            ExternalCohortTable = externalCohortTable;
        }

        public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var cata = target as Catalogue;
            var ei = target as ExtractionInformation;

            if (cata != null)
                SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(cata));

            if (ei != null)
                SetExtractionIdentifierColumn(ei);

            return base.SetTarget(target);
        }

        private ExtractionInformation GetExtractionInformationFromCatalogue(Catalogue catalogue)
        {

            var eis = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            if (eis.Count(ei => ei.IsExtractionIdentifier) != 1)
            {
                SetImpossible("Catalogue must have a single IsExtractionIdentifier column");
                return null;
            }

            return eis.Single(e => e.IsExtractionIdentifier);
        }

        private void SetExtractionIdentifierColumn(ExtractionInformation extractionInformation)
        {
            //if they are trying to set the identifier column to something that isn't marked IsExtractionIdentifier
            if (_extractionIdentifierColumn != null && !extractionInformation.IsExtractionIdentifier)
                SetImpossible("Column is not marked IsExtractionIdentifier");

            _extractionIdentifierColumn = extractionInformation;
        }

        public override void Execute()
        {
            if (_extractionIdentifierColumn == null)
            {
                var cata = SelectOne(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());

                if(cata == null)
                    return;
                SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(cata));
            }

            base.Execute();

            var request = GetCohortCreationRequest("All patient identifiers in ExtractionInformation '" + _extractionIdentifierColumn.CatalogueItem.Catalogue + "." + _extractionIdentifierColumn.GetRuntimeName() + "'  (ID=" + _extractionIdentifierColumn.ID +")");

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            request.ExtractionIdentifierColumn = _extractionIdentifierColumn;
            var configureAndExecute = GetConfigureAndExecuteControl(request, "Import column " + _extractionIdentifierColumn + " as cohort and commmit results");
            
            Activator.ShowWindow(configureAndExecute);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
        }
    }
}