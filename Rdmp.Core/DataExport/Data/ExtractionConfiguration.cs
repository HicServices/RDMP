// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.Data
{
    /// <inheritdoc cref="IExtractionConfiguration"/>
    public class ExtractionConfiguration : DatabaseEntity, IExtractionConfiguration, ICollectSqlParameters,INamed,ICustomSearchString
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
            get { return _cohortRefreshPipelineID; }
            set {  SetField(ref _cohortRefreshPipelineID , value); }
        }

        /// <inheritdoc/>
        public int? CohortIdentificationConfiguration_ID
        {
            get { return _cohortIdentificationConfigurationID; }
            set { SetField(ref _cohortIdentificationConfigurationID, value); }
        }

        /// <inheritdoc/>
        public int? DefaultPipeline_ID
        {
            get { return _defaultPipeline_ID; }
            set { SetField(ref _defaultPipeline_ID, value); }
        }

        /// <inheritdoc/>
        public DateTime? dtCreated
        {
            get { return _dtCreated; }
            set { SetField(ref _dtCreated, value); }
        }

        /// <inheritdoc/>
        public int? Cohort_ID
        {
            get { return _cohort_ID; }
            set { SetField(ref _cohort_ID, value); }
        }

        /// <inheritdoc/>
        public bool IsExtractable(out string reason)
        {
            if(IsReleased)
            {
                reason = "ExtractionConfiguration is released so cannot be executed";
                return false;
            }

            if(Cohort_ID == null)
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
            get { return _requestTicket; }
            set { SetField(ref _requestTicket, value); }
        }
        /// <inheritdoc/>
        public string ReleaseTicket
        {
            get { return _releaseTicket; }
            set { SetField(ref _releaseTicket, value); }
        }
        /// <inheritdoc/>
        public int Project_ID
        {
            get { return _project_ID; }
            set { SetField(ref _project_ID, value); }
        }
        /// <inheritdoc/>
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }
        /// <inheritdoc/>
        public string Separator
        {
            get { return _separator; }
            set { SetField(ref _separator, value); }
        }
        /// <inheritdoc/>
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }
        /// <inheritdoc/>
        public bool IsReleased
        {
            get { return _isReleased; }
            set { SetField(ref _isReleased, value); }
        }

        /// <inheritdoc/>
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <inheritdoc/>
        public int? ClonedFrom_ID
        {
            get { return _clonedFrom_ID; }
            set { SetField(ref _clonedFrom_ID, value); }
        }

        #endregion
        
        #region Relationships
        
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IProject Project
        {
            get { return Repository.GetObjectByID<Project>(Project_ID); }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ISqlParameter[] GlobalExtractionFilterParameters
        {
            get
            {
                return
                    Repository.GetAllObjectsWithParent<GlobalExtractionFilterParameter>(this)
                        .Cast<ISqlParameter>().ToArray();
            }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults
        {
            get
            {
                return Repository.GetAllObjectsWithParent<CumulativeExtractionResults>(this);
            }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IEnumerable<ISupplementalExtractionResults> SupplementalExtractionResults
        {
            get
            {
                return Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this);
            }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IExtractableCohort Cohort
        {
            get
            {
                return Cohort_ID == null ? null : Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);
            }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ISelectedDataSets[] SelectedDataSets
        {
            get
            {
                return Repository.GetAllObjectsWithParent<SelectedDataSets>(this).Cast<ISelectedDataSets>().ToArray();
            }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IReleaseLog[] ReleaseLog
        {
            get { return CumulativeExtractionResults.Select(c => c.GetReleaseLogEntryIfAny()).Where(l => l != null).ToArray(); }
        }

        /// <inheritdoc cref="DefaultPipeline_ID"/>
        [NoMappingToDatabase]
        public IPipeline DefaultPipeline {
            get
            {
                if (DefaultPipeline_ID == null)
                    return null;

                return
                    ((DataExportRepository) Repository).CatalogueRepository.GetObjectByID<Pipeline>(DefaultPipeline_ID.Value);
            }}


        /// <inheritdoc cref="CohortIdentificationConfiguration_ID"/>
        [NoMappingToDatabase]
        public CohortIdentificationConfiguration CohortIdentificationConfiguration
        {
            get
            {
                if (CohortIdentificationConfiguration_ID == null)
                    return null;

                return
                    ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(CohortIdentificationConfiguration_ID.Value);
            }
        }

        /// <inheritdoc cref="CohortRefreshPipeline_ID"/>
        [NoMappingToDatabase]
        public IPipeline CohortRefreshPipeline
        {
            get
            {
                if (CohortRefreshPipeline_ID == null)
                    return null;

                return
                    ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<Pipeline>(CohortRefreshPipeline_ID.Value);
            }
        }


        #endregion
        
        /// <summary>
        /// Creates a new extraction configuration in the <paramref name="repository"/> database for the provided <paramref name="project"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="project"></param>
        public ExtractionConfiguration(IDataExportRepository repository, IProject project)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"dtCreated", DateTime.Now},
                {"Project_ID", project.ID},
                {"Username", Environment.UserName},
                {"Description", "Initial Configuration"},
                {"Name","New ExtractionConfiguration" + Guid.NewGuid() }
            });
        }

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

            object dt = r["dtCreated"];

            if (dt == null || dt == DBNull.Value)
                dtCreated = null;
            else
                dtCreated = (DateTime) dt;

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
        public string GetSearchString()
        {
            return ToString() + "_" + RequestTicket + "_" + ReleaseTicket;
        }

        /// <inheritdoc/>
        public ISqlParameter[] GetAllParameters()
        {
            return GlobalExtractionFilterParameters;
        }

        /// <summary>
        /// Returns the configuration Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        public bool ShouldBeReadOnly(out string reason)
        {
            if (IsReleased)
            {
                reason = ToString() + " has already been released";
                return true;
            }

            reason = null;
            return false;
        }

        /// <summary>
        /// Creates a complete copy of the <see cref="IExtractionConfiguration"/>, all selected datasets, filters etc.  The copy is created directly into
        /// the <see cref="DatabaseEntity.Repository"/> database using a transaction (to prevent a half succesful clone being generated).
        /// </summary>
        /// <returns></returns>
        public ExtractionConfiguration DeepCloneWithNewIDs()
        {
            var repo = (DataExportRepository)Repository;
            using (repo.BeginNewTransactedConnection())
            {
                try
                {
                    //clone the root object (the configuration) - this includes cloning the link to the correct project and cohort 
                    ExtractionConfiguration clone = this.ShallowClone();

                    //find each of the selected datasets for ourselves and clone those too
                    foreach (SelectedDataSets selected in SelectedDataSets)
                    {
                        //clone the link meaning that the dataset is now selected for the clone configuration too 
                        var newSelectedDataSet = new SelectedDataSets(repo, clone, selected.ExtractableDataSet, null);

                        // now clone each of the columns for each of the datasets that we just created links to (make them the same as the old configuration
                        foreach (IColumn extractableColumn in GetAllExtractableColumnsFor(selected.ExtractableDataSet))
                        {
                            ExtractableColumn cloneExtractableColumn = ((ExtractableColumn)extractableColumn).ShallowClone();
                            cloneExtractableColumn.ExtractionConfiguration_ID = clone.ID;
                            cloneExtractableColumn.SaveToDatabase();
                        }

                        //clone should copy accross the forced joins (if any)
                        foreach (SelectedDataSetsForcedJoin oldForcedJoin in Repository.GetAllObjectsWithParent<SelectedDataSetsForcedJoin>(selected))
                            new SelectedDataSetsForcedJoin((IDataExportRepository) Repository, newSelectedDataSet,oldForcedJoin.TableInfo);
                       

                        try
                        {
                            //clone the root filter container
                            var rootContainer = (FilterContainer)GetFilterContainerFor(selected.ExtractableDataSet);

                            //turns out there wasn't one to clone at all
                            if (rootContainer == null)
                                continue;

                            //there was one to clone so clone it recursively (all subcontainers) including filters then set the root filter to the new clone 
                            FilterContainer cloneRootContainer = rootContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
                            newSelectedDataSet.RootFilterContainer_ID = cloneRootContainer.ID;
                            newSelectedDataSet.SaveToDatabase();
                        }
                        catch (Exception e)
                        {
                            clone.DeleteInDatabase();
                            throw new Exception("Problem occurred during cloning filters, problem was " + e.Message + " deleted the clone configuration successfully",e);
                        }
                    }

                    clone.dtCreated = DateTime.Now;
                    clone.IsReleased = false;
                    clone.Username = Environment.UserName;
                    clone.Description = "TO"+"DO:Populate change log here";
                    clone.ReleaseTicket = null;

                    //wire up some changes
                    clone.ClonedFrom_ID = this.ID;
                    clone.SaveToDatabase();

                    repo.EndTransactedConnection(true);

                    return clone;
                }
                catch (Exception)
                {
                    repo.EndTransactedConnection(false);
                    throw;
                }
            }
        }

        private ExtractionConfiguration ShallowClone()
        {
            var clone = new ExtractionConfiguration(DataExportRepository, Project);
            CopyShallowValuesTo(clone);

            clone.Name = "Clone of " + Name;
            clone.SaveToDatabase();
            return clone;
        }

        /// <inheritdoc/>
        public IProject GetProject()
        {
            return Repository.GetObjectByID<Project>(Project_ID);
        }

        /// <inheritdoc/>
        public ConcreteColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset)
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
            int uniqueLoggingServerID = -1;

            var repo = ((DataExportRepository) Repository);

            foreach (int? catalogueID in GetAllExtractableDataSets().Select(ds=>ds.Catalogue_ID))
            {
                if(catalogueID == null)
                    throw new Exception("Cannot get logging server because some ExtractableDatasets in the configuration do not have associated Catalogues (possibly the Catalogue was deleted)");

                var catalogue = repo.CatalogueRepository.GetObjectByID<Catalogue>((int)catalogueID);

                int? loggingServer = catalogue.LiveLoggingServer_ID;

                if ( loggingServer == null)
                    throw new Exception("Catalogue " + catalogue.Name + " does not have a "+(testLoggingServer?"test":"")+" logging server configured");

                if (uniqueLoggingServerID == -1)
                    uniqueLoggingServerID = (int) catalogue.LiveLoggingServer_ID;
                else
                {
                    if(uniqueLoggingServerID != catalogue.LiveLoggingServer_ID)
                        throw new Exception("Catalogues in configuration have different logging servers");
                }
            }

            return repo.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(uniqueLoggingServerID);
        }

        /// <inheritdoc/>
        public IExtractableCohort GetExtractableCohort()
        {
            if (Cohort_ID == null)
                return null;

            return Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);
        }

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
            //it is already part of the configuration
            if( SelectedDataSets.Any(s => s.ExtractableDataSet_ID == extractableDataSet.ID))
                return;

            var dataExportRepo = (IDataExportRepository)Repository;

            var selectedDataSet = new SelectedDataSets(dataExportRepo, this, extractableDataSet, null);

            ExtractionFilter[] mandatoryExtractionFiltersToApplyToDataset = extractableDataSet.Catalogue.GetAllMandatoryFilters();

            //add mandatory filters
            if (mandatoryExtractionFiltersToApplyToDataset.Any())
            {
                //first we need a root container e.g. an AND container
                //add the AND container and set it as the root container for the dataset configuration
                FilterContainer rootFilterContainer = new FilterContainer(dataExportRepo);
                rootFilterContainer.Operation = FilterContainerOperation.AND;
                rootFilterContainer.SaveToDatabase();

                selectedDataSet.RootFilterContainer_ID = rootFilterContainer.ID;
                selectedDataSet.SaveToDatabase();

                var globals = GlobalExtractionFilterParameters;
                var importer = new FilterImporter(new DeployedExtractionFilterFactory(dataExportRepo), globals);

                var mandatoryFilters = importer.ImportAllFilters(mandatoryExtractionFiltersToApplyToDataset, null);

                foreach (DeployedExtractionFilter filter in mandatoryFilters.Cast<DeployedExtractionFilter>())
                {
                    filter.FilterContainer_ID = rootFilterContainer.ID;
                    filter.SaveToDatabase();
                }
            }

            var legacyColumns = GetAllExtractableColumnsFor(extractableDataSet).Cast<ExtractableColumn>().ToArray();

            //add Core or ProjectSpecific columns
            foreach (var all in extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
                if(all.ExtractionCategory == ExtractionCategory.Core || all.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                    if (legacyColumns.All(l => l.CatalogueExtractionInformation_ID != all.ID))
                        AddColumnToExtraction(extractableDataSet, all);
        }

        /// <inheritdoc/>
        public void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet)
        {
            var match = SelectedDataSets.SingleOrDefault(s => s.ExtractableDataSet_ID == extractableDataSet.ID);
            if(match != null)
                match.DeleteInDatabase();
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
                throw new ArgumentException("IColumn (" + column.GetType().Name + ") " + column + " has a blank value for SelectSQL, fix this in the CatalogueManager", "item");

            string query = "";
            query = column.SelectSQL;

            ExtractableColumn addMe;

            if (column is ExtractionInformation)
                addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, column as ExtractionInformation, -1, query);
            else
                addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, null, -1, query); // its custom column of some kind, not tied to a catalogue entry

            addMe.UpdateValuesToMatch(column);

            return addMe;
        }

        /// <summary>
        /// Returns the logging server that should be used to audit extraction executions of this <see cref="IExtractionConfiguration"/>.
        /// </summary>
        /// <returns></returns>
        public LogManager GetExplicitLoggingDatabaseServerOrDefault()
        {
            ExternalDatabaseServer loggingServer=null;
            try
            {
                loggingServer = GetDistinctLoggingServer(false);
            }
            catch (Exception e)
            {
                //failed to get a logging server correctly

                //see if there is a default
                var defaultGetter = Project.DataExportRepository.CatalogueRepository.GetServerDefaults();
                var defaultLoggingServer = defaultGetter.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

                //there is a default?
                if (defaultLoggingServer != null)
                    loggingServer = (ExternalDatabaseServer)defaultLoggingServer;
                else
                    //no, there is no default or user does not want to use it.
                    throw new Exception("There is no default logging server configured and there was a problem asking Catalogues for a logging server instead.  Configure a default logging server via ManageExternalServersUI", e);
            }
            
            var server = DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging);

            LogManager lm;

            try
            {
                lm = new LogManager(server);

                if (!lm.ListDataTasks().Contains(ExecuteDatasetExtractionSource.AuditTaskName))
                {
                    throw new Exception("The logging database " + server +
                                        " does not contain a DataLoadTask called '" + ExecuteDatasetExtractionSource.AuditTaskName +
                                        "' (all data exports are logged under this task regardless of dataset/Catalogue)");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Problem figuring out what logging server to use:" + Environment.NewLine + "\t" + e.Message, e);
            }

            return lm;
        }
        
        /// <inheritdoc/>
        public void Unfreeze()
        {
            foreach (IReleaseLog l in ReleaseLog)
                l.DeleteInDatabase();

            foreach (ICumulativeExtractionResults r in CumulativeExtractionResults)
                r.DeleteInDatabase();

            IsReleased = false;
            SaveToDatabase();
        }

        /// <inheritdoc/>
        public IMapsDirectlyToDatabaseTable[] GetGlobals()
        {
            var sds = SelectedDataSets.FirstOrDefault(s=>s.ExtractableDataSet.Catalogue != null);

            if(sds == null)
                return new IMapsDirectlyToDatabaseTable[0];

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
            foreach (var result in Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this))
                result.DeleteInDatabase();

            base.DeleteInDatabase();
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {Project};
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new IHasDependencies[0];
        }
    }
}
