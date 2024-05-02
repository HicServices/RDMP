// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
///     Determines whether a given ExtractableDataSet in an ExtractionConfiguration is ready for Release.
///     Extraction Destinations will return an implementation of this class which will run checks on the releasaility of
///     the extracted datasets
///     based on the extraction method used.
/// </summary>
public abstract class ReleasePotential : ICheckable
{
    protected readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
    private readonly IRepository _repository;
    private List<IColumn> _columnsToExtract;

    public ISelectedDataSets SelectedDataSet { get; }
    public IExtractionConfiguration Configuration { get; }
    public IExtractableDataSet DataSet { get; }

    public Dictionary<ExtractableColumn, ExtractionInformation> ColumnsThatAreDifferentFromCatalogue
    {
        get;
        private set;
    }

    public Exception Exception { get; }
    public ICumulativeExtractionResults DatasetExtractionResult { get; protected set; }
    public DateTime DateOfExtraction { get; private set; }

    /// <summary>
    ///     The SQL that was run when the extraction was last performed (or null if no extraction has ever been performed)
    /// </summary>
    public string SqlExtracted { get; private set; }

    /// <summary>
    ///     The SQL that would be generated if the configuration/dataset were executed today (if this differes from
    ///     SqlExtracted then there is an Sql Desynchronisation)
    /// </summary>
    public string SqlCurrentConfiguration { get; private set; }

    /// <summary>
    ///     The directory that the extraction configuration last extracted data to (for this dataset).  This may no longer
    ///     exist if people have been monkeying with the filesystem so check .Exists().  If no extraction has ever been made
    ///     this will be NULL
    /// </summary>
    public DirectoryInfo ExtractDirectory { get; protected set; }

    /// <summary>
    ///     The file that contains the dataset data e.g. biochemistry.csv (will be null if no extract files were found)
    /// </summary>
    public FileInfo ExtractFile { get; set; }

    public Dictionary<IExtractionResults, Releaseability> Assessments { get; protected set; }

    protected ReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSet)
    {
        _repositoryLocator = repositoryLocator;
        _repository = selectedDataSet.Repository;
        SelectedDataSet = selectedDataSet;
        Configuration = selectedDataSet.ExtractionConfiguration;
        DataSet = selectedDataSet.ExtractableDataSet;
        Assessments = new Dictionary<IExtractionResults, Releaseability>();

        //see what has been extracted before
        DatasetExtractionResult =
            Configuration.CumulativeExtractionResults.FirstOrDefault(r => r.ExtractableDataSet_ID == DataSet.ID);
    }

    private Releaseability MakeAssessment(ICumulativeExtractionResults extractionResults)
    {
        //always try to figure out what the current SQL is
        SqlCurrentConfiguration = GetCurrentConfigurationSQL();

        //let the user know when the data was extracted
        DateOfExtraction = extractionResults.DateOfExtraction;
        SqlExtracted = extractionResults.SQLExecuted;

        //the cohort has been changed in the configuration in the time elapsed since the file we are evaluating was generated (that was when CummatliveExtractionResults was populated)
        if (extractionResults.CohortExtracted != Configuration.Cohort_ID)
            return Releaseability.CohortDesynchronisation;

        if (SqlOutOfSyncWithDataExportManagerConfiguration(extractionResults))
            return Releaseability.ExtractionSQLDesynchronisation;

        if (ExtractionProgressIsIncomplete(null))
            return Releaseability.NeverBeenSuccessfullyExecuted;

        var finalAssessment = GetSpecificAssessment(extractionResults);

        return finalAssessment == Releaseability.Undefined
            ? SqlDifferencesVsLiveCatalogue()
                ? Releaseability.ColumnDifferencesVsCatalogue
                : Releaseability.Releaseable
            : finalAssessment;
    }

    private bool ExtractionProgressIsIncomplete(ICheckNotifier notifier)
    {
        var progress = SelectedDataSet.ExtractionProgressIfAny;

        if (progress == null) return false;

        if (progress.ProgressDate == null)
        {
            notifier?.OnCheckPerformed(new CheckEventArgs(
                $"ExtractionProgress ProgressDate is null for '{SelectedDataSet}'.", CheckResult.Warning));
            return true;
        }

        if (progress.ProgressDate < progress.EndDate)
        {
            notifier?.OnCheckPerformed(new CheckEventArgs(
                $"ExtractionProgress is incomplete for '{SelectedDataSet}'.  ProgressDate is {progress.ProgressDate} but EndDate is {progress.EndDate}",
                CheckResult.Warning));
            return true;
        }

        return false;
    }

    private Releaseability MakeSupplementalAssesment(ISupplementalExtractionResults supplementalExtractionResults)
    {
        if (_repositoryLocator.GetArbitraryDatabaseObject(
                supplementalExtractionResults.ReferencedObjectRepositoryType,
                supplementalExtractionResults.ReferencedObjectType,
                supplementalExtractionResults.ReferencedObjectID) is not INamed extractedObject)
            return Releaseability.Undefined;

        if (extractedObject is SupportingSQLTable table && table.SQL != supplementalExtractionResults.SQLExecuted)
            return Releaseability.ExtractionSQLDesynchronisation;

        var finalAssessment = GetSupplementalSpecificAssessment(supplementalExtractionResults);

        return finalAssessment == Releaseability.Undefined
            ? extractedObject.Name != supplementalExtractionResults.ExtractedName
                ? Releaseability.ExtractionSQLDesynchronisation
                : Releaseability.Releaseable
            : finalAssessment;
    }

    protected abstract Releaseability GetSupplementalSpecificAssessment(
        IExtractionResults supplementalExtractionResults);

    protected abstract Releaseability GetSpecificAssessment(IExtractionResults extractionResults);

    private bool SqlDifferencesVsLiveCatalogue()
    {
        ColumnsThatAreDifferentFromCatalogue = new Dictionary<ExtractableColumn, ExtractionInformation>();

        foreach (var column in _columnsToExtract)
        {
            var extractableColumn = (ExtractableColumn)column;
            if (extractableColumn.HasOriginalExtractionInformationVanished())
            {
                ColumnsThatAreDifferentFromCatalogue.Add(extractableColumn, null);
                continue;
            }

            var masterCatalogueVersion = extractableColumn.CatalogueExtractionInformation;

            if (!masterCatalogueVersion.SelectSQL.Equals(extractableColumn.SelectSQL))
                ColumnsThatAreDifferentFromCatalogue.Add(extractableColumn, masterCatalogueVersion);
        }

        return ColumnsThatAreDifferentFromCatalogue.Any();
    }

    private string GetCurrentConfigurationSQL()
    {
        //get the cohort
        var cohort = _repository.GetObjectByID<ExtractableCohort>((int)Configuration.Cohort_ID);

        //get the columns that are configured today
        _columnsToExtract = new List<IColumn>(Configuration.GetAllExtractableColumnsFor(DataSet));
        _columnsToExtract.Sort();

        //get the salt
        var project = _repository.GetObjectByID<Project>(Configuration.Project_ID);
        var salt = new HICProjectSalt(project);

        //create a request for an empty bundle - only the dataset
        var request = new ExtractDatasetCommand(Configuration, cohort, new ExtractableDatasetBundle(DataSet),
            _columnsToExtract, salt, null);

        request.GenerateQueryBuilder();

        //Generated the SQL as it would exist today for this extraction
        var resultLive = request.QueryBuilder;

        return resultLive.SQL;
    }

    private bool SqlOutOfSyncWithDataExportManagerConfiguration(IExtractionResults extractionResults)
    {
        if (extractionResults.SQLExecuted == null)
            throw new Exception(
                "Cumulative Extraction Results for the extraction in which this dataset was involved in does not have any SQLExecuted recorded for it.");

        // When using extraction progress the SQL can be whatever you want
        // if the progress date says End then we blindly assume that whatever you
        // executed was legit
        if (SelectedDataSet.ExtractionProgressIfAny != null) return false;

        //if the SQL today is different to the SQL that was run when the user last extracted the data then there is a desync in the SQL (someone has changed something in the catalogue/data export manager configuration since the data was extracted)
        return !SqlCurrentConfiguration.Equals(extractionResults.SQLExecuted);
    }

    public override string ToString()
    {
        return DatasetExtractionResult?.DestinationDescription == null
            ? "Never extracted..."
            : Assessments[DatasetExtractionResult] switch
            {
                Releaseability.ExceptionOccurredWhileEvaluatingReleaseability => Exception.ToString(),
                _ =>
                    $"Dataset: {DataSet} DateOfExtraction: {DateOfExtraction} Status: {Assessments[DatasetExtractionResult]}"
            };
    }

    public virtual void Check(ICheckNotifier notifier)
    {
        if (DatasetExtractionResult?.DestinationDescription == null)
            return;

        // check if we have a halfway completed extraction
        ExtractionProgressIsIncomplete(notifier);

        if (DatasetExtractionResult.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Release potential relates to expired (stale) extraction; you or someone else has executed another data extraction since you added this dataset to the release.Offending dataset was ({DataSet}).  You can probably fix this problem by reloading/refreshing the Releaseability window. If you have already added them to a planned Release you will need to add the newly recalculated one instead.",
                CheckResult.Fail));

        var existingReleaseLog = DatasetExtractionResult.GetReleaseLogEntryIfAny();
        if (existingReleaseLog != null)
            if (notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Dataset {DataSet} has probably already been released as per {existingReleaseLog}!",
                    CheckResult.Warning,
                    null,
                    "Do you want to delete the old release Log? You should check the values first.")))
                existingReleaseLog.DeleteInDatabase();

        var cols = Configuration.GetAllExtractableColumnsFor(DataSet)
            .OfType<ExtractableColumn>()
            .Select(c => c.CatalogueExtractionInformation)
            .ToArray();

        SelectedDataSetsChecker.WarnAboutExtractionCategory(notifier, Configuration, DataSet, cols,
            ErrorCodes.ExtractionContainsSpecialApprovalRequired, ExtractionCategory.SpecialApprovalRequired);
        SelectedDataSetsChecker.WarnAboutExtractionCategory(notifier, Configuration, DataSet, cols,
            ErrorCodes.ExtractionContainsInternal, ExtractionCategory.Internal);
        SelectedDataSetsChecker.WarnAboutExtractionCategory(notifier, Configuration, DataSet, cols,
            ErrorCodes.ExtractionContainsDeprecated, ExtractionCategory.Deprecated);

        if (!Assessments.ContainsKey(DatasetExtractionResult))
            try
            {
                Assessments.Add(DatasetExtractionResult, MakeAssessment(DatasetExtractionResult));
            }
            catch (Exception e)
            {
                Assessments.Add(DatasetExtractionResult, Releaseability.ExceptionOccurredWhileEvaluatingReleaseability);
                notifier.OnCheckPerformed(new CheckEventArgs($"FAILURE: {e.Message}", CheckResult.Fail, e));
            }

        foreach (var supplementalResult in DatasetExtractionResult.SupplementalExtractionResults)
            if (!Assessments.ContainsKey(supplementalResult))
                try
                {
                    Assessments.Add(supplementalResult, MakeSupplementalAssesment(supplementalResult));
                }
                catch (Exception e)
                {
                    Assessments.Add(supplementalResult, Releaseability.ExceptionOccurredWhileEvaluatingReleaseability);
                    notifier.OnCheckPerformed(new CheckEventArgs($"FAILURE: {e.Message}", CheckResult.Fail, e));
                }

        foreach (var kvp in Assessments)
        {
            var checkResult = kvp.Value switch
            {
                Releaseability.ColumnDifferencesVsCatalogue => CheckResult.Warning,
                Releaseability.Releaseable => CheckResult.Success,
                _ => CheckResult.Fail
            };

            notifier.OnCheckPerformed(new CheckEventArgs($"{kvp.Key} is {kvp.Value}", checkResult));
        }
    }
}