// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline.Destinations;

/// <summary>
///     Destination component for Cohort Creation Pipelines, responsible for bulk inserting patient identifiers into the
///     cohort database specified in the
///     ICohortCreationRequest.  This
/// </summary>
public class BasicCohortDestination : IPluginCohortDestination
{
    private string _privateIdentifier;
    private string _releaseIdentifier;

    /// <summary>
    ///     The cohort blueprint we are trying to create.
    /// </summary>
    public ICohortCreationRequest Request { get; private set; }

    private string _fk;

    [DemandsInitialization(
        "Set one of these if you plan to upload lists of patients and want RDMP to automatically allocate an anonymous ReleaseIdentifier",
        TypeOf = typeof(IAllocateReleaseIdentifiers),
        DefaultValue = typeof(ProjectConsistentGuidReleaseIdentifierAllocator))]
    public Type ReleaseIdentifierAllocator { get; set; }

    [DemandsInitialization(
        @"Determines behaviour when you are creating a new version of an existing cohort.  If true then the old (replaced) cohort will be marked IsDeprecated",
        DefaultValue = true)]
    public bool DeprecateOldCohortOnSuccess { get; set; }

    [DemandsInitialization(
        @"Determines behaviour when you are creating a new version of an existing cohort.  If true then any ExtractionConfiguration that are not frozen are moved to the new version of the cohort",
        DefaultValue = false)]
    public bool MigrateUsages { get; set; }

    private IAllocateReleaseIdentifiers _allocator;
    private readonly Dictionary<object, object> _cohortDictionary = new();

    /// <summary>
    ///     Extracts private identifiers from table <paramref name="toProcess" /> and allocates release identifiers.  Cohort is
    ///     only finalised and comitted into the database
    ///     in the <see cref="Dispose" /> method (to prevent incomplete cohorts existing in the database).
    ///     <para>
    ///         Method can be called multiple times in the lifetime of a pipeline (e.g. if you have very large cohorts and
    ///         the pipeline source is batching).
    ///     </para>
    /// </summary>
    /// <param name="toProcess">A batch of private identifiers</param>
    /// <param name="listener"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        //if user has picked an allocator get an instance
        if (ReleaseIdentifierAllocator != null && _allocator == null)
        {
            _allocator = (IAllocateReleaseIdentifiers)ObjectConstructor.Construct(ReleaseIdentifierAllocator);
            _allocator.Initialize(Request);
        }

        if (!toProcess.Columns.Contains(_privateIdentifier))
            throw new Exception(
                $"Could not find column called {_privateIdentifier} in chunk, columns were:{string.Join(",", toProcess.Columns.OfType<DataColumn>().Select(c => c.ColumnName))}");

        //we don't have a release identifier column
        if (!toProcess.Columns.Contains(_releaseIdentifier))
        {
            foreach (DataRow row in toProcess.Rows)
            {
                //so we have to allocate all of them with the allocator

                //get the private cohort id
                var priv = row[_privateIdentifier];

                //already handled these folks?
                if (_cohortDictionary.ContainsKey(priv) || IsNull(priv))
                    continue;

                //no, allocate them an ID (or null if there is no allocator)
                _cohortDictionary.Add(priv,
                    _allocator == null ? DBNull.Value : _allocator.AllocateReleaseIdentifier(priv));
            }
        }
        else
        {
            var foundUserSpecifiedReleaseIds = false;

            foreach (DataRow row in toProcess.Rows)
            {
                //get the private cohort id
                var priv = row[_privateIdentifier];

                //already handled these folks?
                if (_cohortDictionary.ContainsKey(priv) || IsNull(priv))
                    continue;

                //and the release id specified in the input table
                var release = row[_releaseIdentifier];

                //if it was blank
                if (IsNull(release))
                {
                    if (_allocator != null)
                    {
                        if (foundUserSpecifiedReleaseIds && _allocator != null)
                            throw new Exception(
                                $"Input data table had a column '{_releaseIdentifier}' which contained some values but also null values.  There is a configured ReleaseIdentifierAllocator, we cannot cannot continue since it would result in a mixed release identifier list of some provided by you and some provided by the ReleaseIdentifierAllocator");

                        release = _allocator.AllocateReleaseIdentifier(priv);
                    }
                }
                else
                {
                    foundUserSpecifiedReleaseIds = true;
                }

                //no, allocate them an ID (or null if there is no allocator)
                _cohortDictionary.Add(priv, release);
            }
        }

        return null;
    }


    private static bool IsNull(object o)
    {
        return o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString());
    }

    /// <summary>
    ///     Commits the cohort created into the database (assuming no error occured during pipeline processing - See
    ///     <paramref name="pipelineFailureExceptionIfAny" />).
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="pipelineFailureExceptionIfAny"></param>
    public virtual void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        //it exceptioned
        if (pipelineFailureExceptionIfAny != null)
            return;

        var db = Request.NewCohortDefinition.LocationOfCohort.Discover();

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing upload"));

        using (var connection = db.Server.BeginNewTransactedConnection())
        {
            try
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Started Transaction"));
                Request.PushToServer(connection);

                if (Request.NewCohortDefinition.ID == null)
                    throw new Exception(
                        "We pushed the new cohort from the request object to the server (within transaction) but its ID was not populated");

                var tbl = Request.NewCohortDefinition.LocationOfCohort.DiscoverCohortTable();

                var isIdentifiable = string.Equals(_releaseIdentifier, _privateIdentifier);

                using (var bulkCopy = tbl.BeginBulkInsert(connection.ManagedTransaction))
                {
                    var dt = new DataTable();
                    dt.Columns.Add(_privateIdentifier);

                    // don't add 2 columns if they are the same column!
                    if (!isIdentifiable) dt.Columns.Add(_releaseIdentifier);

                    //add the ID as another column
                    dt.Columns.Add(_fk);

                    foreach (var kvp in _cohortDictionary)
                        if (isIdentifiable)
                            dt.Rows.Add(kvp.Key, Request.NewCohortDefinition.ID);
                        else
                            dt.Rows.Add(kvp.Key, kvp.Value, Request.NewCohortDefinition.ID);


                    bulkCopy.Upload(dt);
                }

                connection.ManagedTransaction.CommitAndCloseConnection();
            }
            catch
            {
                connection.ManagedTransaction.AbandonAndCloseConnection();
                throw;
            }
        }

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Successfully uploaded {_cohortDictionary.Count} records"));

        var id = Request.ImportAsExtractableCohort(DeprecateOldCohortOnSuccess, MigrateUsages);

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Cohort successfully committed to destination and imported as an RDMP ExtractableCohort (ID={id} <- this is the ID of the reference pointer, the cohortDefinitionID of the actual cohort remains as you specified:{Request.NewCohortDefinition.ID})"));
    }

    /// <summary>
    ///     Does nothing
    /// </summary>
    /// <param name="listener"></param>
    public virtual void Abort(IDataLoadEventListener listener)
    {
    }

    /// <summary>
    ///     Initialises <see cref="Request" />
    /// </summary>
    /// <param name="value"></param>
    /// <param name="listener"></param>
    public virtual void PreInitialize(ICohortCreationRequest value, IDataLoadEventListener listener)
    {
        Request = value;

        var target = Request.NewCohortDefinition.LocationOfCohort;

        var syntax = target.GetQuerySyntaxHelper();
        _privateIdentifier = syntax.GetRuntimeName(target.PrivateIdentifierField);
        _releaseIdentifier = syntax.GetRuntimeName(target.ReleaseIdentifierField);

        _fk = syntax.GetRuntimeName(Request.NewCohortDefinition.LocationOfCohort.DefinitionTableForeignKeyField);

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"CohortCreationRequest spotted, we will look for columns {_privateIdentifier} and {_releaseIdentifier} (both of which must be in the pipeline before we will allow the cohort to be submitted)"));
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"id column in table {Request.NewCohortDefinition.LocationOfCohort.TableName} is {Request.NewCohortDefinition.LocationOfCohort.DefinitionTableForeignKeyField}"));
    }

    /// <summary>
    ///     Checks <see cref="ReleaseIdentifierAllocator" /> has been set up and that a properly populated
    ///     <see cref="Request" /> has been set.
    /// </summary>
    /// <param name="notifier"></param>
    public virtual void Check(ICheckNotifier notifier)
    {
        if (Request.IsDesignTime)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs("Cannot check because CohortCreationRequest is CohortCreationRequest.Empty",
                    CheckResult.Warning));
            return;
        }

        if (ReleaseIdentifierAllocator == null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "No ReleaseIdentifierAllocator has been set, this means that Release Identifiers must be provided in the cohort uploaded or populated after committing manually",
                CheckResult.Warning));

        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Cohort identifier columns are '{_privateIdentifier}' (private) and '{_releaseIdentifier}' (release)",
            CheckResult.Success));

        Request.Check(notifier);
    }
}