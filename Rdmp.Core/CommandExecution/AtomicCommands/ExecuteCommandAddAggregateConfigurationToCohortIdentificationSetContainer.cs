// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer :BasicCommandExecution
    {
        private readonly AggregateConfigurationCombineable _aggregateConfigurationCombineable;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;
        private readonly bool _offerCohortAggregates;
        private AggregateConfiguration[] _available;

        public AggregateConfiguration AggregateCreatedIfAny { get; private set; }

        /// <summary>
        /// True if the <see cref="AggregateConfigurationCombineable"/> passed to the constructor was a newly created one and does
        /// not need cloning.
        /// </summary>
        public bool DoNotClone { get; set; }

        private void SetCommandWeight()
        {
            if (_offerCohortAggregates)
                Weight = 0.14f;
            else
                Weight = 0.13f;
        }


        private ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems activator, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if (targetCohortAggregateContainer.ShouldBeReadOnly(out string reason))
                SetImpossible(reason);

            UseTripleDotSuffix = true;
            SetCommandWeight();
        }

        public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems activator,AggregateConfigurationCombineable aggregateConfigurationCommand, CohortAggregateContainer targetCohortAggregateContainer) : this(activator,targetCohortAggregateContainer)
        {
            _aggregateConfigurationCombineable = aggregateConfigurationCommand;

            SetCommandWeight();
        }

        /// <summary>
        /// Constructor for selecting one or more aggregates at execute time
        /// </summary>
        /// <param name="basicActivator"></param>
        /// <param name="targetCohortAggregateContainer"></param>
        /// <param name="offerCohortAggregates"></param>
        public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems basicActivator, CohortAggregateContainer targetCohortAggregateContainer, bool offerCohortAggregates) : this(basicActivator, targetCohortAggregateContainer)
        {
            if(offerCohortAggregates)
            {
                _available = BasicActivator.CoreChildProvider.AllAggregateConfigurations.Where(c =>c.IsCohortIdentificationAggregate && !c.IsJoinablePatientIndexTable()).ToArray();

                if(_available.Length == 0)
                {
                    SetImpossible("You do not currently have any cohort sets");
                }
            }
            else
            {
                _available = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>().Where(c => !c.IsCohortIdentificationAggregate).ToArray();

                if (_available.Length == 0)
                {
                    SetImpossible("You do not currently have any non-cohort AggregateConfigurations");
                }
            }

            this._offerCohortAggregates = offerCohortAggregates;

            SetCommandWeight();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return _offerCohortAggregates ? iconProvider.GetImage(RDMPConcept.CohortAggregate,OverlayKind.Add): iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
        }

        public override string GetCommandName()
        {
            // If we're explicity overriding the command name, then use that
            if (!string.IsNullOrWhiteSpace(OverrideCommandName))
                return base.GetCommandName();

            // if an execute time decision is expected then command name should reflect the kind of available objects the user can add
            if (_available?.Any() ?? false)
            {
                return _offerCohortAggregates ? "Import (Copy of) Cohort Set into container" : "Add Aggregate(s) into container";
            }

            return base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();

            if(_aggregateConfigurationCombineable == null)
            {
                // runtime decision is required

                if(_available == null || !_available.Any())
                {
                    throw new Exception("There are no available objects to add");
                }
                
                if(!BasicActivator.SelectObjects(new DialogArgs
                {
                    WindowTitle = "Add Aggregate Configuration(s) to Container",
                    TaskDescription = $"Choose which AggregateConfiguration(s) to add to the cohort container '{_targetCohortAggregateContainer.Name}'.",
                },_available,out var selected))
                {
                    // user cancelled
                    return;
                }

                foreach (AggregateConfiguration aggregateConfiguration in selected)
                {
                    var combineable = new AggregateConfigurationCombineable(aggregateConfiguration);
                    Execute(combineable, aggregateConfiguration == selected.Last());
                }
            }
            else
            {
                Execute(_aggregateConfigurationCombineable,true);
            }

            if (AggregateCreatedIfAny != null)
                Emphasise(AggregateCreatedIfAny);
        }

        private void Execute(AggregateConfigurationCombineable toAdd, bool publish)
        {

            var cic = _targetCohortAggregateContainer.GetCohortIdentificationConfiguration();

            AggregateConfiguration child = DoNotClone
                ? toAdd.Aggregate
                : cic.ImportAggregateConfigurationAsIdentifierList(toAdd.Aggregate, (a, b) => CohortCombineToCreateCommandHelper.PickOneExtractionIdentifier(BasicActivator, a, b));

            //current contents
            var contents = _targetCohortAggregateContainer.GetOrderedContents().ToArray();

            //insert it at the begining of the contents
            int minimumOrder = 0;
            if (contents.Any())
                minimumOrder = contents.Min(o => o.Order);

            //bump everyone down to make room
            _targetCohortAggregateContainer.CreateInsertionPointAtOrder(child, minimumOrder, true);
            _targetCohortAggregateContainer.AddChild(child, minimumOrder);

            if(publish)
                Publish(_targetCohortAggregateContainer);

            AggregateCreatedIfAny = child;
        }
    }
}