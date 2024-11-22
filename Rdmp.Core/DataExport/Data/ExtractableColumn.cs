// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
/// Sometimes when extracting data in an ExtractionConfiguration of a Project you don't want to extract all the available (extractable) columns in a dataset.  For example you might
/// have some columns which require 'special approval' to be released and most extracts will not include the columns.  ExtractableColumn is the object which records which columns in
/// a given ExtractionConfiguration are being released to the researcher.  It also allows you to change the implementation of the column, for example a given researcher might want
/// all values UPPERd or he might want the Value field of Prescribing to be passed through his adjustment Scalar Valued Function.
/// 
/// <para>When selecting a column for extraction in ExtractionConfigurationUI an ExtractableColumn will be created with a pointer to the original ExtractionInformation
/// (CatalogueExtractionInformation_ID) in the Catalogue database.  The ExtractionInformations SelectSQL will also be copied out.  The ExtractionQueryBuilder will use these records to
/// assemble the correct SQL for each Catalogue in your ExtractionConfiguration.</para>
/// 
/// <para>The ExtractableColumn 'copy' process allows not only for you to modify the SelectSQL on a 'per extraction' basis but also it means that if you ever delete an ExtractionInformation
/// from the Catalogue or change the implementation then the record in DataExport database still reflects the values that were actually used to execute the extraction.  This means
/// that if you clone a 10 year old extraction you will still get the same SQL (along with lots of warnings about orphan CatalogueExtractionInformation_ID etc).  It even allows you
/// to delete entire datasets (Catalogues) without breaking old extractions (this is not a good idea though - you should always just deprecate the Catalogue instead).</para>
/// </summary>
public class ExtractableColumn : ConcreteColumn, IInjectKnown<CatalogueItem>, IInjectKnown<ColumnInfo>,
    IInjectKnown<ExtractionInformation>
{
    #region Database Properties

    private int _extractableDataSet_ID;
    private int _extractionConfiguration_ID;
    private int? _catalogueExtractionInformation_ID;

    /// <summary>
    /// The dataset to which this column belongs.  This is used with <see cref="ExtractionConfiguration_ID"/> to specify which dataset in which extraction
    /// this line of SELECT sql is used.
    /// </summary>
    public int ExtractableDataSet_ID
    {
        get => _extractableDataSet_ID;
        set => SetField(ref _extractableDataSet_ID, value);
    }

    /// <summary>
    /// The configuration to which this column belongs.  This is used with <see cref="ExtractableDataSet_ID"/> to specify which dataset in which extraction
    /// this line of SELECT sql is used.
    /// </summary>
    public int ExtractionConfiguration_ID
    {
        get => _extractionConfiguration_ID;
        set => SetField(ref _extractionConfiguration_ID, value);
    }

    /// <summary>
    /// The original master column definition this object was cloned from.  When you add a dataset to an <see cref="ExtractionConfiguration"/> all the column
    /// definitions are copied to ensure the configuration is preserved going forwards.  This enables old extractions to be rerun regardless of changes in
    /// the original dataset.
    /// 
    /// <para>May be null if the parent catalogue <see cref="ExtractionInformation"/> has been deleted</para>
    /// </summary>
    public int? CatalogueExtractionInformation_ID
    {
        get => _catalogueExtractionInformation_ID;
        set
        {
            SetField(ref _catalogueExtractionInformation_ID, value);
            ClearAllInjections();
        }
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="CatalogueExtractionInformation_ID"/>
    [NoMappingToDatabase]
    [CanBeNull]
    public ExtractionInformation CatalogueExtractionInformation => _knownExtractionInformation.Value;

    /// <inheritdoc/>
    [CanBeNull]
    [NoMappingToDatabase]
    public override ColumnInfo ColumnInfo => _knownColumnInfo.Value;

    #endregion

    public ExtractableColumn()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Creates a new line of SELECT Sql for the given <paramref name="dataset"/> as it is extracted in the provided <paramref name="configuration"/>.  The new object will
    /// be created in the <paramref name="repository"/> database.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="dataset"></param>
    /// <param name="configuration"></param>
    /// <param name="extractionInformation"></param>
    /// <param name="order"></param>
    /// <param name="selectSQL"></param>
    public ExtractableColumn(IDataExportRepository repository, IExtractableDataSet dataset,
        ExtractionConfiguration configuration, ExtractionInformation extractionInformation, int order, string selectSQL)
    {
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ExtractableDataSet_ID", dataset.ID },
            { "ExtractionConfiguration_ID", configuration.ID },
            {
                "CatalogueExtractionInformation_ID",
                extractionInformation == null ? DBNull.Value : (object)extractionInformation.ID
            },
            { "Order", order },
            { "SelectSQL", string.IsNullOrWhiteSpace(selectSQL) ? DBNull.Value : (object)selectSQL }
        });

        ClearAllInjections();
    }

    internal ExtractableColumn(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
        ExtractionConfiguration_ID = int.Parse(r["ExtractionConfiguration_ID"].ToString());

        if (r["CatalogueExtractionInformation_ID"] == DBNull.Value)
            CatalogueExtractionInformation_ID = null;
        else
            CatalogueExtractionInformation_ID = int.Parse(r["CatalogueExtractionInformation_ID"].ToString());

        SelectSQL = r["SelectSQL"] as string;
        Order = int.Parse(r["Order"].ToString());
        Alias = r["Alias"] as string;
        HashOnDataRelease = (bool)r["HashOnDataRelease"];
        IsExtractionIdentifier = (bool)r["IsExtractionIdentifier"];
        IsPrimaryKey = (bool)r["IsPrimaryKey"];

        ClearAllInjections();
    }

    #region value caching and injection

    private Lazy<CatalogueItem> _knownCatalogueItem;
    private Lazy<ColumnInfo> _knownColumnInfo;
    private Lazy<ExtractionInformation> _knownExtractionInformation;

    /// <inheritdoc/>
    public void InjectKnown(CatalogueItem instance)
    {
        _knownCatalogueItem = new Lazy<CatalogueItem>(instance);
    }

    /// <inheritdoc/>
    public void InjectKnown(ColumnInfo instance)
    {
        _knownColumnInfo = new Lazy<ColumnInfo>(instance);
    }

    /// <inheritdoc/>
    public void InjectKnown(ExtractionInformation extractionInformation)
    {
        if (extractionInformation == null)
        {
            InjectKnown((CatalogueItem)null);
            InjectKnown((ColumnInfo)null);
        }
        else
        {
            InjectKnown(extractionInformation.CatalogueItem);
            InjectKnown(extractionInformation.ColumnInfo);
        }

        _knownExtractionInformation = new Lazy<ExtractionInformation>(extractionInformation);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _knownCatalogueItem = new Lazy<CatalogueItem>(FetchCatalogueItem);
        _knownExtractionInformation = new Lazy<ExtractionInformation>(FetchExtractionInformation);
        _knownColumnInfo = new Lazy<ColumnInfo>(FetchColumnInfo);
    }

    #endregion

    /// <summary>
    /// Returns the <see cref="ConcreteColumn.SelectSQL"/> or <see cref="ConcreteColumn.Alias"/> of the column (if it has one)
    /// </summary>
    /// <returns></returns>
    public override string ToString() => !string.IsNullOrWhiteSpace(Alias) ? Alias : SelectSQL;

    /// <summary>
    /// Returns true if the underlying column (<see cref="Curation.Data.ColumnInfo"/>) referenced by this class has disapeared since its creation.
    /// </summary>
    /// <returns></returns>
    public bool HasOriginalExtractionInformationVanished() => ColumnInfo == null;

    private ColumnInfo FetchColumnInfo()
    {
        var ci = _knownCatalogueItem.Value;
        return ci?.ColumnInfo_ID == null ? null : ci.ColumnInfo;
    }

    private CatalogueItem FetchCatalogueItem()
    {
        var ei = _knownExtractionInformation.Value;

        return ei?.CatalogueItem;
    }

    private ExtractionInformation FetchExtractionInformation()
    {
        //it's not based on a Catalogue column
        if (!CatalogueExtractionInformation_ID.HasValue)
            return null;

        try
        {
            return ((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<ExtractionInformation>(
                CatalogueExtractionInformation_ID.Value);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns true if the current state of the ExtractableColumn is different from the current state of the original <see cref="ExtractionInformation"/> that
    /// it was cloned from.
    /// </summary>
    /// <returns></returns>
    public bool IsOutOfSync()
    {
        var ei = CatalogueExtractionInformation;

        if (ei != null)
            if (ei.IsExtractionIdentifier != IsExtractionIdentifier ||
                IsPrimaryKey != ei.IsPrimaryKey ||
                ei.SelectSQL != SelectSQL ||
                ei.Order != Order ||
                ei.Alias != Alias ||
                ei.HashOnDataRelease != HashOnDataRelease)
                return true;

        return false;
    }

    /// <summary>
    /// Copies all values (SelectSQL, Order, IsPrimaryKey etc from the specified <see cref="IColumn"/>) then saves to database.
    /// </summary>
    /// <param name="item"></param>
    public void UpdateValuesToMatch(IColumn item)
    {
        //Add new things you want to copy from the Catalogue here
        HashOnDataRelease = item.HashOnDataRelease;
        IsExtractionIdentifier = item.IsExtractionIdentifier;
        IsPrimaryKey = item.IsPrimaryKey;
        Order = item.Order;
        Alias = item.Alias;
        SelectSQL = item.SelectSQL;
        SaveToDatabase();
    }

    public ExtractableColumn ShallowClone()
    {
        var eds = DataExportRepository.GetObjectByID<ExtractableDataSet>(ExtractableDataSet_ID);
        var config = DataExportRepository.GetObjectByID<ExtractionConfiguration>(ExtractionConfiguration_ID);

        var clone = new ExtractableColumn(DataExportRepository, eds, config, CatalogueExtractionInformation, Order,
            SelectSQL);
        CopyShallowValuesTo(clone);
        return clone;
    }
}