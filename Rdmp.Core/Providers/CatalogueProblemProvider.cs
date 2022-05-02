// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.Providers
{
    /// <summary>
    /// Identifies all problems with all objects found in the Catalogue database.  This only includes problems that are fast to detect at runtime.
    /// </summary>
    public class CatalogueProblemProvider : ProblemProvider
    {
        private ICoreChildProvider _childProvider;
        private HashSet<int> _orphanCatalogueItems = new HashSet<int>();
        private HashSet<int> _usedJoinables;

        /// <inheritdoc/>
        public override void RefreshProblems(ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;
            
            //Take all the catalogue items which DONT have an associated ColumnInfo (should hopefully be quite rare)
            var orphans = _childProvider.AllCatalogueItems.Where(ci => ci.ColumnInfo_ID == null);
            
            //now identify those which have an ExtractionInformation (that's a problem! they are extractable but orphaned)
            _orphanCatalogueItems = new HashSet<int>(
                orphans.Where(o => _childProvider.AllExtractionInformations.Any(ei => ei.CatalogueItem_ID == o.ID))

                //store just the ID for performance
                .Select(i=>i.ID));

            _usedJoinables = new HashSet<int>(
                childProvider.AllJoinableCohortAggregateConfigurationUse.Select(
                    ju => ju.JoinableCohortAggregateConfiguration_ID));
        }

        /// <inheritdoc/>
        protected override string DescribeProblemImpl(object o)
        {
            if (o is AllGovernanceNode)
                return DescribeProblem((AllGovernanceNode) o);

            if (o is Catalogue)
                return DescribeProblem((Catalogue)o);

            if (o is CatalogueItem)
                return DescribeProblem((CatalogueItem) o);

            if (o is LoadDirectoryNode)
                return DescribeProblem((LoadDirectoryNode) o);

            if (o is ExtractionInformation)
                return DescribeProblem((ExtractionInformation) o);

            if (o is IFilter)
                return DescribeProblem((IFilter) o);

            if (o is AggregateConfiguration)
                return DescribeProblem((AggregateConfiguration) o);

            if (o is DecryptionPrivateKeyNode)
                return DescribeProblem((DecryptionPrivateKeyNode) o);

            if (o is AllCataloguesUsedByLoadMetadataNode)
                return DescribeProblem((AllCataloguesUsedByLoadMetadataNode) o);

            if (o is ISqlParameter p)
                return DescribeProblem(p);

            if (o is CohortAggregateContainer container)
                return DescribeProblem(container);

            return null;
        }

        public string DescribeProblem(AllCataloguesUsedByLoadMetadataNode allCataloguesUsedByLoadMetadataNode)
        {
            if (!allCataloguesUsedByLoadMetadataNode.UsedCatalogues.Any())
                return "Load has no Catalogues therefore loads no tables";
            
            return null;
        }

        public string DescribeProblem(ISqlParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Value) || parameter.Value == AnyTableSqlParameter.DefaultValue)
                return "No value defined";

            
            if (AnyTableSqlParameter.HasProhibitedName(parameter))
                return "Parameter name is a reserved name for the RDMP software";

            return null;
        }

        public string DescribeProblem(DecryptionPrivateKeyNode decryptionPrivateKeyNode)
        {
            if (decryptionPrivateKeyNode.KeyNotSpecified)
                return "No RSA encryption key has been created yet";

            return null;
        }

        public string DescribeProblem(AggregateConfiguration aggregateConfiguration)
        {
            if (aggregateConfiguration.IsJoinablePatientIndexTable())
                if (!_usedJoinables.Contains(aggregateConfiguration.JoinableCohortAggregateConfiguration.ID))
                    return "Patient Index Table is not joined to any cohort sets";

            if(!aggregateConfiguration.AggregateDimensions.Any())
            {
                return "Aggregate has no dimensions.  Set an AggregateDimension to specify which column is fetched by the query.";
            }

            return null;
        }

        public string DescribeProblem(IFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.WhereSQL))
                return "Filter is blank";

            return null;
        }

        public string DescribeProblem(Catalogue catalogue)
        {
            string reason;
            if (!Catalogue.IsAcceptableName(catalogue.Name, out reason))
                return "Invalid Name:" + reason;

            return null;
        }

        /// <summary>
        /// Identifies problems with dataset governance (e.g. <see cref="Catalogue"/> which have expired <see cref="GovernancePeriod"/>)
        /// </summary>
        /// <param name="allGovernanceNode"></param>
        /// <returns></returns>
        private string DescribeProblem(AllGovernanceNode allGovernanceNode)
        {
            HashSet<int> expiredCatalogueIds = new HashSet<int>();

            //Get all expired Catalogue IDs
            foreach (KeyValuePair<int, HashSet<int>> kvp in _childProvider.GovernanceCoverage)
            {
                var gp = _childProvider.AllGovernancePeriods.Single(g => g.ID == kvp.Key);

                if (gp.IsExpired())
                    foreach (var i in kvp.Value)
                        expiredCatalogueIds.Add(i);
            }

            //Throw out any covered by a not expired one
            foreach (KeyValuePair<int, HashSet<int>> kvp in _childProvider.GovernanceCoverage)
            {
                var gp = _childProvider.AllGovernancePeriods.Single(g => g.ID == kvp.Key);

                if (!gp.IsExpired())
                    foreach (var i in kvp.Value)
                        expiredCatalogueIds.Remove(i);
            }

            var expiredCatalogues = expiredCatalogueIds.Select(id => _childProvider.AllCataloguesDictionary[id]).Where(c => !(c.IsDeprecated /* || c.IsColdStorage || c.IsInternal*/)).ToArray();

            if (expiredCatalogues.Any())
                return "Governance Expired On:" +Environment.NewLine + string.Join(Environment.NewLine, expiredCatalogues.Take(5));

            //no expired governance
            return null;
        }

        private string DescribeProblem(ExtractionInformation extractionInformation)
        {
            //Get the Catalogue that this ExtractionInformation is descended from
            var descendancy = _childProvider.GetDescendancyListIfAnyFor(extractionInformation);
            if (descendancy != null)
            {
                var catalogue = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();
                if (catalogue != null)
                {
                    //if we know the Catalogue extractability
                    
                    //ExtractionCategory.ProjectSpecific should match the Catalogue extractability.IsProjectSpecific
                    //otherwise it's a Problem

                    if (catalogue.IsProjectSpecific(null))
                    {
                        if(extractionInformation.ExtractionCategory != ExtractionCategory.ProjectSpecific)
                            return "Catalogue " + catalogue + " is Project Specific Catalogue so all ExtractionCategory should be " + ExtractionCategory.ProjectSpecific;
                    }
                    else if( extractionInformation.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                        return "ExtractionCategory is only valid when the Catalogue ('"+catalogue+"') is also ProjectSpecific";
                }
            }

            return null;
        }

        private string DescribeProblem(LoadDirectoryNode LoadDirectoryNode)
        {
            if (LoadDirectoryNode.IsEmpty)
                return "No Project Directory has been specified for the load";

            return null;
        }

        public string DescribeProblem(CatalogueItem catalogueItem)
        {
            if (_orphanCatalogueItems.Contains(catalogueItem.ID))
                return "CatalogueItem is extractable but has no associated ColumnInfo";

            return null;
        }

        public string DescribeProblem(CohortAggregateContainer container)
        {
            // Make sure if the user has the default configuration (Root, Inclusion, Exclusion) that they do not mess up the ordering and get very confused

            // if the container is inclusion make sure the user hasn't reordered the container to make it act as exclusion instead!
            if (container.Name?.Contains(ExecuteCommandCreateNewCohortIdentificationConfiguration.InclusionCriteriaName) ?? false)
            {
                // if there is a parent container
                var parents = _childProvider.GetDescendancyListIfAnyFor(container);
                if (parents != null && parents.Last() is CohortAggregateContainer parentContainer)
                {
                    // which is EXCEPT
                    if (parentContainer.Operation == SetOperation.EXCEPT)
                    {
                        // then something called 'inclusion criteria' should be the first among them
                        var first = _childProvider.GetChildren(parentContainer).OfType<IOrderable>().OrderBy(o => o.Order).FirstOrDefault();
                        if (first != null && (!first.Equals(container)))
                        {
                            return $"{container.Name} must be the first container in the parent set.  Please re-order it to be the first";
                        }
                    }
                }
            }

            //count children that are not disabled
            var children = _childProvider.GetChildren(container);
            var enabledChildren = children.Where(o => !(o is IDisableable d) || !d.IsDisabled).ToArray();

            //are there any children with the same order in this container?
            if (children.OfType<IOrderable>().GroupBy(o => o.Order).Any(g => g.Count() > 1))
                return "Child order is ambiguous, show the Order column and reorder contents";

            //check if we're looking at a root container
            if (_childProvider.AllCohortIdentificationConfigurations.Any(c =>
                c.RootCohortAggregateContainer_ID == container.ID))
            {
                //if it's a root container
                //then UNION should have at least 1
                if (enabledChildren.Length < 1 && container.Operation == SetOperation.UNION)
                    return "You must have at least one element in the root container";

                //Excepts and Intersects must have at least 2
                if (enabledChildren.Length < 2 && (container.Operation == SetOperation.EXCEPT || container.Operation == SetOperation.INTERSECT))
                    return "EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required";
            }
            else
            {
                if (UserSettings.StrictValidationForCohortBuilderContainers)
                {
                    //if it's not a root, then there should be at least 2
                    if (enabledChildren.Length == 0)
                        return "SET containers cannot be empty. Either Add a Catalogue or Disable/Delete this container if not required";


                    if (enabledChildren.Length == 1)
                        return "SET containers have no effect if there is only one child within. Either Add a Catalogue or Disable/Delete this container if not required";
                }
            }

            return null;
        }
    }
}
