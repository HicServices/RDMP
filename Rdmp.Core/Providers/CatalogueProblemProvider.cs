// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

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

            if (o is ParametersNode p)
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

        public string DescribeProblem(ParametersNode parameterNode)
        {
            var emptyParams = parameterNode.Parameters.Where(p => string.IsNullOrWhiteSpace(p.Value)).ToArray();

            if (emptyParams.Any())
                return "The following parameters do not have values defined:" +
                       string.Join(",", emptyParams.Select(p => p.ParameterName));

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
            //if it's a root container don't check
            if (_childProvider.AllCohortIdentificationConfigurations.Any(c =>
                c.RootCohortAggregateContainer_ID == container.ID))
                return null;

            var children = _childProvider.GetChildren(container);

            if (children.Length == 0)
                return "Empty SET containers have no effect (and will be ignored)";

            if (children.Length == 1)
                return "SET container operations have no effect if there is only one child within";
            
            return null;
        }
    }
}
