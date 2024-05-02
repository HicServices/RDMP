// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Cache;

/// <summary>
///     Describes a failed attempt to contact a caching service including the time it occurred and any associated Exception
///     as well as whether it has been
///     resolved.  Any object of type ICacheFetchRequest (with paired Exception) can be used to create a failure record.
/// </summary>
public class CacheFetchFailure : DatabaseEntity, ICacheFetchFailure
{
    #region Database Properties

    private int _cacheProgressID;
    private DateTime _fetchRequestStart;
    private DateTime _fetchRequestEnd;
    private string _exceptionText;
    private DateTime _lastAttempt;
    private DateTime? _resolvedOn;


    /// <inheritdoc />
    public int CacheProgress_ID
    {
        get => _cacheProgressID;
        set => SetField(ref _cacheProgressID, value);
    }

    /// <inheritdoc />
    public DateTime FetchRequestStart
    {
        get => _fetchRequestStart;
        set => SetField(ref _fetchRequestStart, value);
    }

    /// <inheritdoc cref="ICacheFetchFailure.FetchRequestStart" />
    public DateTime FetchRequestEnd
    {
        get => _fetchRequestEnd;
        set => SetField(ref _fetchRequestEnd, value);
    }

    /// <inheritdoc />
    public string ExceptionText
    {
        get => _exceptionText;
        set => SetField(ref _exceptionText, value);
    }

    /// <inheritdoc />
    public DateTime LastAttempt
    {
        get => _lastAttempt;
        set => SetField(ref _lastAttempt, value);
    }

    /// <inheritdoc />
    public DateTime? ResolvedOn
    {
        get => _resolvedOn;
        set => SetField(ref _resolvedOn, value);
    }

    #endregion

    public CacheFetchFailure()
    {
    }

    /// <summary>
    ///     Documents that a given cache fetch request was not succesfully executed e.g. the remote endpoint returned an error
    ///     for that date range.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="cacheProgress"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="e"></param>
    public CacheFetchFailure(ICatalogueRepository repository, ICacheProgress cacheProgress, DateTime start,
        DateTime end, Exception e)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "CacheProgress_ID", cacheProgress.ID },
            { "FetchRequestStart", start },
            { "FetchRequestEnd", end },
            { "ExceptionText", ExceptionHelper.ExceptionToListOfInnerMessages(e, true) },
            { "LastAttempt", DateTime.Now },
            { "ResolvedOn", DBNull.Value }
        });
    }

    internal CacheFetchFailure(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        CacheProgress_ID = int.Parse(r["CacheProgress_ID"].ToString());
        FetchRequestStart = (DateTime)r["FetchRequestStart"];
        FetchRequestEnd = (DateTime)r["FetchRequestEnd"];
        ExceptionText = r["ExceptionText"].ToString();
        LastAttempt = (DateTime)r["LastAttempt"];
        ResolvedOn = ObjectToNullableDateTime(r["ResolvedOn"]);
    }

    /// <inheritdoc />
    public void Resolve()
    {
        ResolvedOn = DateTime.Now;
        SaveToDatabase();
    }
}