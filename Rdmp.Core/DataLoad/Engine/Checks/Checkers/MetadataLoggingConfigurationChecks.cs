// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.ReusableLibraryCode.Checks;
using LogManager = Rdmp.Core.Logging.LogManager;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

internal class MetadataLoggingConfigurationChecks : ICheckable
{
    private readonly ILoadMetadata _loadMetadata;


    public MetadataLoggingConfigurationChecks(ILoadMetadata loadMetadata)
    {
        _loadMetadata = loadMetadata;
    }


    public void Check(ICheckNotifier notifier)
    {
        var catalogues = _loadMetadata.GetAllCatalogues().ToArray();

        //if there are no logging tasks defined on any Catalogues
        if (catalogues.Any() && catalogues.All(c => string.IsNullOrWhiteSpace(c.LoggingDataTask)))
        {
            string proposedName;

            bool fix;

            if (catalogues.Length == 1)
            {
                proposedName = $"Loading '{catalogues[0]}'";
                fix = notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Catalogue {catalogues[0]} does not have a logging task specified",
                        CheckResult.Fail, null, $"Create a new Logging Task called '{proposedName}'?"));
            }
            else
            {
                proposedName = _loadMetadata.Name;

                fix =
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"Catalogues {string.Join(",", catalogues.Select(c => c.Name))} do not have a logging task specified",
                            CheckResult.Fail, null, $"Create a new Logging Task called '{proposedName}'?"));
            }

            if (fix)
                CreateNewLoggingTaskFor(notifier, catalogues, proposedName);
            else
                return;
        }

        #region Fix missing LoggingDataTask

        var missingTasks = catalogues.Where(c => string.IsNullOrWhiteSpace(c.LoggingDataTask)).ToArray();
        var potentialTasks = catalogues.Except(missingTasks).Select(c => c.LoggingDataTask).Distinct().ToArray();

        //If any Catalogues are missing tasks
        if (missingTasks.Any())
            //but there is consensus for those that are not missing tasks
            if (potentialTasks.Length == 1)
            {
                var fix = notifier.OnCheckPerformed(new CheckEventArgs("Some catalogues have NULL LoggingDataTasks",
                    CheckResult.Fail, null, $"Set task to {potentialTasks.Single()}"));

                if (fix)
                    foreach (var cata in missingTasks)
                    {
                        cata.LoggingDataTask = potentialTasks.Single();
                        cata.SaveToDatabase();
                    }
            }

        #endregion

        #region Fix missing LiveLoggingServer_ID

        var missingServer = catalogues.Where(c => c.LiveLoggingServer_ID == null).ToArray();
        var potentialServer = catalogues.Except(missingServer).Select(c => c.LiveLoggingServer_ID).Distinct().ToArray();

        if (missingServer.Any())
        {
            if (potentialServer.Length == 1)
            {
                var fix = notifier.OnCheckPerformed(new CheckEventArgs("Some catalogues have NULL LiveLoggingServer_ID",
                    CheckResult.Fail, null, $"Set LiveLoggingServer_ID to {potentialServer.Single()}"));

                if (fix)
                    foreach (var cata in missingServer)
                    {
                        cata.LiveLoggingServer_ID = potentialServer.Single();
                        cata.SaveToDatabase();
                    }
            }
            else
            {
                var defaults = _loadMetadata.CatalogueRepository;
                var defaultLoggingServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

                if (defaultLoggingServer != null)
                {
                    var fix = notifier.OnCheckPerformed(new CheckEventArgs(
                        "Some catalogues have NULL LiveLoggingServer_ID", CheckResult.Fail, null,
                        $"Set LiveLoggingServer_ID to '{defaultLoggingServer}' (the default)"));

                    if (fix)
                        foreach (var cata in missingServer)
                        {
                            cata.LiveLoggingServer_ID = defaultLoggingServer.ID;
                            cata.SaveToDatabase();
                        }
                }
            }
        }

        #endregion

        string distinctLoggingTask = null;
        try
        {
            distinctLoggingTask = _loadMetadata.GetDistinctLoggingTask();
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"All Catalogues agreed on a single Logging Task:{distinctLoggingTask}", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Catalogues could not agreed on a single Logging Task",
                CheckResult.Fail, e));
        }


        try
        {
            var settings = _loadMetadata.GetDistinctLoggingDatabase();
            settings.TestConnection();
            notifier.OnCheckPerformed(new CheckEventArgs("Connected to logging architecture successfully",
                CheckResult.Success));


            if (distinctLoggingTask != null)
            {
                var lm = new LogManager(settings);
                var dataTasks = lm.ListDataTasks();

                if (dataTasks.Contains(distinctLoggingTask))
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Found Logging Task {distinctLoggingTask} in Logging database", CheckResult.Success));
                }
                else
                {
                    var fix = notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Could not find Logging Task {distinctLoggingTask} in Logging database", CheckResult.Fail,
                        null,
                        $"Create Logging Task '{distinctLoggingTask}'"));
                    if (fix)
                        lm.CreateNewLoggingTaskIfNotExists(distinctLoggingTask);
                }
            }
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Could reach default logging server", CheckResult.Fail, e));
        }
    }

    private void CreateNewLoggingTaskFor(ICheckNotifier notifier, ICatalogue[] catalogues, string proposedName)
    {
        var catarepo = _loadMetadata.CatalogueRepository;

        var serverIds = catalogues.Select(c => c.LiveLoggingServer_ID).Where(i => i.HasValue).Distinct().ToArray();

        IExternalDatabaseServer loggingServer;

        if (serverIds.Any())
        {
            //a server is specified
            if (serverIds.Length != 1)
                throw new Exception("Catalogues do not agree on a single logging server");

            //we checked for HasValue above see the WHERE in the linq
            loggingServer = catarepo.GetObjectByID<ExternalDatabaseServer>(serverIds[0].Value);
        }
        else
        {
            loggingServer = catarepo.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer == null)
                throw new Exception("There is no default logging server!");
        }

        var logManager = new LogManager(loggingServer);
        logManager.CreateNewLoggingTaskIfNotExists(proposedName);
        notifier.OnCheckPerformed(new CheckEventArgs($"Created Logging Task '{proposedName}'", CheckResult.Success));

        foreach (var catalogue in catalogues.Cast<Catalogue>())
        {
            catalogue.LiveLoggingServer_ID = loggingServer.ID;
            catalogue.LoggingDataTask = proposedName;
            catalogue.SaveToDatabase();
        }
    }
}