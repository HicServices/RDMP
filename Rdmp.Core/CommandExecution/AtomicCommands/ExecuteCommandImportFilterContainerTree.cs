// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Imports the entire tree from another <see cref="ISelectedDataSets"/> or <see cref="AggregateConfiguration"/> into a given <see cref="SelectedDataSets"/> (as new copies)
    /// </summary>
    public class ExecuteCommandImportFilterContainerTree : BasicCommandExecution
    {
        /// <summary>
        /// ID of the Catalogue that is being extracted by <see cref="_into"/> to ensure that we only import filters from the same table
        /// </summary>
        private readonly int _catalogue;
        private readonly SelectedDataSets _into;

        public ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator, SelectedDataSets into):base(activator)
        {
            _into = into;

            if(into.RootFilterContainer != null)
                SetImpossible("Dataset already has a root container");
            
            if(!(activator.CoreChildProvider is DataExportChildProvider))
                SetImpossible("Data export functions unavailable");

            _catalogue = _into.ExtractableDataSet.Catalogue_ID;

        }

        public override void Execute()
        {
            base.Execute();
            
            var childProvider = (DataExportChildProvider)BasicActivator.CoreChildProvider;

            // The root object that makes most sense to the user e.g. they select an extraction 
            var fromConfiguration
                =
            childProvider.AllCohortIdentificationConfigurations.Where(IsElligible)
            .Cast<DatabaseEntity>()
            .Union(childProvider.ExtractionConfigurations.Where(IsElligible)).ToList();

            if(!fromConfiguration.Any())
            {
                Show("There are no extractions or cohort builder configurations of this dataset that use filters");
                return;
            }
                

            if(SelectOne(fromConfiguration,out DatabaseEntity selected))
            {
                if(selected is ExtractionConfiguration ec)
                {
                    Import(GetElligibleChild(ec).RootFilterContainer);
                }
                if(selected is CohortIdentificationConfiguration cic)
                {
                    var chosen = SelectOne(GetElligibleChildren(cic).ToList(),null,true);

                    if(chosen != null)
                        Import(chosen.RootFilterContainer);
                }
                    
                    
            }
        }

        private void Import(IContainer from)
        {
            var factory = new DeployedExtractionFilterFactory(BasicActivator.RepositoryLocator.DataExportRepository);
            
            var newRoot = DeepClone(from,factory);
            _into.RootFilterContainer_ID = newRoot.ID;
            _into.SaveToDatabase();

            
        }

        private FilterContainer DeepClone(IContainer from, DeployedExtractionFilterFactory factory)
        {
            var newRoot = new FilterContainer(BasicActivator.RepositoryLocator.DataExportRepository);
            newRoot.Operation = from.Operation;

            //clone the subcontainers
            foreach(var container in from.GetSubContainers())
            {
                newRoot.AddChild(DeepClone(container,factory));
            }
            
            var wizard = new FilterImportWizard(BasicActivator);
            
            //clone the filters
            foreach(var filter in from.GetFilters())
                wizard.Import(newRoot,filter);

            return newRoot;
        }

        private bool IsElligible(CohortIdentificationConfiguration arg)
        {
            return GetElligibleChildren(arg).Any();
        }


        /// <summary>
        /// Returns all <see cref="AggregateConfiguration"/> from the <paramref name="arg"/> where the dataset is the same and there are filters defined
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private IEnumerable<AggregateConfiguration> GetElligibleChildren(CohortIdentificationConfiguration arg)
        {
            if(arg.RootCohortAggregateContainer_ID == null)
                return new AggregateConfiguration[0];

            return arg.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                .Where(ac=>ac.Catalogue_ID == _catalogue && ac.RootFilterContainer_ID != null);
        }
        private bool IsElligible(ExtractionConfiguration arg)
        {
            return GetElligibleChild(arg) != null;
        }

        /// <summary>
        /// Returns the <see cref="ISelectedDataSets"/> that matches the dataset <see cref="_into"/> if it is one of the datasets in the <see cref="ExtractionConfiguration"/> <paramref name="arg"/> (each dataset can only be extracted once in a given <see cref="ExtractionConfiguration"/>)
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private ISelectedDataSets GetElligibleChild(ExtractionConfiguration arg)
        {
            return arg.SelectedDataSets.FirstOrDefault(s=>s.ExtractableDataSet_ID == _into.ExtractableDataSet_ID && s.RootFilterContainer_ID !=null);
        }
    }
}
