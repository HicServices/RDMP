// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
/// DLE component responsible for creating the RAW (first database in the RAW=>STAGING=>LIVE model of DLE loading) database (if required).  Also runs
/// all LoadStage.Mounting and components.
/// </summary>
public class PopulateRAW : CompositeDataLoadComponent
{
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    public PopulateRAW(List<IRuntimeTask> collection, HICDatabaseConfiguration databaseConfiguration) : base(
        collection.Cast<IDataLoadComponent>().ToList())
    {
        _databaseConfiguration = databaseConfiguration;
        Description = "Populate RAW";
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (Skip(job))
            return ExitCodeType.Error;

        // We may or may not need to create the raw database, depending on how we are getting the data
        CreateRawDatabaseIfRequired(job);

        var toReturn = base.Run(job, cancellationToken);

        if (toReturn == ExitCodeType.Success)
            // Verify that we have put something into the database
            VerifyExistenceOfRawData(job);

        return toReturn;
    }

    private bool MustCreateRawDatabase()
    {
        var attachingProcesses = Components.OfType<AttacherRuntimeTask>().ToArray();

        switch (attachingProcesses.Length)
        {
            //we do not have any attaching processes ... magically the data must appear somehow in our RAW database so better create it -- maybe an executable is going to populate it or something
            case 0:
                return true;
            case > 1:
                {
                    // if there are multiple attachers, ensure that they all agree on whether or not they require external database creation
                    var attachers = attachingProcesses.Select(runtime => runtime.Attacher).ToList();
                    var numAttachersRequiringDbCreation =
                        attachers.Count(attacher => attacher.RequestsExternalDatabaseCreation);

                    if (numAttachersRequiringDbCreation > 0 && numAttachersRequiringDbCreation < attachingProcesses.Length)
                        throw new Exception(
                            $"If there are multiple attachers then they should all agree on whether they require database creation or not: {attachers.Aggregate("", (s, attacher) => $"{s} {attacher.GetType().Name}:{attacher.RequestsExternalDatabaseCreation}")}");
                    break;
                }
        }

        return attachingProcesses[0].Attacher.RequestsExternalDatabaseCreation;
    }

    private void CreateRawDatabaseIfRequired(IDataLoadJob job)
    {
        // Ask the runtime process host if we need to create the RAW database
        if (!MustCreateRawDatabase()) return;

        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                "Determined that we must create the RAW database tables..."));

        var cloner = new DatabaseCloner(_databaseConfiguration);

        if (!job.PersistentRaw) cloner.CreateDatabaseForStage(LoadBubble.Raw);

        job.CreateTablesInStage(cloner, LoadBubble.Raw);
    }

    // Check that either Raw database exists and is populated, or that 'forLoading' is not empty
    private void VerifyExistenceOfRawData(IDataLoadJob job)
    {
        var raw = _databaseConfiguration.DeployInfo[LoadBubble.Raw];

        if (!raw.Exists())
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"The Mounting stage has not created the {raw.GetRuntimeName()} database."));

        var rawDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Raw];

        if (_databaseConfiguration.ExpectTables(job, LoadBubble.Raw, true).All(t => t.IsEmpty()))
        {
            var message = $"The Mounting stage has not populated the RAW database ({rawDbInfo}) with any data";
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, message));
            throw new Exception(message);
        }
    }
}