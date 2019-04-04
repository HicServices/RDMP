// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDisableOrEnable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IDisableable _target;

        public ExecuteCommandDisableOrEnable(IActivateItems itemActivator, IDisableable target):base(itemActivator)
        {
            _target = target;

            var container = _target as CohortAggregateContainer;

            //don't let them disable the root container
            if(container != null && container.IsRootContainer() && !container.IsDisabled)
                SetImpossible("You cannot disable the root container of a cic");

            var aggregateConfiguration = _target as AggregateConfiguration;
            if(aggregateConfiguration != null)
                if(!aggregateConfiguration.IsCohortIdentificationAggregate)
                    SetImpossible("Only cohort identification aggregates can be disabled");
                else
                    if(aggregateConfiguration.IsJoinablePatientIndexTable() && !aggregateConfiguration.IsDisabled)
                        SetImpossible("Joinable Patient Index Tables cannot be disabled");
        }

        public override void Execute()
        {
            base.Execute();

            _target.IsDisabled = !_target.IsDisabled;
            _target.SaveToDatabase();
            Publish((DatabaseEntity)_target);
        }

        public override string GetCommandName()
        {
            return _target.IsDisabled ? "Enable" : "Disable";
        }
    }
}