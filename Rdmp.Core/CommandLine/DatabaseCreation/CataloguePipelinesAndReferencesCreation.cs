// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.DatabaseCreation;

/// <summary>
/// Creates default pipelines required for basic functionality in RDMP.  These are templates that work but can be expanded upon / modified by the user.  For
/// example the user might want to add a ColumnForbidder to the default export pipeline to prevent sensitive fields being extracted etc.
/// 
/// </summary>
public class CataloguePipelinesAndReferencesCreation
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
    private readonly SqlConnectionStringBuilder _logging;
    private readonly SqlConnectionStringBuilder _dqe;
    private ExternalDatabaseServer _edsLogging;


    public CataloguePipelinesAndReferencesCreation(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        SqlConnectionStringBuilder logging, SqlConnectionStringBuilder dqe)
    {
        _repositoryLocator = repositoryLocator;
        _logging = logging;
        _dqe = dqe;
    }

    private void DoStartup()
    {
        var startup = new Startup.Startup(_repositoryLocator);
        startup.DoStartup(IgnoreAllErrorsCheckNotifier.Instance);
    }

    private void CreateServers(PlatformDatabaseCreationOptions options)
    {
        var defaults = _repositoryLocator.CatalogueRepository;
        if (options.CreateLoggingServer)
        {
            _edsLogging = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "Logging", new LoggingDatabasePatcher())
            {
                Server = _logging?.DataSource ?? throw new InvalidOperationException("Null logging database provided"),
                Database = _logging.InitialCatalog
            };

            if (_logging.UserID != null)
            {
                _edsLogging.Username = _logging.UserID;
                _edsLogging.Password = _logging.Password;
            }

            _edsLogging.SaveToDatabase();
            defaults.SetDefault(PermissableDefaults.LiveLoggingServer_ID, _edsLogging);
            Console.WriteLine("Successfully configured default logging server");
        }

        var edsDQE = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "DQE", new DataQualityEnginePatcher())
        {
            Server = _dqe.DataSource,
            Database = _dqe.InitialCatalog
        };

        if (_dqe.UserID != null)
        {
            edsDQE.Username = _dqe.UserID;
            edsDQE.Password = _dqe.Password;
        }

        edsDQE.SaveToDatabase();
        defaults.SetDefault(PermissableDefaults.DQE, edsDQE);
        Console.WriteLine("Successfully configured default dqe server");

        var edsRAW = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "RAW Server", null)
        {
            Server = _dqe.DataSource
        };

        //We are expecting a single username/password for everything here, so just use the dqe one
        var hasLoggingDB = _logging != null && _logging.UserID != null;
        if (_dqe.UserID != null && hasLoggingDB)
        {
            if (_logging.UserID != _dqe.UserID || _logging.Password != _dqe.Password)
                throw new Exception(
                    "DQE uses sql authentication but the credentials are not the same as the logging db.  Could not pick a single set of credentials to use for the RAW server entry");

            edsRAW.Username = _dqe.UserID;
            edsRAW.Password = _dqe.Password;
        }

        edsRAW.SaveToDatabase();
        defaults.SetDefault(PermissableDefaults.RAWDataLoadServer, edsRAW);
        Console.WriteLine("Successfully configured RAW server");
    }


    public void CreatePipelines(PlatformDatabaseCreationOptions options)
    {
        var bulkInsertCsvPipe =
            CreatePipeline("BULK INSERT: CSV Import File (manual column-type editing)",
                typeof(DelimitedFlatFileDataFlowSource), typeof(DataTableUploadDestination));
        var bulkInsertCsvPipewithAdjuster =
            CreatePipeline("BULK INSERT: CSV Import File (automated column-type detection)",
                typeof(DelimitedFlatFileDataFlowSource), typeof(DataTableUploadDestination));
        CreatePipeline("BULK INSERT: Excel File", typeof(ExcelDataFlowSource), typeof(DataTableUploadDestination));

        SetComponentProperties(bulkInsertCsvPipe.Source, "Separator", ",");
        SetComponentProperties(bulkInsertCsvPipe.Source, "StronglyTypeInput", false);
        SetComponentProperties(bulkInsertCsvPipewithAdjuster.Source, "Separator", ",");
        SetComponentProperties(bulkInsertCsvPipewithAdjuster.Source, "StronglyTypeInput", false);

        if (options.CreateLoggingServer)
        {
            SetComponentProperties(bulkInsertCsvPipe.Destination, "LoggingServer", _edsLogging);
            SetComponentProperties(bulkInsertCsvPipewithAdjuster.Destination, "LoggingServer", _edsLogging);
        }
        var createCohortFromCSV = CreatePipeline("CREATE COHORT:From CSV File", typeof(DelimitedFlatFileDataFlowSource),
            typeof(BasicCohortDestination));
        SetComponentProperties(createCohortFromCSV.Source, "Separator", ",");

        CreatePipeline("CREATE COHORT:By Executing Cohort Identification Configuration",
            typeof(CohortIdentificationConfigurationSource), typeof(BasicCohortDestination));

        CreatePipeline("CREATE COHORT: From Catalogue", typeof(PatientIdentifierColumnSource),
            typeof(BasicCohortDestination));

        CreatePipeline("IMPORT COHORT CUSTOM DATA: From PatientIndexTable", typeof(PatientIndexTableSource), null);

        CreatePipeline("DATA EXPORT:To CSV", typeof(ExecuteDatasetExtractionSource),
            typeof(ExecuteDatasetExtractionFlatFileDestination));

        CreatePipeline("RELEASE PROJECT:To Directory", null, typeof(BasicDataReleaseDestination),
            typeof(ReleaseFolderProvider));

        CreatePipeline("CREATE TABLE:From Aggregate Query", null, typeof(DataTableUploadDestination));
    }

    private static void SetComponentProperties(IPipelineComponent component, string propertyName, object value)
    {
        var d = (PipelineComponentArgument)component.GetAllArguments().Single(a => a.Name.Equals(propertyName));
        d.SetValue(value);
        d.SaveToDatabase();
    }

    private Pipeline CreatePipeline(string nameOfPipe, Type sourceType, Type destinationTypeIfAny,
        params Type[] componentTypes)
    {
        if (componentTypes == null || componentTypes.Length == 0)
            return CreatePipeline(nameOfPipe, sourceType, destinationTypeIfAny);

        var pipeline = CreatePipeline(nameOfPipe, sourceType, destinationTypeIfAny);

        var i = 1;
        foreach (var componentType in componentTypes)
        {
            var component = new PipelineComponent(_repositoryLocator.CatalogueRepository, pipeline, componentType, i++);
            component.CreateArgumentsForClassIfNotExists(componentType);
            component.Pipeline_ID = pipeline.ID;
        }

        return pipeline;
    }

    private Pipeline CreatePipeline(string nameOfPipe, Type sourceType, Type destinationTypeIfAny)
    {
        var pipe = new Pipeline(_repositoryLocator.CatalogueRepository, nameOfPipe);

        if (sourceType != null)
        {
            var source = new PipelineComponent(_repositoryLocator.CatalogueRepository, pipe, sourceType, 0);
            source.CreateArgumentsForClassIfNotExists(sourceType);
            pipe.SourcePipelineComponent_ID = source.ID;
        }

        if (destinationTypeIfAny != null)
        {
            var destination =
                new PipelineComponent(_repositoryLocator.CatalogueRepository, pipe, destinationTypeIfAny, 100);
            destination.CreateArgumentsForClassIfNotExists(destinationTypeIfAny);
            pipe.DestinationPipelineComponent_ID = destination.ID;
        }

        pipe.SaveToDatabase();

        return pipe;
    }

    public void Create(PlatformDatabaseCreationOptions options)
    {
        DoStartup();
        CreateServers(options);
        CreatePipelines(options);
    }
}