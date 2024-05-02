// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     Prepares the Environment and the Source Database for the ReleaseEngine.
/// </summary>
public class MsSqlReleaseSource : FixedReleaseSource<ReleaseAudit>
{
    private readonly ICatalogueRepository _catalogueRepository;
    private DiscoveredDatabase _database;
    private DirectoryInfo _dataPathMap;

    public MsSqlReleaseSource(ICatalogueRepository catalogueRepository)
    {
        _catalogueRepository = catalogueRepository;
    }

    protected override ReleaseAudit GetChunkImpl(IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var sourceFolder = GetSourceFolder();
        Debug.Assert(sourceFolder != null, "sourceFolder != null");
        var dbOutputFolder = sourceFolder.CreateSubdirectory(ExtractionDirectory.MASTER_DATA_FOLDER_NAME);

        var releaseAudit = new ReleaseAudit
        {
            SourceGlobalFolder = PrepareSourceGlobalFolder()
        };

        if (_database != null)
        {
            _database.Detach();
            var databaseName = _database.GetRuntimeName();

            File.Copy(Path.Combine(_dataPathMap.FullName, $"{databaseName}.mdf"), Path.Combine(dbOutputFolder.FullName,
                $"{databaseName}.mdf"));
            File.Copy(Path.Combine(_dataPathMap.FullName, $"{databaseName}_log.ldf"), Path.Combine(
                dbOutputFolder.FullName,
                $"{databaseName}_log.ldf"));
            File.Delete(Path.Combine(_dataPathMap.FullName, $"{databaseName}.mdf"));
            File.Delete(Path.Combine(_dataPathMap.FullName, $"{databaseName}_log.ldf"));
        }

        return releaseAudit;
    }

    private DirectoryInfo GetSourceFolder()
    {
        var extractDir = _releaseData.ConfigurationsForRelease.First().Key.GetProject().ExtractionDirectory;

        return new ExtractionDirectory(extractDir, _releaseData.ConfigurationsForRelease.First().Key)
            .ExtractionDirectoryInfo;
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        firstTime = true;
    }

    public override void Abort(IDataLoadEventListener listener)
    {
        firstTime = true;
    }

    protected override void RunSpecificChecks(ICheckNotifier notifier, bool isRunTime)
    {
        if (!_releaseData.ReleaseGlobals || _releaseData.ReleaseState == ReleaseState.DoingPatch)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "You cannot untick globals or release a subset of datasets when releasing from a DB",
                CheckResult.Fail));

        var foundConnection = string.Empty;
        var tables = new List<string>();
        foreach (var cumulativeResult in _releaseData.ConfigurationsForRelease.SelectMany(x =>
                     x.Key.CumulativeExtractionResults))
        {
            if (cumulativeResult.DestinationDescription.Split('|').Length != 3)
                throw new Exception("The extraction did not generate a description that can be parsed. " +
                                    "Have you extracted to a mix of CSVs and DB tables?");

            var candidate =
                $"{cumulativeResult.DestinationDescription.Split('|')[0]}|{cumulativeResult.DestinationDescription.Split('|')[1]}";

            tables.Add(cumulativeResult.DestinationDescription.Split('|')[2]);

            if (string.IsNullOrEmpty(foundConnection)) // the first time we use the candidate as our connection...
                foundConnection = candidate;

            if (foundConnection != candidate) // ...then we check that all other candidates point to the same DB
                throw new Exception(
                    "You are trying to extract from multiple servers or databases. This is not allowed! " +
                    "Please re-run the extracts against the same database.");

            foreach (var supplementalResult in cumulativeResult.SupplementalExtractionResults
                         .Where(x => x.IsReferenceTo(typeof(SupportingSQLTable)) ||
                                     x.IsReferenceTo(typeof(TableInfo))))
            {
                if (supplementalResult.DestinationDescription.Split('|').Length != 3)
                    throw new Exception("The extraction did not generate a description that can be parsed. " +
                                        "Have you extracted to a mix of CSVs and DB tables?");

                candidate =
                    $"{supplementalResult.DestinationDescription.Split('|')[0]}|{supplementalResult.DestinationDescription.Split('|')[1]}";

                tables.Add(supplementalResult.DestinationDescription.Split('|')[2]);

                if (foundConnection != candidate) // ...then we check that all other candidates point to the same DB
                    throw new Exception(
                        "You are trying to extract from multiple servers or databases. This is not allowed! " +
                        "Please re-run the extracts against the same database.");
            }
        }

        foreach (var globalResult in _releaseData.ConfigurationsForRelease
                     .SelectMany(x => x.Key.SupplementalExtractionResults)
                     .Where(x => x.IsReferenceTo(typeof(SupportingSQLTable)) ||
                                 x.IsReferenceTo(typeof(TableInfo))))
        {
            if (globalResult.DestinationDescription.Split('|').Length != 3)
                throw new Exception("The extraction did not generate a description that can be parsed. " +
                                    "Have you extracted the Globals to CSVs rather than DB tables?");

            var candidate =
                $"{globalResult.DestinationDescription.Split('|')[0]}|{globalResult.DestinationDescription.Split('|')[1]}";

            tables.Add(globalResult.DestinationDescription.Split('|')[2]);

            if (string.IsNullOrEmpty(foundConnection)) // the first time we use the candidate as our connection...
                foundConnection = candidate;

            if (foundConnection != candidate) // ...then we check that all other candidates point to the same DB
                throw new Exception(
                    "You are trying to extract from multiple servers or databases. This is not allowed! " +
                    "Please re-run the extracts against the same database.");
        }

        var externalServerId = int.Parse(foundConnection.Split('|')[0]);
        var dbName = foundConnection.Split('|')[1];

        var externalServer = _catalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
        if (!string.IsNullOrWhiteSpace(externalServer.MappedDataPath))
            _dataPathMap = new DirectoryInfo(externalServer.MappedDataPath);
        else
            throw new Exception(
                $"The selected Server ({externalServer.Name}) must have a Data Path in order to be used as an extraction destination.");

        var server = DataAccessPortal.ExpectServer(externalServer, DataAccessContext.DataExport, false);
        _database = server.ExpectDatabase(dbName);

        if (!_database.Exists()) throw new Exception($"Database {_database} does not exist!");

        foreach (var table in tables)
        {
            var foundTable = _database.ExpectTable(table);
            if (!foundTable.Exists()) throw new Exception($"Table {table} does not exist!");
        }

        var spuriousTables = _database.DiscoverTables(false).Where(t => !tables.Contains(t.GetRuntimeName())).ToList();
        if (spuriousTables.Any() && !notifier.OnCheckPerformed(new CheckEventArgs(
                $"Spurious table(s): {string.Join(",", spuriousTables)} found in the DB.These WILL BE released, you may want to check them before proceeding.",
                CheckResult.Warning,
                null,
                "Are you sure you want to continue the release process?")))
            if (!isRunTime)
                throw new Exception("Release aborted by user.");

        var sourceFolder = GetSourceFolder();
        var dbOutputFolder = sourceFolder.CreateSubdirectory(ExtractionDirectory.MASTER_DATA_FOLDER_NAME);

        var databaseName = _database.GetRuntimeName();

        if (File.Exists(Path.Combine(dbOutputFolder.FullName, $"{databaseName}.mdf")) ||
            File.Exists(Path.Combine(dbOutputFolder.FullName, $"{databaseName}_log.ldf")))
        {
            if (notifier.OnCheckPerformed(new CheckEventArgs(
                    $"It seems that database {databaseName} was already detached previously into {dbOutputFolder.FullName} but not released or cleaned from the extraction folder",
                    CheckResult.Warning,
                    null,
                    "Do you want to delete it? You should check the contents first. Clicking 'No' will abort the Release.")))
            {
                File.Delete(Path.Combine(dbOutputFolder.FullName, $"{databaseName}.mdf"));
                File.Delete(Path.Combine(dbOutputFolder.FullName, $"{databaseName}_log.ldf"));
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Cleaned non-empty existing db output folder folder: {dbOutputFolder.FullName}",
                    CheckResult.Success));
            }
            else
            {
                if (!isRunTime)
                    throw new Exception("Release aborted by user.");
            }
        }
    }

    protected override DirectoryInfo PrepareSourceGlobalFolder()
    {
        return _releaseData.ReleaseGlobals ? base.PrepareSourceGlobalFolder() : null;
    }
}