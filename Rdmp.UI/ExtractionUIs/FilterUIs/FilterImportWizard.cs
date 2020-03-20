// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.ExtractionUIs.FilterUIs
{
    /// <summary>
    /// Handles deploying <see cref="ExtractionFilter"/> instances into cohort identification / data extraction <see cref="IContainer"/>s.
    /// This adds WHERE logic to the query the user is building.  The interactive bits of this class only come into effect when there are
    /// one or more <see cref="ExtractionFilterParameterSet"/> configured that they can select from.
    /// </summary>
    public class FilterImportWizard
    {
        private readonly IActivateItems _activator;

        public FilterImportWizard(IActivateItems activator)
        {
            _activator = activator;
        }

        public IFilter Import(IContainer containerToImportOneInto, IFilter filterToImport)
        {
            ISqlParameter[] globals;
            IFilter[] otherFilters;
            GetGlobalsAndFilters(containerToImportOneInto,out globals,out otherFilters);
            return Import(containerToImportOneInto, filterToImport, globals, otherFilters);
        }

        public IFilter ImportOneFromSelection(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported)
        {
            ISqlParameter[] global;
            IFilter[] otherFilters;
            GetGlobalsAndFilters(containerToImportOneInto,out global, out otherFilters);
            return ImportOneFromSelection(containerToImportOneInto, filtersThatCouldBeImported, global, otherFilters);
        }

        private IFilter Import(IContainer containerToImportOneInto, IFilter filterToImport,ISqlParameter[] globalParameters, IFilter[] otherFiltersInScope)
        {
            //Sometimes filters have some recommended parameter values which the user can pick from (e.g. filter Condition could have parameter value sets for 'Dementia', 'Alzheimers' etc
            var chosenParameterValues = AdvertiseAvailableFilterParameterSetsIfAny(filterToImport as ExtractionFilter);

            FilterImporter importer = null;

            if (containerToImportOneInto is AggregateFilterContainer)
                importer = new FilterImporter(new AggregateFilterFactory((ICatalogueRepository)containerToImportOneInto.Repository), globalParameters);
            else if (containerToImportOneInto is FilterContainer)
                importer =
                    new FilterImporter(new DeployedExtractionFilterFactory((IDataExportRepository) containerToImportOneInto.Repository), globalParameters);
            else
                throw new ArgumentException("Cannot import into IContainer of type " + containerToImportOneInto.GetType().Name, "containerToImportOneInto");

            //if there is a parameter value set then tell the importer to use these parameter values instead of the IFilter's default ones
            if (chosenParameterValues != null)
                importer.AlternateValuesToUseForNewParameters = chosenParameterValues.GetAllParameters();

            //create the filter
            var newFilter = importer.ImportFilter(filterToImport, otherFiltersInScope);

            //if we used custom parameter values we should update the filter name so the user is reminded that the concept of the filter includes both 'Condition' and the value they selected e.g. 'Dementia'
            if (chosenParameterValues != null)
            {
                newFilter.Name += "_" + chosenParameterValues.Name;
                newFilter.SaveToDatabase();
            }

            return newFilter;
        }

        private IFilter ImportOneFromSelection(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported,ISqlParameter[] globalParameters,IFilter[] otherFiltersInScope)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator, filtersThatCouldBeImported, false, false);
            if (dialog.ShowDialog() == DialogResult.OK && dialog.Selected != null)
            {
                var chosenFilter = (IFilter)dialog.Selected;
                return Import(containerToImportOneInto, chosenFilter, globalParameters, otherFiltersInScope);
            }

            return null;//user chose not to import anything
        }

        private ExtractionFilterParameterSet AdvertiseAvailableFilterParameterSetsIfAny(ExtractionFilter extractionFilterOrNull)
        {
            if (extractionFilterOrNull == null)
                return null;

            var parameterSets = extractionFilterOrNull.Repository.GetAllObjectsWithParent<ExtractionFilterParameterSet>(extractionFilterOrNull);

            if(parameterSets.Any())
                if(MessageBox.Show("Filter " + extractionFilterOrNull + " has some preconfigured values for parameters that represent useful configurations for this filter, would you use one of these?  If you change your mind you can still choose 'Select Null'","Use curated parameter set values?",MessageBoxButtons.YesNo)== DialogResult.Yes)
                {
                    var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator, parameterSets, true, false);
                    if (dialog.ShowDialog() == DialogResult.OK)
                        return dialog.Selected as ExtractionFilterParameterSet;

                }

            return null;
        }

        private void GetGlobalsAndFilters(IContainer containerToImportOneInto,out ISqlParameter[] globals, out IFilter[] otherFilters)
        {
            var aggregatecontainer = containerToImportOneInto as AggregateFilterContainer;
            var filtercontainer = containerToImportOneInto as FilterContainer;
            

            if (aggregatecontainer != null)
            {
                var aggregate = aggregatecontainer.GetAggregate();
                var factory = new AggregateBuilderOptionsFactory();
                var options = factory.Create(aggregate);

                globals = options.GetAllParameters(aggregate);
                var root = aggregate.RootFilterContainer;
                otherFilters = root == null ? new IFilter[0] : GetAllFiltersRecursively(root, new List<IFilter>()).ToArray();    
                return;
            }
            
            if(filtercontainer != null)
            {
                var selectedDataSet = filtercontainer.GetSelectedDataSetsRecursively();
                var config = selectedDataSet.ExtractionConfiguration;
                var root = selectedDataSet.RootFilterContainer;

                globals = config.GlobalExtractionFilterParameters;
                otherFilters = root == null ? new IFilter[0] : GetAllFiltersRecursively(root, new List<IFilter>()).ToArray();    

                return;
            }
            

            throw new Exception("Container " + containerToImportOneInto + " was an unexpected Type:" + containerToImportOneInto.GetType().Name);
        }

        private List<IFilter> GetAllFiltersRecursively(IContainer currentContainer, List<IFilter> foundSoFar)
        {
            foreach (var container in currentContainer.GetSubContainers())
                foundSoFar.AddRange(GetAllFiltersRecursively(container, foundSoFar));

            foundSoFar.AddRange(currentContainer.GetFilters());

            return foundSoFar;
        }
    }
}