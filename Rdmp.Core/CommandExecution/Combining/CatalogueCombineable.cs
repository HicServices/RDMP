// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Runtime.CompilerServices;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
/// <see cref="ICombineToMakeCommand"/> for an object of type <see cref="Catalogue"/>
/// </summary>
public class CatalogueCombineable : IHasFolderCombineable
{
    public bool ContainsAtLeastOneExtractionIdentifier { get; private set; }
    public Catalogue Catalogue { get; set; }

    public CohortIdentificationConfiguration.ChooseWhichExtractionIdentifierToUseFromManyHandler
        ResolveMultipleExtractionIdentifiers
    { get; set; }

    public IHasFolder Folderable => Catalogue;

    public CatalogueCombineable(Catalogue catalogue)
    {
        Catalogue = catalogue;
        ContainsAtLeastOneExtractionIdentifier = catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Any(e => e.IsExtractionIdentifier);
    }

    public string GetSqlString() => null;

    /// <summary>
    /// Creates a new AggregateConfiguration based on the Catalogue and returns it as an <see cref="AggregateConfigurationCombineable"/>, you should only use this method during EXECUTE as you do not
    /// want to be randomly creating these as the user waves an object around over the user interface trying to decide where to drop it.
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="cohortAggregateContainer"></param>
    /// <param name="importMandatoryFilters"></param>
    /// <param name="caller"></param>
    /// <returns></returns>
    public AggregateConfigurationCombineable GenerateAggregateConfigurationFor(IBasicActivateItems activator,
        CohortAggregateContainer cohortAggregateContainer, bool importMandatoryFilters = true,
        [CallerMemberName] string caller = null)
    {
        var cic = cohortAggregateContainer.GetCohortIdentificationConfiguration();

        return cic == null ? null : GenerateAggregateConfigurationFor(activator, cic, importMandatoryFilters);
    }

    public AggregateConfigurationCombineable GenerateAggregateConfigurationFor(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic, bool importMandatoryFilters = true,
        [CallerMemberName] string caller = null)
    {
        var newAggregate = cic.CreateNewEmptyConfigurationForCatalogue(Catalogue,
            ResolveMultipleExtractionIdentifiers ??
            ((a, b) => CohortCombineToCreateCommandHelper.PickOneExtractionIdentifier(activator, a, b)),
            importMandatoryFilters);
        return new AggregateConfigurationCombineable(newAggregate);
    }
}