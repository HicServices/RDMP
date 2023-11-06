// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    internal class Args : RDMPCommandLineOptions { }
    internal class ExecuteCommandExportPlatformDatabasesToYaml : BasicCommandExecution, IAtomicCommand
    {
        private readonly TableRepository _catalogueRepository;
        private readonly TableRepository _dataExportRepository;
        private readonly string _outputFile;

        public ExecuteCommandExportPlatformDatabasesToYaml(IBasicActivateItems activator, [DemandsInitialization("Where the yaml file should be created")] string outputfile)
        {

            _catalogueRepository = activator.RepositoryLocator.CatalogueRepository as TableRepository;
            _dataExportRepository = activator.RepositoryLocator.DataExportRepository as TableRepository;
            _outputFile = outputfile;

        }

        public override void Execute()
        {
            base.Execute();
            Args args = new Args()
            {
                Dir = _outputFile,
            };
            var yamlRespositoryLocator = args.GetRepositoryLocator();
            _catalogueRepository.GetAllObjects<Catalogue>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<CohortAggregateContainer>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<GovernanceDocument>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<GovernancePeriod>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<StandardRegex>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AnyTableSqlParameter>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ANOTable>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateConfiguration>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateContinuousDateAxis>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateDimension>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateFilter>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateFilterContainer>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateFilterParameter>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<CatalogueItem>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ColumnInfo>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<JoinableCohortAggregateConfiguration>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<JoinableCohortAggregateConfigurationUse>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExternalDatabaseServer>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExtractionFilter>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExtractionFilterParameter>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExtractionInformation>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExtractionFilterParameterSet>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<LoadMetadata>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ExtractionFilterParameterSetValue>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<LoadProgress>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<Favourite>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<Pipeline>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<Lookup>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<AggregateTopX>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<PipelineComponent>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<LookupCompositeJoinInfo>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<PipelineComponentArgument>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<PreLoadDiscardedColumn>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ProcessTask>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<DashboardLayout>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ProcessTaskArgument>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<DashboardControl>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<DataAccessCredentials>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<SupportingDocument>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<DashboardObjectUse>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<SupportingSQLTable>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<TableInfo>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<RemoteRDMP>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ObjectImport>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ObjectExport>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<CacheProgress>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<ConnectionStringKeyword>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<WindowLayout>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<PermissionWindow>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<TicketingSystemConfiguration>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));
            _catalogueRepository.GetAllObjects<CacheFetchFailure>().ToList().ForEach(item => yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item));


            _dataExportRepository.GetAllObjects<SupplementalExtractionResults>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<CumulativeExtractionResults>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<DeployedExtractionFilter>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<DeployedExtractionFilterParameter>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExternalCohortTable>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExtractableCohort>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExtractableColumn>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExtractableDataSet>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExtractionConfiguration>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<FilterContainer>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<GlobalExtractionFilterParameter>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<Project>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<SelectedDataSets>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ExtractableDataSetPackage>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
            _dataExportRepository.GetAllObjects<SelectedDataSetsForcedJoin>().ToList().ForEach(item => yamlRespositoryLocator.DataExportRepository.SaveToDatabase(item));
        }
    }
}
