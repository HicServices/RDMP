// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Standard;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution;

/// <summary>
///     This is factored out more for documentation's sake. It is a description of the HIC data load pipeline, in factory
///     form!
/// </summary>
public class HICDataLoadFactory
{
    private readonly HICDatabaseConfiguration _databaseConfiguration;
    private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
    private readonly ICatalogueRepository _repository;
    private readonly ILogManager _logManager;
    private readonly IList<ICatalogue> _cataloguesToLoad;

    public ILoadMetadata LoadMetadata { get; }

    public HICDataLoadFactory(ILoadMetadata loadMetadata, HICDatabaseConfiguration databaseConfiguration,
        HICLoadConfigurationFlags loadConfigurationFlags, ICatalogueRepository repository, ILogManager logManager)
    {
        _databaseConfiguration = databaseConfiguration;
        _loadConfigurationFlags = loadConfigurationFlags;
        _repository = repository;
        _logManager = logManager;
        LoadMetadata = loadMetadata;

        // If we are not supplied any catalogues to load, it is expected that we will load all catalogues associated with the provided ILoadMetadata
        _cataloguesToLoad = LoadMetadata.GetAllCatalogues().ToList();
        if (!_cataloguesToLoad.Any())
            throw new InvalidOperationException(
                $"LoadMetadata {LoadMetadata.ID} is not related to any Catalogues, there is nothing to load");
    }

    public IDataLoadExecution Create(IDataLoadEventListener postLoadEventListener)
    {
        var loadArgsDictionary = new LoadArgsDictionary(LoadMetadata, _databaseConfiguration.DeployInfo);

        //warn user about disabled tasks
        var processTasks = LoadMetadata.ProcessTasks.ToList();
        foreach (var task in processTasks
                     .Where(p => p.IsDisabled))
            postLoadEventListener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning, $"Found disabled ProcessTask{task}"));

        //Get all the runtime tasks which are not disabled
        var factory = new RuntimeTaskPackager(processTasks.Where(p => !p.IsDisabled), loadArgsDictionary.LoadArgs,
            _cataloguesToLoad, _repository);

        var getFiles = new LoadFiles(factory.GetRuntimeTasksForStage(LoadStage.GetFiles));

        var mounting = new PopulateRAW(factory.GetRuntimeTasksForStage(LoadStage.Mounting), _databaseConfiguration);

        var adjustRaw = factory.CreateCompositeDataLoadComponentFor(LoadStage.AdjustRaw, "Adjust RAW");

        var migrateToStaging = new MigrateRAWToStaging(_databaseConfiguration, _loadConfigurationFlags);

        var adjustStaging = factory.CreateCompositeDataLoadComponentFor(LoadStage.AdjustStaging, "Adjust Staging");

        var migrateStagingToLive = new MigrateStagingToLive(_databaseConfiguration, _loadConfigurationFlags);

        var postLoad = factory.CreateCompositeDataLoadComponentFor(LoadStage.PostLoad, "Post Load");

        var archiveFiles = new ArchiveFiles(_loadConfigurationFlags);

        var loadStagingDatabase = new CompositeDataLoadComponent(new List<IDataLoadComponent>
        {
            mounting,
            adjustRaw,
            migrateToStaging
        });

        var adjustStagingAndMigrateToLive = new CompositeDataLoadComponent(new List<IDataLoadComponent>
        {
            loadStagingDatabase,
            adjustStaging,
            migrateStagingToLive,
            postLoad
        });

        var components = new List<IDataLoadComponent>
        {
            getFiles,
            adjustStagingAndMigrateToLive,
            archiveFiles
        };

        return new SingleJobExecution(components);
    }
}