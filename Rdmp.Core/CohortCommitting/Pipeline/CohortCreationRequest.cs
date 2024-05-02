// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Connections;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
///     All metadata details nessesary to create a cohort including which project it goes into, its name, version etc.
///     There are no identifiers for the cohort.
///     Also functions as the use case for cohort creation (to which it passes itself as an input object).
/// </summary>
public sealed class CohortCreationRequest : PipelineUseCase, ICohortCreationRequest, ICanBeSummarised
{
    private readonly IDataExportRepository _repository;

    //for pipeline editing initialization when no known cohort is available

    #region Things that can be turned into cohorts

    private FlatFileToLoad _fileToLoad;
    private ExtractionInformation _extractionIdentifierColumn;
    private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

    public FlatFileToLoad FileToLoad
    {
        get => _fileToLoad;
        set
        {
            //remove old value if it had one
            Pop(_fileToLoad);

            _fileToLoad = value;

            //add the new one
            Push(value);
        }
    }

    public CohortIdentificationConfiguration CohortIdentificationConfiguration
    {
        get => _cohortIdentificationConfiguration;
        set
        {
            Pop(_cohortIdentificationConfiguration);
            _cohortIdentificationConfiguration = value;
            Push(value);
        }
    }

    public ExtractionInformation ExtractionIdentifierColumn
    {
        get => _extractionIdentifierColumn;
        set
        {
            Pop(_extractionIdentifierColumn);
            _extractionIdentifierColumn = value;
            Push(_extractionIdentifierColumn);
        }
    }

    private void Pop(object oldValue)
    {
        if (oldValue != null && InitializationObjects.Contains(oldValue))
            InitializationObjects.Remove(oldValue);
    }

    private void Push(object newValue)
    {
        AddInitializationObject(newValue);
    }

    #endregion

    public IProject Project { get; }
    public ICohortDefinition NewCohortDefinition { get; set; }

    public ExtractableCohort CohortCreatedIfAny { get; set; }

    public string DescriptionForAuditLog { get; set; }

    public CohortCreationRequest(IProject project, ICohortDefinition newCohortDefinition,
        IDataExportRepository repository, string descriptionForAuditLog)
    {
        _repository = repository;
        Project = project;
        NewCohortDefinition = newCohortDefinition;

        DescriptionForAuditLog = descriptionForAuditLog;

        AddInitializationObject(Project);
        AddInitializationObject(this);

        GenerateContext();
    }

    /// <summary>
    ///     For refreshing the current extraction configuration CohortIdentificationConfiguration ONLY.  The
    ///     ExtractionConfiguration must have a cic and a refresh pipeline configured on it.
    /// </summary>
    /// <param name="configuration"></param>
    public CohortCreationRequest(ExtractionConfiguration configuration)
    {
        _repository = (IDataExportRepository)configuration.Repository;

        if (configuration.CohortIdentificationConfiguration_ID == null)
            throw new NotSupportedException(
                $"Configuration '{configuration}' does not have an associated CohortIdentificationConfiguration for cohort refreshing");

        var origCohort = configuration.Cohort;
        var origCohortData = origCohort.GetExternalData();
        CohortIdentificationConfiguration = configuration.CohortIdentificationConfiguration;
        Project = configuration.Project;

        if (Project.ProjectNumber == null)
            throw new ProjectNumberException($"Project '{Project}' does not have a ProjectNumber");

        var definition = new CohortDefinition(null, origCohortData.ExternalDescription,
            origCohortData.ExternalVersion + 1, (int)Project.ProjectNumber, origCohort.ExternalCohortTable)
        {
            CohortReplacedIfAny = origCohort
        };

        NewCohortDefinition = definition;
        DescriptionForAuditLog = "Cohort Refresh";

        AddInitializationObject(Project);
        AddInitializationObject(CohortIdentificationConfiguration);
        AddInitializationObject(FileToLoad);
        AddInitializationObject(ExtractionIdentifierColumn);
        AddInitializationObject(this);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        return new DataFlowPipelineContext<DataTable>
        {
            MustHaveDestination = typeof(ICohortPipelineDestination),
            MustHaveSource = typeof(IDataFlowSource<DataTable>)
        };
    }


    public void Check(ICheckNotifier notifier)
    {
        NewCohortDefinition.LocationOfCohort.Check(notifier);

        if (NewCohortDefinition.ID != null)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Expected the cohort definition {NewCohortDefinition} to have a null ID - we are trying to create this, why would it already exist?",
                    CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Confirmed that cohort ID is null", CheckResult.Success));

        if (Project.ProjectNumber == null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Project {Project} does not have a ProjectNumber specified, it should have the same number as the CohortCreationRequest ({NewCohortDefinition.ProjectNumber})",
                CheckResult.Fail));
        else if (Project.ProjectNumber != NewCohortDefinition.ProjectNumber)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Project {Project} has ProjectNumber={Project.ProjectNumber} but the CohortCreationRequest.ProjectNumber is {NewCohortDefinition.ProjectNumber}",
                    CheckResult.Fail));


        if (!NewCohortDefinition.IsAcceptableAsNewCohort(out var matchDescription))
            notifier.OnCheckPerformed(new CheckEventArgs($"Cohort failed novelness check:{matchDescription}",
                CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Confirmed that cohort {NewCohortDefinition} does not already exist",
                CheckResult.Success));

        if (string.IsNullOrWhiteSpace(DescriptionForAuditLog))
            notifier.OnCheckPerformed(
                new CheckEventArgs("User did not provide a description of the cohort for the AuditLog",
                    CheckResult.Fail));
    }

    public void PushToServer(IManagedConnection connection)
    {
        if (!NewCohortDefinition.IsAcceptableAsNewCohort(out var reason))
            throw new Exception(reason);

        NewCohortDefinition.LocationOfCohort.PushToServer(NewCohortDefinition, connection);
    }

    public int ImportAsExtractableCohort(bool deprecateOldCohortOnSuccess, bool migrateUsages)
    {
        if (NewCohortDefinition.ID == null)
            throw new NotSupportedException(
                "CohortCreationRequest cannot be imported because its ID is null, it is likely that it has not been pushed to the server yet");

        var cohort = new ExtractableCohort(_repository, (ExternalCohortTable)NewCohortDefinition.LocationOfCohort,
            (int)NewCohortDefinition.ID);
        cohort.AppendToAuditLog(DescriptionForAuditLog);

        CohortCreatedIfAny = cohort;

        if (deprecateOldCohortOnSuccess && NewCohortDefinition.CohortReplacedIfAny != null)
        {
            NewCohortDefinition.CohortReplacedIfAny.IsDeprecated = true;
            NewCohortDefinition.CohortReplacedIfAny.SaveToDatabase();
        }

        if (migrateUsages && NewCohortDefinition.CohortReplacedIfAny != null)
        {
            var oldId = NewCohortDefinition.CohortReplacedIfAny.ID;
            var newId = cohort.ID;

            // ExtractionConfigurations that use the old (replaced) cohort
            var liveUsers = _repository.GetAllObjects<ExtractionConfiguration>()
                .Where(ec => ec.Cohort_ID == oldId && ec.IsReleased == false);

            foreach (var ec in liveUsers)
            {
                ec.Cohort_ID = newId;
                ec.SaveToDatabase();
            }
        }

        return cohort.ID;
    }


    /// <summary>
    ///     Design time types
    /// </summary>
    private CohortCreationRequest() : base(new[]
    {
        typeof(FlatFileToLoad),
        typeof(CohortIdentificationConfiguration),
        typeof(Project),
        typeof(ExtractionInformation),
        typeof(ICohortCreationRequest)
    })
    {
        GenerateContext();
    }

    public static PipelineUseCase DesignTime()
    {
        return new CohortCreationRequest();
    }

    public override string ToString()
    {
        return NewCohortDefinition == null ? base.ToString() : NewCohortDefinition.Description;
    }

    public string GetSummary(bool includeName, bool includeId)
    {
        return $"External Cohort Table: {NewCohortDefinition?.LocationOfCohort}";
    }
}