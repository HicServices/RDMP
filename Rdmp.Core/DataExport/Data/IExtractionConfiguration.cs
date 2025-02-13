// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
/// Represents a collection of datasets (Catalogues), ExtractableColumns, ExtractionFilters etc and a single ExtractableCohort for a Project.  You can have multiple active
/// ExtractionConfigurations at a time for example a Project might have two cohorts 'Cases' and 'Controls' and you would have two ExtractionConfiguration possibly containing
/// the same datasets and filters but with different cohorts.
/// 
/// <para>Once you have executed, extracted and released an ExtractionConfiguration then it becomes 'frozen' IsReleased and it is not possible to edit it.  This is intended
/// to ensure that once data has gone out the door the configuration that generated the data is immutable.</para>
/// 
/// <para>If you need to perform a repeat extraction (e.g. an update of data 5 years on) then you should 'Clone' the ExtractionConfiguration in the Project and give it a new name
/// e.g. 'Cases - 5 year update'.</para>
/// </summary>
public interface IExtractionConfiguration : INamed, IHasDependencies, IMightBeReadOnly, ILoggedActivityRootObject
{
    IDataExportRepository DataExportRepository { get; }

    /// <summary>
    /// When the configuration was created
    /// </summary>
    DateTime? dtCreated { get; set; }

    /// <summary>
    /// The cohort that will be linked with datasets in the configuration to produce anonymous data extracts.
    /// </summary>
    int? Cohort_ID { get; set; }

    /// <summary>
    /// Optional - Ticket in an <see cref="ITicketingSystem"/> (e.g. JIRA) which can be used to record project documents, requirements, governance etc
    /// </summary>
    string RequestTicket { get; set; }

    /// <summary>
    /// Optional - Ticket in an <see cref="ITicketingSystem"/> (e.g. JIRA) which can be used to record project documents, requirements, governance etc
    /// </summary>
    string ReleaseTicket { get; set; }

    /// <summary>
    /// The <see cref="IProject"/> to which this configuration belongs.  A project can have multiple configurations (e.g. "Cases" and "Controls").  You can also
    /// have multiple configurations over time in the project (e.g. quarterly data refreshes).
    /// </summary>
    int Project_ID { get; }

    /// <inheritdoc cref="Project_ID"/>
    IProject Project { get; }

    /// <summary>
    /// The username of the data analyst who originally created this configuration
    /// </summary>
    string Username { get; }

    /// <summary>
    /// The file separator that should be used when extracting this dataset to a delimited flat file destination.  Ignore if extracting to database, excel etc
    /// </summary>
    string Separator { get; set; }

    /// <summary>
    /// User provided description of what the extraction represents, any filters etc
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// True if the configuration has been successfully extracted and the artifacts have been released to the end researchers.  When true the configuration
    /// should be treated as immutable.  If the user wants to make further changes the recommended course of action is to clone a new copy of it.
    /// </summary>
    bool IsReleased { get; set; }

    /// <summary>
    /// If the <see cref="IExtractionConfiguration"/> was cloned from another configuration this will store the ID of the original
    /// </summary>
    int? ClonedFrom_ID { get; set; }


    /// <inheritdoc cref="Cohort_ID"/>
    IExtractableCohort Cohort { get; }

    /// <summary>
    /// The <see cref="IPipeline"/> which should be used to extract the linked datasets unless the user specifies a specific alternative.
    /// </summary>
    int? DefaultPipeline_ID { get; set; }

    /// <summary>
    /// If the <see cref="IExtractableCohort"/> for this configuration was created by an RDMP cohort query (<see cref="CohortIdentificationConfiguration"/>)
    /// then this ID can be set to provide a link back.
    /// 
    /// <para>This field is also used for 'Cohort Refresh' in which a cohort list is updated to match the new state of the query/live clinical database</para>
    /// </summary>
    int? CohortIdentificationConfiguration_ID { get; set; }

    /// <summary>
    /// The <see cref="IPipeline"/> which should be executed during 'Cohort Refresh' in order to turn the query
    /// (<see cref="CohortIdentificationConfiguration_ID"/>) into an <see cref="IExtractableCohort"/>.
    /// </summary>
    int? CohortRefreshPipeline_ID { get; set; }


    /// <inheritdoc cref="Cohort_ID"/>
    IExtractableCohort GetExtractableCohort();

    /// <inheritdoc cref="Project_ID"/>
    IProject GetProject();

    /// <summary>
    /// Returns all global <see cref="ISqlParameter"/> which can be used in WHERE filters of <see cref="ISelectedDataSets"/> being extracted in this
    /// <see cref="IExtractionConfiguration"/>.  This helps avoid replication of values e.g. record extraction window start/end.
    /// </summary>
    ISqlParameter[] GlobalExtractionFilterParameters { get; }


    /// <summary>
    /// If the extracted artifacts for this configuration have been released (See <see cref="IsReleased"/>) then this will return all the
    /// audit objects describing that process.
    /// </summary>
    IReleaseLog[] ReleaseLog { get; }

    /// <summary>
    /// If the user (or an automated system) has attempted to extract any datasets in this <see cref="IExtractionConfiguration"/> then this will
    /// return all the audit objects describing that extraction.
    /// </summary>
    IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults { get; }

    /// <summary>
    /// If the user (or an automated system) has attempted to extract any datasets with supplemental artifacts (e.g. <see cref="SupportingDocument"/>)
    /// then this will return all the audit objects describing that extraction.
    /// </summary>
    IEnumerable<ISupplementalExtractionResults> SupplementalExtractionResults { get; }

    /// <summary>
    /// Returns all the columns that have been selected for linkage and extraction in the given <paramref name="dataset"/> as it exists in this <see cref="IExtractionConfiguration"/>
    /// </summary>
    /// <param name="dataset"></param>
    /// <returns></returns>
    ExtractableColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset);

    /// <summary>
    /// Returns the root AND/OR container (if any) that provides WHERE logic to restrict which records are extracted
    ///  in the  given <paramref name="dataset"/> as it exists in this <see cref="IExtractionConfiguration"/>.
    /// 
    /// <para>This does not include the cohort linkage logic (join) which further reduces records extracted</para>
    /// </summary>
    /// <param name="dataset"></param>
    /// <returns></returns>
    IContainer GetFilterContainerFor(IExtractableDataSet dataset);


    /// <summary>
    /// Returns all <see cref="IExtractableDataSet"/> which have been selected for linkage and extraction in this <see cref="IExtractionConfiguration"/>
    /// </summary>
    /// <returns></returns>
    IExtractableDataSet[] GetAllExtractableDataSets();

    /// <summary>
    /// Returns all <see cref="ISelectedDataSets"/> which have been selected for linkage and extraction in this <see cref="IExtractionConfiguration"/>
    /// 
    /// <para><see cref="ISelectedDataSets"/> is the link object between the dataset and the configuration</para>
    /// </summary>
    /// <returns></returns>
    ISelectedDataSets[] SelectedDataSets { get; }

    /// <summary>
    /// Deletes the <see cref="ISelectedDataSets"/> which records the fact that the given <paramref name="extractableDataSet"/> should be extracted in
    /// this <see cref="IExtractionConfiguration"/> (removes the dataset from the extraction).
    /// </summary>
    /// <param name="extractableDataSet"></param>
    void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet);

    /// <summary>
    /// Clears the <see cref="IsReleased"/> status of the <see cref="IExtractionConfiguration"/> and deletes any audit objects that would prevent
    /// re-extraction.  This is not the recommended way of changing a released configuration, instead it is advised that you clone the configuration
    /// to preserve the extraction history.
    /// </summary>
    void Unfreeze();

    /// <summary>
    /// Returns all supplemental artifacts (e.g. <see cref="SupportingDocument"/>) marked IsGlobal which should be extracted whenever the <see cref="IExtractionConfiguration"/>
    /// is run.  This can include disclaimers, general purpose help materials etc.
    /// </summary>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable[] GetGlobals();

    /// <summary>
    /// Returns true if the configuration looks like it is ready for execution (has a cohort, some datasets and isn't already <see cref="IsReleased"/>)
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    bool IsExtractable(out string reason);
}