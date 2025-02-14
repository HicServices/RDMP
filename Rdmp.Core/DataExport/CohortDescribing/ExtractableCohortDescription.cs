// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.DataExport.CohortDescribing;

/// <summary>
/// Summary of all useful information about an ExtractableCohort including the number of unique patients and rowcount (can differ if there are aliases
/// for a patient - 2 private identifiers map to the same release identifier).
/// 
/// <para>Depending on whether you are using an CohortDescriptionDataTableAsyncFetch some properties of this class may start out null/0 and become populated
/// after the CohortDescriptionDataTableAsyncFetch completes.</para>
/// </summary>
public class ExtractableCohortDescription
{
    public readonly ExtractableCohort Cohort;
    public readonly CohortDescriptionDataTableAsyncFetch Fetch;
    public int Count;
    public int CountDistinct;

    public string SourceName;
    public string ReleaseIdentifier;
    public string PrivateIdentifier;

    public int ProjectNumber;
    public int Version;
    public string Description;

    public int OriginID;

    public Exception Exception { get; private set; }

    public DateTime? CreationDate { get; set; }


    /// <summary>
    /// Creates a non async cohort description, this will block until counts are available for the cohort
    /// </summary>
    /// <param name="cohort"></param>
    public ExtractableCohortDescription(ExtractableCohort cohort)
    {
        Cohort = cohort;

        try
        {
            Count = cohort.Count;
        }
        catch (Exception e)
        {
            Exception = e;
            Count = -1;
        }

        try
        {
            CountDistinct = cohort.CountDistinct;
        }
        catch (Exception e)
        {
            CountDistinct = -1;
            Exception = e;
            throw;
        }

        OriginID = cohort.OriginID;


        try
        {
            ReleaseIdentifier = cohort.GetReleaseIdentifier();
        }
        catch (Exception)
        {
            ReleaseIdentifier = "Unknown";
        }

        try
        {
            PrivateIdentifier = cohort.GetPrivateIdentifier();
        }
        catch (Exception)
        {
            PrivateIdentifier = "Unknown";
        }

        var externalData = cohort.GetExternalData();
        SourceName = externalData.ExternalCohortTableName;
        Version = externalData.ExternalVersion;
        CreationDate = externalData.ExternalCohortCreationDate;
        ProjectNumber = externalData.ExternalProjectNumber;
        Description = externalData.ExternalDescription;
    }


    /// <summary>
    /// Creates a new description based on the async fetch request for all cohorts including row counts etc (which might have already completed btw).  If you
    /// use this constructor then the properties will start out with text like "Loading..." but it will perform much faster, when the fetch completes the
    /// values will be populated.  In general if you want to use this feature you should probably use CohortDescriptionFactory and only use it if you are
    /// trying to get all the cohorts at once.
    ///  
    /// </summary>
    /// <param name="cohort"></param>
    /// <param name="fetch"></param>
    public ExtractableCohortDescription(ExtractableCohort cohort, CohortDescriptionDataTableAsyncFetch fetch)
    {
        Cohort = cohort;
        Fetch = fetch;
        OriginID = cohort.OriginID;
        Count = -1;
        CountDistinct = -1;
        SourceName = fetch.Source.Name;

        try
        {
            ReleaseIdentifier = cohort.GetReleaseIdentifier(true);
        }
        catch (Exception)
        {
            ReleaseIdentifier = "Unknown";
        }

        try
        {
            PrivateIdentifier = cohort.GetPrivateIdentifier(true);
        }
        catch (Exception)
        {
            PrivateIdentifier = "Unknown";
        }

        ProjectNumber = -1;
        Version = -1;
        Description = "Loading...";

        //if it's already finished
        if (fetch.Task is { IsCompleted: true })
            FetchOnFinished();
        else
            fetch.Finished += FetchOnFinished;
    }


    private void FetchOnFinished()
    {
        try
        {
            if (Fetch.Task.IsFaulted)
                throw new Exception(
                    $"Fetch cohort data failed for source {Fetch.Source} see inner Exception for details",
                    Fetch.Task.Exception);

            if (Fetch.DataTable == null)
                throw new Exception($"IsFaulted was false but DataTable was not populated for fetch {Fetch.Source}");

            var row = Fetch.DataTable.Rows.Cast<DataRow>()
                .FirstOrDefault(r => Convert.ToInt32(r["OriginID"]) == OriginID) ?? throw new Exception(
                $"No row found for Origin ID {OriginID} in fetched cohort description table for source {Fetch.Source}");


            //it's overridden ugh, got to go the slow way
            if (!string.IsNullOrWhiteSpace(Cohort.OverrideReleaseIdentifierSQL))
            {
                Count = Cohort.Count;
                CountDistinct = Cohort.CountDistinct;
            }
            else
            {
                //it's a proper not overridden release identifier so we can use the DataTable value
                Count = Convert.ToInt32(row["Count"]);
                CountDistinct = Convert.ToInt32(row["CountDistinct"]);
            }

            ProjectNumber = Convert.ToInt32(row["ProjectNumber"]);
            Version = Convert.ToInt32(row["Version"]);
            Description = row["Description"] as string;
            CreationDate = ObjectToNullableDateTime(row["dtCreated"]);
        }
        catch (Exception e)
        {
            Exception = e;
        }
    }


    public override string ToString() => Cohort.ToString();

    private static DateTime? ObjectToNullableDateTime(object o) => o == null || o == DBNull.Value ? null : (DateTime)o;
}