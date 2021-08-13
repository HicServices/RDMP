// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using System;
using System.Drawing;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddCatalogueToCohortIdentificationSetContainer : BasicCommandExecution
    {
        private readonly CatalogueCombineable _catalogueCombineable;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        private ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer _postImportCommand;

        public bool SkipMandatoryFilterCreation { get; set; }

        public AggregateConfiguration AggregateCreatedIfAny
        {
            get
            {
                if (_postImportCommand == null)
                    return null;

                return _postImportCommand.AggregateCreatedIfAny;
            }
        }

        public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IBasicActivateItems activator, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {

            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if (targetCohortAggregateContainer.ShouldBeReadOnly(out string reason))
                SetImpossible(reason);
        }
        public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IBasicActivateItems activator,CatalogueCombineable catalogueCombineable, CohortAggregateContainer targetCohortAggregateContainer) : this(activator,targetCohortAggregateContainer)
        {
            _catalogueCombineable = catalogueCombineable;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            UpdateIsImpossibleFor(catalogueCombineable);
        }

        private void UpdateIsImpossibleFor(CatalogueCombineable catalogueCombineable)
        {
            // TODO : Allow adding if it is an API!

            if (!catalogueCombineable.ContainsAtLeastOneExtractionIdentifier)
                SetImpossible("Catalogue " + catalogueCombineable.Catalogue + " does not contain any IsExtractionIdentifier columns");
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue,OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            // if user hasn't picked a Catalogue yet
            if(_catalogueCombineable == null)
            {
                if(!SelectMany(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected))
                {
                    // user didn't pick one
                    return;
                }

                // for each catalogue they picked
                foreach (Catalogue catalogue in selected)
                {

                    var combineable = new CatalogueCombineable(catalogue);

                    UpdateIsImpossibleFor(combineable);
                    
                    if(IsImpossible)
                        throw new ImpossibleCommandException(this, ReasonCommandImpossible);

                    // add it to the cic container
                    Execute(combineable, catalogue == selected.Last());
                }
            }
            else
            {
                Execute(_catalogueCombineable,true);
            }

        }

        private void Execute(CatalogueCombineable catalogueCombineable, bool publish)
        {
            var cmd = catalogueCombineable.GenerateAggregateConfigurationFor(BasicActivator, _targetCohortAggregateContainer, !SkipMandatoryFilterCreation);
            if (cmd != null)
            {
                _postImportCommand =
                    new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(BasicActivator, cmd, _targetCohortAggregateContainer)
                    {
                        DoNotClone = true,
                        NoPublish = !publish
                    };
                _postImportCommand.Execute();
            }
        }
    }
}