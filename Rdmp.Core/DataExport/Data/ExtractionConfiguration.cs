// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IExtractionConfiguration"/>
public class ExtractionConfiguration : DatabaseEntity, IExtractionConfiguration, ICollectSqlParameters, INamed,
    ICustomSearchString
{
    #region Database Properties

    private DateTime? _dtCreated;
    private int? _cohort_ID;
    private string _requestTicket;
    private string _releaseTicket;
    private int _project_ID;
    private string _username;
    private string _separator;
    private string _description;
    private bool _isReleased;
    private string _name;
    private int? _clonedFrom_ID;

    private int? _defaultPipeline_ID;
    private int? _cohortIdentificationConfigurationID;
    private int? _cohortRefreshPipelineID;

    /// <inheritdoc/>
    public int? CohortRefreshPipeline_ID
    {
        get => _cohortRefreshPipelineID;
        set => SetField(ref _cohortRefreshPipelineID, value);
    }

    /// <inheritdoc/>
    public int? CohortIdentificationConfiguration_ID
    {
        get => _cohortIdentificationConfigurationID;
        set => SetField(ref _cohortIdentificationConfigurationID, value);
    }

    /// <inheritdoc/>
    public int? DefaultPipeline_ID
    {
        get => _defaultPipeline_ID;
        set => SetField(ref _defaultPipeline_ID, value);
    }

    /// <inheritdoc/>
    public DateTime? dtCreated
    {
        get => _dtCreated;
        set => SetField(ref _dtCreated, value);
    }

    /// <inheritdoc/>
    public int? Cohort_ID
    {
        get => _cohort_ID;
        set => SetField(ref _cohort_ID, value);
    }

    /// <inheritdoc/>
    public bool IsExtractable(out string reason)
    {
        if (IsReleased)
        {
            reason = "ExtractionConfiguration is released so cannot be executed";
            return false;
        }

        if (Cohort_ID == null)
        {
            reason = "No cohort has been configured for ExtractionConfiguration";
            return false;
        }

        if (!GetAllExtractableDataSets().Any())
        {
            reason = "ExtractionConfiguration does not have an selected datasets";
            return false;
        }

        reason = null;
        return true;
    }

    /// <inheritdoc/>
    public string RequestTicket
    {
        get => _requestTicket;
        set => SetField(ref _requestTicket, value);
    }

    /// <inheritdoc/>
    public string ReleaseTicket
    {
        get => _releaseTicket;
        set => SetField(ref _releaseTicket, value);
    }

    /// <inheritdoc/>
    public int Project_ID
    {
        get => _project_ID;
        set => SetField(ref _project_ID, value);
    }

    /// <inheritdoc/>
    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    /// <inheritdoc/>
    public string Separator
    {
        get => _separator;
        set => SetField(ref _separator, value);
    }

    /// <inheritdoc/>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <inheritdoc/>
    public bool IsReleased
    {
        get => _isReleased;
        set => SetField(ref _isReleased, value);
    }

    /// <inheritdoc/>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public int? ClonedFrom_ID
    {
        get => _clonedFrom_ID;
        set => SetField(ref _clonedFrom_ID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IProject Project => Repository.GetObjectByID<Project>(Project_ID);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ISqlParameter[] GlobalExtractionFilterParameters =>
        Repository.GetAllObjectsWithParent<GlobalExtractionFilterParameter>(this)
            .Cast<ISqlParameter>().ToArray();

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults =>
        Repository.GetAllObjectsWithParent<CumulativeExtractionResults>(this);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IEnumerable<ISupplementalExtractionResults> SupplementalExtractionResults =>
        Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IExtractableCohort Cohort =>
        Cohort_ID == null ? null : Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ISelectedDataSets[] SelectedDataSets => Repository.GetAllObjectsWithParent<SelectedDataSets>(this)
        .Cast<ISelectedDataSets>().ToArray();

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IReleaseLog[] ReleaseLog
    {
        get
        {
            return CumulativeExtractionResults.Select(c => c.GetReleaseLogEntryIfAny()).Where(l => l != null).ToArray();
        }
    }

    /// <inheritdoc cref="DefaultPipeline_ID"/>
    [NoMappingToDatabase]
    public IPipeline DefaultPipeline =>
        DefaultPipeline_ID == null
            ? null
            : (IPipeline)((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<Pipeline>(
                DefaultPipeline_ID.Value);


    /// <inheritdoc cref="CohortIdentificationConfiguration_ID"/>
    [NoMappingToDatabase]
    public CohortIdentificationConfiguration CohortIdentificationConfiguration =>
        CohortIdentificationConfiguration_ID == null
            ? null
            : ((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(
                CohortIdentificationConfiguration_ID.Value);

    /// <inheritdoc cref="CohortRefreshPipeline_ID"/>
    [NoMappingToDatabase]
    public IPipeline CohortRefreshPipeline =>
        CohortRefreshPipeline_ID == null
            ? null
            : (IPipeline)((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<Pipeline>(
                CohortRefreshPipeline_ID.Value);

    /// <summary>
    /// Returns a name suitable for describing the extraction of a dataset(s) from this configuration (in a <see cref="DataLoadInfo"/>)
    /// </summary>
    /// <returns></returns>
    public string GetLoggingRunName() => $"{Project.Name} {GetExtractionLoggingName()}";

    private string GetExtractionLoggingName() => $"(ExtractionConfiguration ID={ID})";

    #endregion

    /// <summary>
    /// Returns <see cref="INamed.Name"/>
    /// </summary>
    [NoMappingToDatabase]
    [UsefulProperty]
    public string ProjectName => Project.Name;

    public ExtractionConfiguration()
    {
        // Default (also default in db)
        Separator = ",";
    }

    /// <summary>
    /// Creates a new extraction configuration in the <paramref name="repository"/> database for the provided <paramref name="project"/>.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="project"></param>
    /// <param name="name"></param>
    public ExtractionConfiguration(IDataExportRepository repository, IProject project, string name = null)
    {
        Repository = repository;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "dtCreated", DateTime.Now },
            { "Project_ID", project.ID },
            { "Username", Environment.UserName },
            { "Description", "Initial Configuration" },
            { "Name", string.IsNullOrWhiteSpace(name) ? $"New ExtractionConfiguration{Guid.NewGuid()}" : name },
            { "Separator", "," }
        });
    }

    /// <summary>
    /// Provides a short human readable representation of the <see cref="Project"/> to which this
    /// <see cref="ExtractionConfiguration"/> is associated with
    /// </summary>
    /// <param name="shortString">True for a short representation.  False for a longer representation.</param>
    /// <returns></returns>
    public string GetProjectHint(bool shortString) =>
        shortString ? $"({Project.ProjectNumber})" : $"'{Project.Name}' (PNo. {Project.ProjectNumber})";

    /// <summary>
    /// Reads an existing <see cref="IExtractionConfiguration"/> out of the  <paramref name="repository"/> database.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal ExtractionConfiguration(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Project_ID = int.Parse(r["Project_ID"].ToString());

        if (!string.IsNullOrWhiteSpace(r["Cohort_ID"].ToString()))
            Cohort_ID = int.Parse(r["Cohort_ID"].ToString());

        RequestTicket = r["RequestTicket"].ToString();
        ReleaseTicket = r["ReleaseTicket"].ToString();

        var dt = r["dtCreated"];

        if (dt == null || dt == DBNull.Value)
            dtCreated = null;
        else
            dtCreated = (DateTime)dt;

        Username = r["Username"] as string;
        Description = r["Description"] as string;
        Separator = r["Separator"] as string;
        IsReleased = (bool)r["IsReleased"];
        Name = r["Name"] as string;

        if (r["ClonedFrom_ID"] == DBNull.Value)
            ClonedFrom_ID = null;
        else
            ClonedFrom_ID = Convert.ToInt32(r["ClonedFrom_ID"]);

        DefaultPipeline_ID = ObjectToNullableInt(r["DefaultPipeline_ID"]);
        CohortIdentificationConfiguration_ID = ObjectToNullableInt(r["CohortIdentificationConfiguration_ID"]);
        CohortRefreshPipeline_ID = ObjectToNullableInt(r["CohortRefreshPipeline_ID"]);
    }

    /// <inheritdoc/>
    public string GetSearchString() => $"{ToString()}_{RequestTicket}_{ReleaseTicket}";

    /// <inheritdoc/>
    public ISqlParameter[] GetAllParameters() => GlobalExtractionFilterParameters;

    /// <summary>
    /// Returns the configuration Name
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;

    public bool ShouldBeReadOnly(out string reason)
    {
        if (IsReleased)
        {
            reason = $"{ToString()} has already been released";
            return true;
        }

        reason = null;
        return false;
    }

    /// <summary>
    /// Creates a complete copy of the <see cref="IExtractionConfiguration"/>, all selected datasets, filters etc.  The copy is created directly into
    /// the <see cref="DatabaseEntity.Repository"/> database using a transaction (to prevent a half successful clone being generated).
    /// </summary>
    /// <returns></returns>
    public ExtractionConfiguration DeepCloneWithNewIDs()
    {
        var repo = (IDataExportRepository)Repository;
        using (repo.BeginNewTransaction())
        {
            try
            {
                //clone the root object (the configuration) - this includes cloning the link to the correct project and cohort       
                var clone = ShallowClone();

                // Clone GlobalExtractionFilterParameters
                foreach (var param in GlobalExtractionFilterParameters.OfType<GlobalExtractionFilterParameter>())
                {
                    // Create the new parameter with the same SQL declaration
                    var clonedParam = new GlobalExtractionFilterParameter(repo, clone, param.ParameterSQL);

                    // Copy value and comment if present
                    clonedParam.Value = param.Value;
                    clonedParam.Comment = param.Comment;

                    clonedParam.SaveToDatabase();
                }

                //find each of the selected datasets for ourselves and clone those too
                foreach (SelectedDataSets selected in SelectedDataSets)
                {
                    //clone the link meaning that the dataset is now selected for the clone configuration too
                    var newSelectedDataSet = new SelectedDataSets(repo, clone, selected.ExtractableDataSet, null);

                    // now clone each of the columns for each of the datasets that we just created links to (make them the same as the old configuration
                    foreach (var cloneExtractableColumn in GetAllExtractableColumnsFor(selected.ExtractableDataSet).Select(static extractableColumn => extractableColumn.ShallowClone()))
                    {
                        cloneExtractableColumn.ExtractionConfiguration_ID = clone.ID;
                        cloneExtractableColumn.SaveToDatabase();
                    }

                    //clone should copy across the forced joins (if any)
                    foreach (var oldForcedJoin in Repository.GetAllObjectsWithParent<SelectedDataSetsForcedJoin>(
                                 selected))
                        new SelectedDataSetsForcedJoin((IDataExportRepository)Repository, newSelectedDataSet,
                            oldForcedJoin.TableInfo);

                    // clone should copy any ExtractionProgresses
                    if (selected.ExtractionProgressIfAny != null)
                    {
                        var old = selected.ExtractionProgressIfAny;
                        var clonedProgress = new ExtractionProgress(repo, newSelectedDataSet, old.StartDate,
                            old.EndDate, old.NumberOfDaysPerBatch, old.Name, old.ExtractionInformation_ID);

                        // Notice that we do not set the ProgressDate because the cloned copy should be extracting from the beginning
                        // when it is run.  We don't want the user to have to manually reset it
                        clonedProgress.SaveToDatabase();
                    }

                    try
                    {
                        //clone the root filter container
                        var rootContainer = (FilterContainer)GetFilterContainerFor(selected.ExtractableDataSet);

                        //turns out there wasn't one to clone at all
                        if (rootContainer == null)
                            continue;

                        //there was one to clone so clone it recursively (all subcontainers) including filters then set the root filter to the new clone
                        var cloneRootContainer = rootContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
                        newSelectedDataSet.RootFilterContainer_ID = cloneRootContainer.ID;
                        newSelectedDataSet.SaveToDatabase();
                    }
                    catch (Exception e)
                    {
                        clone.DeleteInDatabase();
                        throw new Exception(
                            $"Problem occurred during cloning filters, problem was {e.Message} deleted the clone configuration successfully",
                            e);
                    }
                }

                clone.dtCreated = DateTime.Now;
                clone.IsReleased = false;
                clone.Username = Environment.UserName;
                clone.Description = "TO" + "DO:Populate change log here";
                clone.ReleaseTicket = null;

                //wire up some changes
                clone.ClonedFrom_ID = ID;
                clone.SaveToDatabase();

                repo.EndTransaction(true);

                return clone;
            }
            catch (Exception)
            {
                repo.EndTransaction(false);
                throw;
            }
        }
    }

    private ExtractionConfiguration ShallowClone()
    {
        var clone = new ExtractionConfiguration(DataExportRepository, Project);
        CopyShallowValuesTo(clone);

        clone.Name = $"Clone of {Name}";
        clone.SaveToDatabase();
        return clone;
    }

    /// <inheritdoc/>
    public IProject GetProject() => Repository.GetObjectByID<Project>(Project_ID);

    /// <inheritdoc/>
    public ExtractableColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset)
    {
        return
            Repository.GetAllObjectsWhere<ExtractableColumn>("ExtractionConfiguration_ID", ID)
                .Where(e => e.ExtractableDataSet_ID == dataset.ID).ToArray();
    }

    /// <inheritdoc/>
    public IContainer GetFilterContainerFor(IExtractableDataSet dataset)
    {
        return Repository.GetAllObjectsWhere<SelectedDataSets>("ExtractionConfiguration_ID", ID)
            .Single(sds => sds.ExtractableDataSet_ID == dataset.ID)
            .RootFilterContainer;
    }

    private ExternalDatabaseServer GetDistinctLoggingServer(bool testLoggingServer)
    {
        var uniqueLoggingServerID = -1;

        var repo = (IDataExportRepository)Repository;

        foreach (int? catalogueID in GetAllExtractableDataSets().Select(ds => ds.Catalogue_ID))
        {
            if (catalogueID == null)
                throw new Exception(
                    "Cannot get logging server because some ExtractableDatasets in the configuration do not have associated Catalogues (possibly the Catalogue was deleted)");

            var catalogue = repo.CatalogueRepository.GetObjectByID<Catalogue>((int)catalogueID);

            var loggingServer = catalogue.LiveLoggingServer_ID ?? throw new Exception(
                $"Catalogue {catalogue.Name} does not have a {(testLoggingServer ? "test" : "")} logging server configured");
            if (uniqueLoggingServerID == -1)
            {
                uniqueLoggingServerID = (int)catalogue.LiveLoggingServer_ID;
            }
            else
            {
                if (uniqueLoggingServerID != catalogue.LiveLoggingServer_ID)
                    throw new Exception("Catalogues in configuration have different logging servers");
            }
        }

        return repo.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(uniqueLoggingServerID);
    }

    /// <inheritdoc/>
    public IExtractableCohort GetExtractableCohort() => Cohort_ID == null
        ? null
        : (IExtractableCohort)Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);

    /// <inheritdoc/>
    public IExtractableDataSet[] GetAllExtractableDataSets()
    {
        return
            Repository.GetAllObjectsWithParent<SelectedDataSets>(this)
                .Select(sds => sds.ExtractableDataSet)
                .ToArray();
    }

    /// <summary>
    /// Makes the provided <paramref name="extractableDataSet"/> extractable in the current <see cref="IExtractionConfiguration"/>.  This
    /// includes selecting it (<see cref="ISelectedDataSets"/>) and replicating any mandatory filters.
    /// </summary>
    /// <param name="extractableDataSet"></param>
    public void AddDatasetToConfiguration(IExtractableDataSet extractableDataSet)
    {
        AddDatasetToConfiguration(extractableDataSet, out _);
    }

    /// <summary>
    /// Makes the provided <paramref name="extractableDataSet"/> extractable in the current <see cref="IExtractionConfiguration"/>.  This
    /// includes selecting it (<see cref="ISelectedDataSets"/>) and replicating any mandatory filters.
    /// </summary>
    /// <param name="extractableDataSet"></param>
    /// <param name="selectedDataSet">The RDMP object that indicates that the dataset is extracted in this configuration</param>
    public void AddDatasetToConfiguration(IExtractableDataSet extractableDataSet, out ISelectedDataSets selectedDataSet)
    {
        selectedDataSet = null;

        //it is already part of the configuration
        if (SelectedDataSets.Any(s => s.ExtractableDataSet_ID == extractableDataSet.ID))
            return;

        var dataExportRepo = (IDataExportRepository)Repository;

        selectedDataSet = new SelectedDataSets(dataExportRepo, this, extractableDataSet, null);

        var mandatoryExtractionFiltersToApplyToDataset = extractableDataSet.Catalogue.GetAllMandatoryFilters();

        //add mandatory filters
        if (mandatoryExtractionFiltersToApplyToDataset.Any())
        {
            //first we need a root container e.g. an AND container
            //add the AND container and set it as the root container for the dataset configuration
            var rootFilterContainer = new FilterContainer(dataExportRepo)
            {
                Operation = FilterContainerOperation.AND
            };
            rootFilterContainer.SaveToDatabase();

            selectedDataSet.RootFilterContainer_ID = rootFilterContainer.ID;
            selectedDataSet.SaveToDatabase();

            var globals = GlobalExtractionFilterParameters;
            var importer = new FilterImporter(new DeployedExtractionFilterFactory(dataExportRepo), globals);

            var mandatoryFilters =
                importer.ImportAllFilters(rootFilterContainer, mandatoryExtractionFiltersToApplyToDataset, null);

            foreach (var filter in mandatoryFilters.Cast<DeployedExtractionFilter>())
            {
                filter.FilterContainer_ID = rootFilterContainer.ID;
                filter.SaveToDatabase();
            }
        }

        var legacyColumns = GetAllExtractableColumnsFor(extractableDataSet).Cast<ExtractableColumn>().ToArray();

        //add Core or ProjectSpecific columns
        foreach (var all in extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            if (all.ExtractionCategory == ExtractionCategory.Core ||
                all.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                if (legacyColumns.All(l => l.CatalogueExtractionInformation_ID != all.ID))
                    AddColumnToExtraction(extractableDataSet, all);
    }

    /// <inheritdoc/>
    public void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet)
    {
        var match = SelectedDataSets.SingleOrDefault(s => s.ExtractableDataSet_ID == extractableDataSet.ID);
        match?.DeleteInDatabase();
    }

    /// <summary>
    /// Makes the given <paramref name="column"/> SELECT Sql part of the query for linking and extracting the provided <paramref name="forDataSet"/>
    /// for this <see cref="IExtractionConfiguration"/>.
    /// </summary>
    /// <param name="forDataSet"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public ExtractableColumn AddColumnToExtraction(IExtractableDataSet forDataSet, IColumn column)
    {
        if (string.IsNullOrWhiteSpace(column.SelectSQL))
            throw new ArgumentException(
                $"IColumn ({column.GetType().Name}) {column} has a blank value for SelectSQL, fix this in the CatalogueManager",
                nameof(column));

        var query = column.SelectSQL;

        ExtractableColumn addMe;

        if (column is ExtractionInformation extractionInformation)
            addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, extractionInformation, -1, query);
        else
            addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, null, -1,
                query); // its custom column of some kind, not tied to a catalogue entry

        addMe.UpdateValuesToMatch(column);

        return addMe;
    }

    /// <summary>
    /// Returns the logging server that should be used to audit extraction executions of this <see cref="IExtractionConfiguration"/>.
    /// </summary>
    /// <returns></returns>
    public LogManager GetExplicitLoggingDatabaseServerOrDefault()
    {
        ExternalDatabaseServer loggingServer;
        try
        {
            loggingServer = GetDistinctLoggingServer(false);
        }
        catch (Exception e)
        {
            //failed to get a logging server correctly

            //see if there is a default
            var defaultGetter = Project.DataExportRepository.CatalogueRepository;
            var defaultLoggingServer = defaultGetter.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            //there is a default?
            if (defaultLoggingServer != null)
                loggingServer = (ExternalDatabaseServer)defaultLoggingServer;
            else
                //no, there is no default or user does not want to use it.
                throw new Exception(
                    "There is no default logging server configured and there was a problem asking Catalogues for a logging server instead.  Configure a default logging server via ManageExternalServersUI",
                    e);
        }

        var server = DataAccessPortal.ExpectServer(loggingServer, DataAccessContext.Logging);

        LogManager lm;

        try
        {
            lm = new LogManager(server);

            if (!lm.ListDataTasks().Contains(ExecuteDatasetExtractionSource.AuditTaskName))
                throw new Exception(
                    $"The logging database {server} does not contain a DataLoadTask called '{ExecuteDatasetExtractionSource.AuditTaskName}' (all data exports are logged under this task regardless of dataset/Catalogue)");
        }
        catch (Exception e)
        {
            throw new Exception($"Problem figuring out what logging server to use:{Environment.NewLine}\t{e.Message}",
                e);
        }

        return lm;
    }

    /// <inheritdoc/>
    public void Unfreeze()
    {
        foreach (var l in ReleaseLog)
            l.DeleteInDatabase();

        foreach (var r in CumulativeExtractionResults)
            r.DeleteInDatabase();

        IsReleased = false;
        SaveToDatabase();
    }

    /// <inheritdoc/>
    public IMapsDirectlyToDatabaseTable[] GetGlobals()
    {
        var sds = SelectedDataSets.FirstOrDefault(s => s.ExtractableDataSet.Catalogue != null);

        if (sds == null)
            return Array.Empty<IMapsDirectlyToDatabaseTable>();

        var cata = sds.ExtractableDataSet.Catalogue;

        return
            cata.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableGlobals)
                .Cast<IMapsDirectlyToDatabaseTable>()
                .Union(
                    cata.GetAllSupportingDocuments(FetchOptions.ExtractableGlobals))
                .ToArray();
    }

    /// <inheritdoc/>
    public override void DeleteInDatabase()
    {
        // Delete only GlobalExtractionFilterParameters for this configuration
        foreach (var param in Repository.GetAllObjectsWithParent<GlobalExtractionFilterParameter>(this))
            param.DeleteInDatabase();

        foreach (var result in Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this))
            result.DeleteInDatabase();

        base.DeleteInDatabase();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new[] { Project };
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => Array.Empty<IHasDependencies>();

    public DiscoveredServer GetDistinctLoggingDatabase() =>
        GetDistinctLoggingServer(false).Discover(DataAccessContext.Logging).Server;

    public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
    {
        serverChosen = GetDistinctLoggingServer(false);
        return serverChosen.Discover(DataAccessContext.Logging).Server;
    }

    public string GetDistinctLoggingTask() => ExecuteDatasetExtractionSource.AuditTaskName;

    /// <summary>
    /// Returns runs from the data extraction task where the run was for this ExtractionConfiguration
    /// </summary>
    /// <param name="runs"></param>
    /// <returns></returns>
    public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs)
    {
        // allow for the project name changing but not our ID
        return runs.Where(r => r.Description.Contains(GetExtractionLoggingName()));
    }
}