// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <inheritdoc cref="ILoadProgress" />
public class LoadProgress : DatabaseEntity, ILoadProgress, ICheckable
{
    #region Database Properties

    private bool _isDisabled;
    private string _name;
    private DateTime? _originDate;
    private string _loadPeriodicity;
    private DateTime? _dataLoadProgress;
    private int _loadMetadata_ID;
    private int _defaultNumberOfDaysToLoadEachTime;

    /// <inheritdoc />
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }

    /// <inheritdoc />
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc />
    public DateTime? OriginDate
    {
        get => _originDate;
        set => SetField(ref _originDate, value);
    }

    /// <summary>
    ///     Not used
    /// </summary>
    [Obsolete("Do not use")]
    public string LoadPeriodicity
    {
        get => _loadPeriodicity;
        set => SetField(ref _loadPeriodicity, value);
    }

    /// <inheritdoc />
    public DateTime? DataLoadProgress
    {
        get => _dataLoadProgress;
        set => SetField(ref _dataLoadProgress, value);
    }

    /// <inheritdoc />
    public int LoadMetadata_ID
    {
        get => _loadMetadata_ID;
        set => SetField(ref _loadMetadata_ID, value);
    }

    /// <inheritdoc />
    public int DefaultNumberOfDaysToLoadEachTime
    {
        get => _defaultNumberOfDaysToLoadEachTime;
        set => SetField(ref _defaultNumberOfDaysToLoadEachTime, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public ILoadMetadata LoadMetadata => Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID);

    /// <inheritdoc />
    [NoMappingToDatabase]
    public ICacheProgress CacheProgress => Repository.GetAllObjectsWithParent<CacheProgress>(this).SingleOrDefault();

    #endregion

    public LoadProgress()
    {
    }

    /// <inheritdoc cref="ILoadProgress" />
    public LoadProgress(ICatalogueRepository repository, LoadMetadata parent)
    {
        repository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "Name", Guid.NewGuid().ToString() },
                { "LoadMetadata_ID", parent.ID }
            });
    }

    internal LoadProgress(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"] as string;
        OriginDate = ObjectToNullableDateTime(r["OriginDate"]);
        DataLoadProgress = ObjectToNullableDateTime(r["DataLoadProgress"]);
        LoadMetadata_ID = int.Parse(r["LoadMetaData_ID"].ToString());
        _loadPeriodicity = r["LoadPeriodicity"].ToString();
        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
        DefaultNumberOfDaysToLoadEachTime = Convert.ToInt32(r["DefaultNumberOfDaysToLoadEachTime"]);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} ID={ID}";
    }

    public void Check(ICheckNotifier notifier)
    {
        if (OriginDate != null && OriginDate > DateTime.Now)
            notifier.OnCheckPerformed(new CheckEventArgs($"OriginDate cannot be in the future ({Name})",
                CheckResult.Fail));

        if (DataLoadProgress != null && DataLoadProgress > DateTime.Now)
            notifier.OnCheckPerformed(new CheckEventArgs($"DataLoadProgress cannot be in the future ({Name})",
                CheckResult.Fail));
    }
}