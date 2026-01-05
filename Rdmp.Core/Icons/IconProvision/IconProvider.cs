// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Icons.IconProvision.IconProviders;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.Core.Providers.Nodes.UsedByNodes;
using Rdmp.Core.Providers.Nodes.UsedByProject;
using Rdmp.Core.Reports;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.Icons.IconProvision;

/// <summary>
/// Provides all icons for RDMP
/// </summary>
public class IconProvider : ICoreIconProvider
{
    public Image<Rgba32> ImageUnknown => Image.Load<Rgba32>(CatalogueIcons.NoIconAvailable);

    public IconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
    }

    public virtual Image<Rgba32> GetImage(object concept, OverlayKind kind = OverlayKind.None)
    {
        if (concept is RDMPConcept rc) return RDMPConceptIconProvider.GetIcon(rc, kind);
        if (concept is Catalogue) return Image.Load<Rgba32>(CatalogueIcons.Catalogue);
        if (concept is CatalogueItem) return Image.Load<Rgba32>(CatalogueIcons.CatalogueItem);
        if (concept is CatalogueItemsNode) return CatalogueItemsNodeIconProvider.GetIcon(concept, kind);
        if (concept is ColumnInfo)
        {
            return Image.Load<Rgba32>(CatalogueIcons.ColumnInfo);
        }
        if (concept is ExtractionInformation) return Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation);
        if (concept is FolderNode<Catalogue>) return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        if (concept is AggregatesNode)
        {
            return Image.Load<Rgba32>(CatalogueIcons.AggregatesNode);
        }
        if (concept is AggregateConfiguration ac)
        {
            if (ac.IsJoinablePatientIndexTable()) return ImageUnknown;
            if (ac.IsCohortIdentificationAggregate) return Image.Load<Rgba32>(CatalogueIcons.CohortAggregate); ;
            return Image.Load<Rgba32>(CatalogueIcons.AggregateGraph);
        }
        if (concept is CohortAggregateContainer cac)
        {
            if (cac.Operation == SetOperation.UNION) return Image.Load<Rgba32>(CatalogueIcons.UNIONCohortAggregate);
            if (cac.Operation == SetOperation.INTERSECT) return Image.Load<Rgba32>(CatalogueIcons.INTERSECTCohortAggregate);
            //if (cac.Operation == SetOperation.EXCEPT) return Image.Load<Rgba32>(CatalogueIcons.EXCEPTCohortAggregate);
        }
        if (concept is AggregateDimension) return Image.Load<Rgba32>(CatalogueIcons.AggregateDimension);
        if (concept is AggregateContinuousDateAxis) return Image.Load<Rgba32>(CatalogueIcons.AggregateContinuousDateAxis);
        if (concept is AllGovernanceNode) return Image.Load<Rgba32>(CatalogueIcons.AllGovernanceNode);
        if (concept is GovernancePeriod) return Image.Load<Rgba32>(CatalogueIcons.GovernancePeriod);
        if (concept is GovernanceDocument) return Image.Load<Rgba32>(CatalogueIcons.GovernanceDocument);
        if (concept is CohortIdentificationConfiguration) return Image.Load<Rgba32>(CatalogueIcons.CohortIdentificationConfiguration);
        if (concept is FolderNode<CohortIdentificationConfiguration>) return Image.Load<Rgba32>(CatalogueIcons.AllFreeCohortIdentificationConfigurationsNode);
        if (concept is AllOrphanAggregateConfigurationsNode) return Image.Load<Rgba32>(CatalogueIcons.AllOrphanAggregateConfigurationsNode);
        if (concept is AllTemplateAggregateConfigurationsNode) return Image.Load<Rgba32>(CatalogueIcons.AllTemplateAggregateConfigurationsNode);
        if (concept is AllTemplateCohortIdentificationConfigurationsNode) return Image.Load<Rgba32>(CatalogueIcons.AllTemplateCohortIdentificationConfigurationsNode);

        if (concept is JoinableCollectionNode) return Image.Load<Rgba32>(CatalogueIcons.JoinableCollectionNode);
        if (concept is AggregateFilter) return Image.Load<Rgba32>(CatalogueIcons.Filter);
        if (concept is AggregateFilterContainer) return AggregateFilterContainerIconProvider.GetIcon(concept, kind);
        if (concept is ExtractableCohort) return Image.Load<Rgba32>(CatalogueIcons.ExtractableCohort);
        if (concept is ExternalCohortTable) return Image.Load<Rgba32>(CatalogueIcons.ExternalCohortTable);
        if (concept is AllCohortsNode) return Image.Load<Rgba32>(CatalogueIcons.AllCohortsNode);

        if (concept is FolderNode<Project>) return Image.Load<Rgba32>(CatalogueIcons.ProjectsNode);
        if (concept is Project) return Image.Load<Rgba32>(CatalogueIcons.Project);
        if (concept is ProjectCohortsNode) return Image.Load<Rgba32>(CatalogueIcons.ProjectCohortsNode);
        if (concept is ProjectCohortIdentificationConfigurationAssociationsNode) return Image.Load<Rgba32>(CatalogueIcons.ProjectCohortIdentificationConfigurationAssociationsNode);
        if (concept is ProjectCataloguesNode) return Image.Load<Rgba32>(CatalogueIcons.ProjectCataloguesNode);
        if (concept is ExtractionConfigurationsNode) return Image.Load<Rgba32>(CatalogueIcons.ExtractionConfigurationsNode);
        if (concept is ExtractionConfiguration) return Image.Load<Rgba32>(CatalogueIcons.ExtractionConfiguration);
        if (concept is ExtractionDirectoryNode) return Image.Load<Rgba32>(CatalogueIcons.ExtractionDirectoryNode);
        if (concept is CommittedCohortIdentificationNode) return Image.Load<Rgba32>(CatalogueIcons.AllProjectCohortIdentificationConfigurationsNode);
        if (concept is AssociatedCohortIdentificationTemplatesNode) return Image.Load<Rgba32>(CatalogueIcons.AllTemplateCohortIdentificationConfigurationsNode);
        if (concept is ProjectCohortIdentificationConfigurationAssociation) return Image.Load<Rgba32>(CatalogueIcons.ProjectCohortIdentificationConfigurationAssociation);
        if (concept is ProjectSavedCohortsNode) return Image.Load<Rgba32>(CatalogueIcons.ProjectSavedCohortsNode);
        if (concept is FrozenExtractionConfigurationsNode) return Image.Load<Rgba32>(CatalogueIcons.FrozenExtractionConfigurationsNode);
        if (concept is FolderNode<LoadMetadata>) return Image.Load<Rgba32>(CatalogueIcons.LoadMetadataFolder);
        if (concept is LoadMetadata) return Image.Load<Rgba32>(CatalogueIcons.LoadMetadata);
        if (concept is AllPermissionWindowsNode) return Image.Load<Rgba32>(CatalogueIcons.AllPermissionWindowsNode);
        //if (concept is LoadMetadataScheduleNode) return Image.Load<Rgba32>(CatalogueIcons.LoadMetadataScheduleNode);
        if (concept is LoadProgress) return Image.Load<Rgba32>(CatalogueIcons.LoadProgress);
        if (concept is LoadMetadataVersionNode) return Image.Load<Rgba32>(CatalogueIcons.LoadMetadataFolder);
        if (concept is AllProcessTasksUsedByLoadMetadataNode) return Image.Load<Rgba32>(CatalogueIcons.AllProcessTasksUsedByLoadMetadataNode);
        if (concept is LoadDirectoryNode) return Image.Load<Rgba32>(CatalogueIcons.LoadDirectoryNode);
        if (concept is LoadStageNode) return LoadStageNodeIconProvider.GetIcon(concept, kind);
        if (concept is ProcessTask) return Image.Load<Rgba32>(CatalogueIcons.ProcessTask);
        if (concept is ProcessTaskArgument) return Image.Load<Rgba32>(CatalogueIcons.ProcessTaskArgument);
        if (concept is ANOTable) return Image.Load<Rgba32>(CatalogueIcons.ANOTable);
        if (concept is AllANOTablesNode) return Image.Load<Rgba32>(CatalogueIcons.AllANOTablesNode);
        if (concept is AllConnectionStringKeywordsNode) return Image.Load<Rgba32>(CatalogueIcons.AllConnectionStringKeywordsNode);
        if (concept is ConnectionStringKeyword) return Image.Load<Rgba32>(CatalogueIcons.ConnectionStringKeyword);
        if (concept is AllDashboardsNode) return Image.Load<Rgba32>(CatalogueIcons.AllDashboardsNode);
        if (concept is DashboardLayout) return Image.Load<Rgba32>(CatalogueIcons.DashboardLayout);
        if (concept is DataAccessCredentials) return Image.Load<Rgba32>(CatalogueIcons.DataAccessCredentials);
        if (concept is AllDataAccessCredentialsNode) return Image.Load<Rgba32>(CatalogueIcons.AllDataAccessCredentialsNode);
        if (concept is AllServersNode) return Image.Load<Rgba32>(CatalogueIcons.AllServersNode);
        if (concept is TableInfoServerNode) return Image.Load<Rgba32>(CatalogueIcons.TableInfoServerNode);
        if (concept is TableInfoDatabaseNode) return Image.Load<Rgba32>(CatalogueIcons.TableInfoDatabaseNode);
        if (concept is TableInfo) return Image.Load<Rgba32>(CatalogueIcons.TableInfo);
        if (concept is DecryptionPrivateKeyNode) return Image.Load<Rgba32>(CatalogueIcons.DecryptionPrivateKeyNode);
        if (concept is AllExternalServersNode) return Image.Load<Rgba32>(CatalogueIcons.AllExternalServersNode);
        if (concept is ExternalDatabaseServer) return Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer);
        if (concept is AllPipelinesNode) return Image.Load<Rgba32>(CatalogueIcons.AllPipelinesNode);
        if (concept is Pipeline) return Image.Load<Rgba32>(CatalogueIcons.Pipeline);
        if (concept is PipelineComponent) return PipelineComponentIconProvider.GetIcon(concept, kind);
        if (concept is PipelineComponentArgument) return Image.Load<Rgba32>(CatalogueIcons.PipelineComponentArgument);
        if (concept is PipelineCompatibleWithUseCaseNode) return Image.Load<Rgba32>(CatalogueIcons.Pipeline);
        if (concept is OtherPipelinesNode) return Image.Load<Rgba32>(CatalogueIcons.AllPipelinesNode);
        if (concept is StandardPipelineUseCaseNode) return Image.Load<Rgba32>(CatalogueIcons.AllPipelinesNode);
        if (concept is AllPluginsNode) return Image.Load<Rgba32>(CatalogueIcons.AllPluginsNode);
        if (concept is Plugin) return Image.Load<Rgba32>(CatalogueIcons.Plugin);
        if (concept is AllStandardRegexesNode) return Image.Load<Rgba32>(CatalogueIcons.AllStandardRegexesNode);
        if (concept is StandardRegex) return Image.Load<Rgba32>(CatalogueIcons.StandardRegex);
        if (concept is AllDatasetsNode) return Image.Load<Rgba32>(CatalogueIcons.AllDatasetsNode);
        if (concept is Curation.Data.Dataset) return Image.Load<Rgba32>(CatalogueIcons.Dataset);

        if (concept is ArbitraryFolderNode) return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        if (concept is ExtractionArbitraryFolderNode) return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        if (concept is SelectedDataSets) return Image.Load<Rgba32>(CatalogueIcons.Catalogue);
        if (concept is LinkedColumnInfoNode) return Image.Load<Rgba32>(CatalogueIcons.ColumnInfo);
        if (concept is AllCataloguesUsedByLoadMetadataNode) return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        if (concept is CatalogueUsedByLoadMetadataNode) return Image.Load<Rgba32>(CatalogueIcons.Catalogue);
        if (concept is PermissionWindow) return Image.Load<Rgba32>(CatalogueIcons.PermissionWindow);
        if (concept is CheckResult) return CheckResultIconProvider.GetIcon(concept);
        if (concept is ExtractionCategory ec) return ExtractionCategoryIconProvider.GetIcon(concept);
        if (concept is ExtractCommandState) return ExtractCommandStateIconProvider.GetIcon(concept);
        if (concept is LinkedCohortNode) return Image.Load<Rgba32>(CatalogueIcons.CohortIdentificationConfiguration);
        if (concept is CohortSourceUsedByProjectNode) return Image.Load<Rgba32>(CatalogueIcons.AllCohortsNode);//todo is this right?
        if (concept is DatabaseType) return IconProviders.DatabaseTypeIconProvider.GetIcon(concept);
        if (concept is LoadMetadataScheduleNode) return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        if (concept is LoadBubble) return Image.Load<Rgba32>(CatalogueIcons.TableInfoDatabaseNode);
        if (concept is LoadStage) return LoadStageNodeIconProvider.GetIcon(concept);
        if (concept is ExtractableColumn) return Image.Load<Rgba32>(CatalogueIcons.ColumnInfo);
        if (concept is CohortAggregateContainer ) return CohortAggregateContainerIconProvider.GetIcon(concept);
        var x = concept.GetType();
        return ImageUnknown;
    }

    //Used for testing
    public bool HasIcon(object o)
    {
        return GetImage(o) != ImageUnknown;
    }
}