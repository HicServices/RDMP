// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using TypeGuesser;

namespace Rdmp.Core.Providers;

/// <summary>
/// Identifies all problems with all objects found in the Catalogue database.  This only includes problems that are fast to detect at runtime.
/// </summary>
public class CatalogueProblemProvider : ProblemProvider
{
    private ICoreChildProvider _childProvider;
    private HashSet<int> _orphanCatalogueItems = new();
    private HashSet<int> _usedJoinables;
    private JoinInfo[] _joinsWithMismatchedCollations = Array.Empty<JoinInfo>();

    /// <summary>
    /// Set the culture for problem provision which is culture sensitive
    /// e.g. detecting date values or leave null for the default system culture
    /// </summary>
    public CultureInfo Culture;

    /// <inheritdoc/>
    public override void RefreshProblems(ICoreChildProvider childProvider)
    {
        _childProvider = childProvider;

        //Take all the catalogue items which DON'T have an associated ColumnInfo (should hopefully be quite rare)
        var catalogueIDs = _childProvider.AllCatalogueItems.Where(ci => ci.ColumnInfo_ID == null).Select(i => i.ID).ToList();
        var extractionInfoIDs = _childProvider.AllExtractionInformations.Select(ei => ei.CatalogueItem_ID).ToList();
        var orphans = catalogueIDs.Intersect(extractionInfoIDs);
        _orphanCatalogueItems = orphans.ToHashSet<int>();
        _usedJoinables = new HashSet<int>(
       childProvider.AllJoinableCohortAggregateConfigurationUse.Select(
           ju => ju.JoinableCohortAggregateConfiguration_ID));

        _joinsWithMismatchedCollations = childProvider.AllJoinInfos.Where(j =>
            !string.IsNullOrWhiteSpace(j.PrimaryKey.Collation) &&
            !string.IsNullOrWhiteSpace(j.ForeignKey.Collation) &&

            // does not have an explicit join collation specified
            string.IsNullOrWhiteSpace(j.Collation) &&
            !string.Equals(j.PrimaryKey.Collation, j.ForeignKey.Collation)
        ).ToArray();
    }

    /// <inheritdoc/>
    protected override string DescribeProblemImpl(object o)
    {
        return o switch
        {
            AllGovernanceNode node => DescribeProblem(node),
            Catalogue catalogue => DescribeProblem(catalogue),
            CatalogueItem item => DescribeProblem(item),
            LoadDirectoryNode directoryNode => DescribeProblem(directoryNode),
            ExtractionInformation information => DescribeProblem(information),
            IFilter filter => DescribeProblem(filter),
            AggregateConfiguration configuration => DescribeProblem(configuration),
            DecryptionPrivateKeyNode keyNode => DescribeProblem(keyNode),
            AllCataloguesUsedByLoadMetadataNode metadataNode => DescribeProblem(metadataNode),
            ISqlParameter p => DescribeProblem(p),
            CohortAggregateContainer container => DescribeProblem(container),
            PipelineCompatibleWithUseCaseNode pipelineUseCaseNode => DescribeProblem(pipelineUseCaseNode),
            PipelineComponent pipelineComponent => DescibeProblem(pipelineComponent),
            _ => null
        };
    }

    public string DescibeProblem(IPipelineComponent pipelineComponent)
    {
        var value = DataFlowPipelineEngineFactory.TryCreateComponent(pipelineComponent, out var exConstruction);
        if (exConstruction is not null)
        {
            return exConstruction.Message;
        }
        if (value is null)
        {
            return "Unable to construct object";
        }
        MandatoryPropertyChecker _mandatoryChecker = new MandatoryPropertyChecker(value);
        try
        {
            _mandatoryChecker.Check(ThrowImmediatelyCheckNotifier.Quiet);
        }
        catch (Exception e)
        {
            return e.Message;
        }

        return null;
    }

    public string DescribeProblem(PipelineCompatibleWithUseCaseNode pipelineUseCaseNode)
    {
        var repo = new MemoryRepository();
        var pipeline = pipelineUseCaseNode.Pipeline;
        var useCaseNode = new PipelineCompatibleWithUseCaseNode(repo, pipeline, pipelineUseCaseNode.UseCase);
        var useCase = useCaseNode.UseCase;
        if(!useCase.IsAllowable(pipeline))
        {
            return "Something is wrong with this pipeline";
        }
        foreach(var component in pipeline.PipelineComponents)
        {
            var componentProblem = DescibeProblem(component);
            if (componentProblem != null)
            {
                return componentProblem;
            }
        }
        return null;
    }

    public static string DescribeProblem(AllCataloguesUsedByLoadMetadataNode allCataloguesUsedByLoadMetadataNode) =>
        !allCataloguesUsedByLoadMetadataNode.UsedCatalogues.Any()
            ? "Load has no Catalogues therefore loads no tables"
            : null;

    public string DescribeProblem(ISqlParameter parameter)
    {
        if (AnyTableSqlParameter.HasProhibitedName(parameter))
            return "Parameter name is a reserved name for the RDMP software";

        // if parameter has no value that's a problem
        if (string.IsNullOrWhiteSpace(parameter.Value) || parameter.Value == AnyTableSqlParameter.DefaultValue)
        {
            // unless it has ExtractionFilterParameterSets defined on it
            var desc = _childProvider.GetDescendancyListIfAnyFor(parameter);
            if (desc != null && parameter is ExtractionFilterParameter)
            {
                var filter = desc.Parents.OfType<ExtractionFilter>().FirstOrDefault();
                if (filter != null && filter.ExtractionFilterParameterSets.Any()) return null;
            }

            return "No value defined";
        }

        var v = parameter.Value;

        var g = new Guesser();

        if (Culture != null)
            g.Culture = Culture;

        g.AdjustToCompensateForValue(v);

        // if user has entered a date as the value
        if (g.Guess.CSharpType == typeof(DateTime))
            // and there are no delimiters
            if (v.All(c => c != '\'' && c != '"'))
                return "Parameter value looks like a date but is not surrounded by quotes";

        return null;
    }

    public static string DescribeProblem(DecryptionPrivateKeyNode decryptionPrivateKeyNode) =>
        decryptionPrivateKeyNode.KeyNotSpecified ? "No RSA encryption key has been created yet" : null;

    public string DescribeProblem(AggregateConfiguration aggregateConfiguration)
    {
        if (aggregateConfiguration.IsJoinablePatientIndexTable())
            if (!_usedJoinables.Contains(aggregateConfiguration.JoinableCohortAggregateConfiguration.ID))
                return "Patient Index Table is not joined to any cohort sets";

        return !aggregateConfiguration.Catalogue.IsApiCall() && !aggregateConfiguration.AggregateDimensions.Any()
            ? "Aggregate has no dimensions.  Set an AggregateDimension to specify which column is fetched by the query."
            : null;
    }

    public static string DescribeProblem(IFilter filter) =>
        string.IsNullOrWhiteSpace(filter.WhereSQL) ? "Filter is blank" : null;

    public static string DescribeProblem(Catalogue catalogue) =>
        !Catalogue.IsAcceptableName(catalogue.Name, out var reason) ? $"Invalid Name:{reason}" : null;

    /// <summary>
    /// Identifies problems with dataset governance (e.g. <see cref="Catalogue"/> which have expired <see cref="GovernancePeriod"/>)
    /// </summary>
    /// <param name="allGovernanceNode"></param>
    /// <returns></returns>
    private string DescribeProblem(AllGovernanceNode allGovernanceNode)
    {
        var expiredCatalogueIds = new HashSet<int>();

        //Get all expired Catalogue IDs
        foreach (var kvp in _childProvider.GovernanceCoverage)
        {
            var gp = _childProvider.AllGovernancePeriods.Single(g => g.ID == kvp.Key);

            if (gp.IsExpired())
                foreach (var i in kvp.Value)
                    expiredCatalogueIds.Add(i);
        }

        //Throw out any covered by a not expired one
        foreach (var kvp in _childProvider.GovernanceCoverage)
        {
            var gp = _childProvider.AllGovernancePeriods.Single(g => g.ID == kvp.Key);

            if (!gp.IsExpired())
                foreach (var i in kvp.Value)
                    expiredCatalogueIds.Remove(i);
        }

        var expiredCatalogues = expiredCatalogueIds.Select(id => _childProvider.AllCataloguesDictionary[id])
            .Where(c => !c.IsDeprecated /* || c.IsInternal*/).ToArray();

        if (expiredCatalogues.Any())
            return
                $"Governance Expired On:{Environment.NewLine}{string.Join(Environment.NewLine, expiredCatalogues.Take(5))}";

        //no expired governance
        return null;
    }

    private string DescribeProblem(ExtractionInformation extractionInformation)
    {
        //Get the Catalogue that this ExtractionInformation is descended from
        var descendancy = _childProvider.GetDescendancyListIfAnyFor(extractionInformation);
        var catalogue = descendancy?.Parents.OfType<Catalogue>().SingleOrDefault();
        if (catalogue != null)
        {
            //if we know the Catalogue extractability

            //ExtractionCategory.ProjectSpecific should match the Catalogue extractability.IsProjectSpecific
            //otherwise it's a Problem

            if (catalogue.IsProjectSpecific(null))
            {
                if (extractionInformation.ExtractionCategory != ExtractionCategory.ProjectSpecific)
                    return
                        $"Catalogue {catalogue} is Project Specific Catalogue so all ExtractionCategory should be {ExtractionCategory.ProjectSpecific}";
            }
            else if (extractionInformation.ExtractionCategory == ExtractionCategory.ProjectSpecific)
            {
                return
                    $"ExtractionCategory is only valid when the Catalogue ('{catalogue}') is also ProjectSpecific";
            }
        }

        return null;
    }

    private static string DescribeProblem(LoadDirectoryNode LoadDirectoryNode) => LoadDirectoryNode.IsEmpty
        ? "No Project Directory has been specified for the load"
        : null;

    public string DescribeProblem(CatalogueItem catalogueItem)
    {
        if (_orphanCatalogueItems.Contains(catalogueItem.ID))
            return "CatalogueItem is extractable but has no associated ColumnInfo";

        var badJoin = _joinsWithMismatchedCollations.FirstOrDefault(j =>
            j.PrimaryKey_ID == catalogueItem.ColumnInfo_ID ||
            j.ForeignKey_ID == catalogueItem.ColumnInfo_ID);

        return badJoin != null
            ? $"Columns in joins declared on this column have mismatched collations ({badJoin})"
            : null;
    }

    public string DescribeProblem(CohortAggregateContainer container)
    {
        // Make sure if the user has the default configuration (Root, Inclusion, Exclusion) that they do not mess up the ordering and get very confused

        // if the container is inclusion make sure the user hasn't reordered the container to make it act as exclusion instead!
        if (container.Name?.Contains(ExecuteCommandCreateNewCohortIdentificationConfiguration.InclusionCriteriaName) ??
            false)
        {
            // if there is a parent container
            var parents = _childProvider.GetDescendancyListIfAnyFor(container);
            if (parents?.Last() is CohortAggregateContainer { Operation: SetOperation.EXCEPT } parentContainer)
            // which is EXCEPT
            {
                // then something called 'inclusion criteria' should be the first among them
                var first = _childProvider.GetChildren(parentContainer).OfType<IOrderable>().MinBy(o => o.Order);
                if (first != null && !first.Equals(container))
                    return
                        $"{container.Name} must be the first container in the parent set.  Please re-order it to be the first";
            }
        }

        //count children that are not disabled
        var children = _childProvider.GetChildren(container);
        var enabledChildren = children.Where(o => o is not IDisableable { IsDisabled: true }).ToArray();

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
            if (enabledChildren.Length < 2 && (container.Operation == SetOperation.EXCEPT ||
                                               container.Operation == SetOperation.INTERSECT))
                return
                    "EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required";
        }
        else
        {
            if (UserSettings.StrictValidationForCohortBuilderContainers)
            {
                //if it's not a root, then there should be at least 2
                if (enabledChildren.Length == 0)
                    return
                        "SET containers cannot be empty. Either Add a Catalogue or Disable/Delete this container if not required";


                if (enabledChildren.Length == 1)
                    return
                        "SET containers have no effect if there is only one child within. Either Add a Catalogue or Disable/Delete this container if not required";
            }
        }

        return null;
    }
}