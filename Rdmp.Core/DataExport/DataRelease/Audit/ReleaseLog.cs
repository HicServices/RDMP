// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.DataExport.DataRelease.Audit;

/// <summary>
///     Records the fact that a given extracted dataset has been released.  It audits the user performing the release, the
///     environmental release potential,
///     destination directory etc.
///     <para>
///         This is done by linking the CumulativeExtractionResult with a record in the ReleaseLog.  Each SelectedDataSet
///         in an ExtractionConfiguration
///         can only have 1 CumulativeExtractionResult at a time (it is a record of the last extracted SQL etc - See
///         CumulativeExtractionResult) and there can be
///         only 1 ReleaseLog entry per CumulativeExtractionResult.  This means that once a dataset has been released it
///         cannot be extracted/released again (this
///         is intended behaviour).  If you want to re run a released ExtractionConfiguration then you should clone it.
///     </para>
/// </summary>
public class ReleaseLog : DatabaseEntity, IReleaseLog
{
    public int CumulativeExtractionResults_ID
    {
        get => _cumulativeExtractionResultsID;
        set => SetField(ref _cumulativeExtractionResultsID, value);
    }

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public DateTime DateOfRelease
    {
        get => _dateOfRelease;
        set => SetField(ref _dateOfRelease, value);
    }

    public string MD5OfDatasetFile
    {
        get => _md5OfDatasetFile;
        set => SetField(ref _md5OfDatasetFile, value);
    }

    public string DatasetState
    {
        get => _datasetState;
        set => SetField(ref _datasetState, value);
    }

    public string EnvironmentState
    {
        get => _environmentState;
        set => SetField(ref _environmentState, value);
    }

    public bool IsPatch
    {
        get => _isPatch;
        set => SetField(ref _isPatch, value);
    }

    public string ReleaseFolder
    {
        get => _releaseFolder;
        set => SetField(ref _releaseFolder, value);
    }


    private int _cumulativeExtractionResultsID;
    private string _username;
    private DateTime _dateOfRelease;
    private string _md5OfDatasetFile;
    private string _datasetState;
    private string _environmentState;
    private bool _isPatch;
    private string _releaseFolder;

    private string _datasetName;

    public override string ToString()
    {
        if (_datasetName == null)
            try
            {
                ICumulativeExtractionResults cumulativeExtractionResults =
                    Repository.GetObjectByID<CumulativeExtractionResults>(CumulativeExtractionResults_ID);
                IExtractableDataSet ds =
                    Repository.GetObjectByID<ExtractableDataSet>(cumulativeExtractionResults.ExtractableDataSet_ID);
                _datasetName = ds.ToString();
            }
            catch (Exception e)
            {
                _datasetName = e.Message;
            }


        return
            $"ReleaseLogEntry(CumulativeExtractionResults_ID={CumulativeExtractionResults_ID},DatasetName={_datasetName},DateOfRelease={DateOfRelease},Username={Username})";
    }

    public ReleaseLog()
    {
    }

    public ReleaseLog(IDataExportRepository repository, ReleasePotential dataset,
        ReleaseEnvironmentPotential environment, bool isPatch, DirectoryInfo releaseDirectory,
        FileInfo datasetFileBeingReleased)
    {
        repository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "CumulativeExtractionResults_ID", dataset.DatasetExtractionResult.ID },
                { "Username", Environment.UserName },
                { "DateOfRelease", DateTime.Now },
                {
                    "MD5OfDatasetFile",
                    datasetFileBeingReleased == null ? "X" : UsefulStuff.HashFile(datasetFileBeingReleased.FullName)
                },
                { "DatasetState", dataset.DatasetExtractionResult.ToString() },
                { "EnvironmentState", environment.Assesment.ToString() },
                { "IsPatch", isPatch },
                { "ReleaseFolder", releaseDirectory.FullName }
            });
    }

    public ReleaseLog(IDataExportRepository repository, DbDataReader r) : base(repository, r)
    {
        CumulativeExtractionResults_ID = Convert.ToInt32(r["CumulativeExtractionResults_ID"]);
        Username = r["Username"].ToString();
        MD5OfDatasetFile = r["MD5OfDatasetFile"].ToString();
        DatasetState = r["DatasetState"].ToString();
        EnvironmentState = r["EnvironmentState"].ToString();
        DateOfRelease = Convert.ToDateTime(r["DateOfRelease"]);
        IsPatch = Convert.ToBoolean(r["IsPatch"]);
        ReleaseFolder = r["ReleaseFolder"].ToString();
    }
}