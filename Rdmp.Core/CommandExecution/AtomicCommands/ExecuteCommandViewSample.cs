// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewSample : BasicCommandExecution, IAtomicCommand
    {
        private readonly AggregateConfiguration _aggregate;

        public ExecuteCommandViewSample(IBasicActivateItems activator, AggregateConfiguration aggregate) : base(activator)
        {
            _aggregate = aggregate;

            if (_aggregate.IsCohortIdentificationAggregate && _aggregate.GetCohortIdentificationConfigurationIfAny() == null)
                SetImpossible("Cohort Identification Aggregate is an orphan (it's cic has been deleted)");

            UseTripleDotSuffix = true;
        }

        public override void Execute()
        {
            base.Execute();

            var cic = _aggregate.GetCohortIdentificationConfigurationIfAny();

            var collection = new ViewAggregateExtractUICollection(_aggregate);

            //if it has a cic with a query cache AND it uses joinables.  Since this is a TOP 100 select * from dataset the cache on CHI is useless only patient index tables used by this query are useful if cached
            if (cic != null && cic.QueryCachingServer_ID != null && _aggregate.PatientIndexJoinablesUsed.Any())
            {
                if(!BasicActivator.YesNo("Use Query Cache when building query?", "Use Configured Cache",out bool chosen))
                        return;

                collection.UseQueryCache = chosen;
            }

            BasicActivator.ShowData(collection);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Execute);
        }
    }
}