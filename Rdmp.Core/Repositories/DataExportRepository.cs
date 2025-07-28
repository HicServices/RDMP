// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Repositories;

/// <summary>
/// Pointer to the Data Export Repository database in which all export related DatabaseEntities are stored (e.g. ExtractionConfiguration).  Every DatabaseEntity class must exist in a
/// Microsoft Sql Server Database (See DatabaseEntity) and each object is compatible only with a specific type of TableRepository (i.e. the database that contains the
/// table matching their name).
/// 
/// <para>This class allows you to fetch objects and should be passed into constructors of classes you want to construct in the Data Export database.  This includes extraction
/// Projects, ExtractionConfigurations, ExtractableCohorts etc.</para>
/// 
/// <para>Data Export databases are only valid when you have a CatalogueRepository database too and are always paired to a specific CatalogueRepository database (i.e. there are
/// IDs in the data export database that specifically map to objects in the Catalogue database).  You can use the CatalogueRepository property to fetch/create objects
/// in the paired Catalogue database.</para>
/// </summary>
public class DataExportRepository : TableRepository, IDataExportRepository
{
    /// <summary>
    /// The paired Catalogue database which contains non extract metadata (i.e. datasets, aggregates, data loads etc).  Some objects in this database
    /// contain references to objects in the CatalogueRepository.
    /// </summary>
    public ICatalogueRepository CatalogueRepository { get; private set; }

    public IFilterManager FilterManager { get; private set; }

    public IDataExportPropertyManager DataExportPropertyManager { get; private set; }

    private Lazy<Dictionary<int, List<int>>> _packageContentsDictionary;

    public DataExportRepository(DbConnectionStringBuilder connectionString, ICatalogueRepository catalogueRepository) :
        base(null, connectionString)
    {
        CatalogueRepository = catalogueRepository;

        FilterManager = new DataExportFilterManager(this);

        _packageContentsDictionary = new Lazy<Dictionary<int, List<int>>>(GetPackageContentsDictionary);

        DataExportPropertyManager = new DataExportPropertyManager(false, this);

        Constructors.Add(typeof(SupplementalExtractionResults),
            (rep, r) => new SupplementalExtractionResults((IDataExportRepository)rep, r));
        Constructors.Add(typeof(CumulativeExtractionResults),
            (rep, r) => new CumulativeExtractionResults((IDataExportRepository)rep, r));
        Constructors.Add(typeof(DeployedExtractionFilter),
            (rep, r) => new DeployedExtractionFilter((IDataExportRepository)rep, r));
        Constructors.Add(typeof(DeployedExtractionFilterParameter),
            (rep, r) => new DeployedExtractionFilterParameter((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExternalCohortTable),
            (rep, r) => new ExternalCohortTable((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractableCohort), (rep, r) => new ExtractableCohort((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractableColumn), (rep, r) => new ExtractableColumn((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractableDataSetProject), (rep, r) => new ExtractableDataSetProject((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractableDataSet), (rep, r) => new ExtractableDataSet((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractionConfiguration),
            (rep, r) => new ExtractionConfiguration((IDataExportRepository)rep, r));
        Constructors.Add(typeof(FilterContainer), (rep, r) => new FilterContainer((IDataExportRepository)rep, r));
        Constructors.Add(typeof(GlobalExtractionFilterParameter),
            (rep, r) => new GlobalExtractionFilterParameter((IDataExportRepository)rep, r));
        Constructors.Add(typeof(Project), (rep, r) => new Project((IDataExportRepository)rep, r));
        Constructors.Add(typeof(SelectedDataSets), (rep, r) => new SelectedDataSets((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ExtractableDataSetPackage),
            (rep, r) => new ExtractableDataSetPackage((IDataExportRepository)rep, r));
        Constructors.Add(typeof(ProjectCohortIdentificationConfigurationAssociation),
            (rep, r) => new ProjectCohortIdentificationConfigurationAssociation((IDataExportRepository)rep, r));
        Constructors.Add(typeof(SelectedDataSetsForcedJoin),
            (rep, r) => new SelectedDataSetsForcedJoin((IDataExportRepository)rep, r));
    }

    public IEnumerable<ICumulativeExtractionResults> GetAllCumulativeExtractionResultsFor(
        IExtractionConfiguration configuration, IExtractableDataSet dataset) =>
        GetAllObjects<CumulativeExtractionResults>(
            $"WHERE ExtractionConfiguration_ID={configuration.ID}AND ExtractableDataSet_ID={dataset.ID}");

    private readonly ObjectConstructor _constructor = new();

    protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader) =>
        Constructors.TryGetValue(t, out var constructor)
            ? constructor(this, reader)
            : ObjectConstructor.ConstructIMapsDirectlyToDatabaseObject<IDataExportRepository>(t, this, reader);

    public CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c)
    {
        var eds = GetAllObjectsWithParent<ExtractableDataSet>(c).ToList();
        return eds.Count == 0 ? new CatalogueExtractabilityStatus(false, false) : new CatalogueExtractabilityStatus(true, eds.Count > 1 ? true : eds.First().Projects.Any());
    }

    public ISelectedDataSets[] GetSelectedDatasetsWithNoExtractionIdentifiers() =>
        SelectAll<SelectedDataSets>(@"
SELECT ID  FROM SelectedDataSets sds
where not exists (
select 1 FROM ExtractableColumn ec where 
ec.ExtractableDataSet_ID = sds.ExtractableDataSet_ID
AND
ec.IsExtractionIdentifier = 1
AND
ec.ExtractionConfiguration_ID = sds.ExtractionConfiguration_ID
)", "ID").ToArray();

    private readonly Dictionary<Type, IRowVerCache> _caches = new();

    public override T[] GetAllObjects<T>()
    {
        if (!_caches.ContainsKey(typeof(T)))
            _caches.Add(typeof(T), new RowVerCache<T>(this));

        return _caches[typeof(T)].GetAllObjects<T>();
    }

    public override T[] GetAllObjectsNoCache<T>() => base.GetAllObjects<T>();


    /// <inheritdoc/>
    public IExtractableDataSet[] GetAllDataSets(IExtractableDataSetPackage package, IExtractableDataSet[] allDataSets)
    {
        //we know of no children
        return !_packageContentsDictionary.Value.TryGetValue(package.ID, out var contents)
            ? Array.Empty<IExtractableDataSet>()
            : contents.Select(i => allDataSets.Single(ds => ds.ID == i)).ToArray();
    }


    public Dictionary<int, List<int>> GetPackageContentsDictionary()
    {
        var toReturn = new Dictionary<int, List<int>>();

        using var con = GetConnection();
        using var r = DiscoveredServer
            .GetCommand(
                "SELECT * FROM ExtractableDataSetPackage_ExtractableDataSet ORDER BY ExtractableDataSetPackage_ID", con)
            .ExecuteReader();

        var lastPackageId = -1;
        while (r.Read())
        {
            var packageID = Convert.ToInt32(r["ExtractableDataSetPackage_ID"]);
            var dataSetID = Convert.ToInt32(r["ExtractableDataSet_ID"]);

            if (lastPackageId != packageID)
            {
                toReturn.Add(packageID, new List<int>());
                lastPackageId = packageID;
            }

            toReturn[packageID].Add(dataSetID);
        }

        return toReturn;
    }

    /// <summary>
    /// Adds the given <paramref name="dataSet"/> to the <paramref name="package"/> and updates the cached package contents
    /// in memory.
    /// 
    /// <para>This change is immediately written to the database</para>
    ///
    ///  <para>Throws ArgumentException if the <paramref name="dataSet"/> is already part of the package</para>
    /// </summary>
    /// <param name="package"></param>
    /// <param name="dataSet"></param>
    public void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        if (_packageContentsDictionary.Value.TryGetValue(package.ID, out var contents))
        {
            if (contents.Contains(dataSet.ID))
                throw new ArgumentException($"dataSet {dataSet} is already part of package '{package}'",
                    nameof(dataSet));
        }
        else
        {
            _packageContentsDictionary.Value.Add(package.ID, new List<int>());
        }

        using (var con = GetConnection())
        {
            DiscoveredServer.GetCommand(
                $"INSERT INTO ExtractableDataSetPackage_ExtractableDataSet(ExtractableDataSetPackage_ID,ExtractableDataSet_ID) VALUES ({package.ID},{dataSet.ID})",
                con).ExecuteNonQuery();
        }

        _packageContentsDictionary.Value[package.ID].Add(dataSet.ID);
    }


    /// <summary>
    /// Removes the given <paramref name="dataSet"/> from the <paramref name="package"/> and updates the cached package contents
    /// in memory.
    /// 
    /// <para>This change is immediately written to the database</para>
    ///
    ///  <para>Throws ArgumentException if the <paramref name="dataSet"/> is not part of the package</para>
    /// </summary>
    /// <param name="package"></param>
    /// <param name="dataSet"></param>
    public void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        if (!_packageContentsDictionary.Value[package.ID].Contains(dataSet.ID))
            throw new ArgumentException($"dataSet {dataSet} is not part of package {package} so cannot be removed",
                nameof(dataSet));

        using (var con = GetConnection())
        {
            DiscoveredServer.GetCommand(
                $"DELETE FROM ExtractableDataSetPackage_ExtractableDataSet WHERE ExtractableDataSetPackage_ID = {package.ID} AND ExtractableDataSet_ID ={dataSet.ID}",
                con).ExecuteNonQuery();
        }

        _packageContentsDictionary.Value[package.ID].Remove(dataSet.ID);
    }

    public IReleaseLog GetReleaseLogEntryIfAny(CumulativeExtractionResults cumulativeExtractionResults)
    {
        using var con = GetConnection();
        using var cmdselect = DatabaseCommandHelper
            .GetCommand($@"SELECT *
                                    FROM ReleaseLog
                                    where
                                    CumulativeExtractionResults_ID = {cumulativeExtractionResults.ID}", con.Connection,
                con.Transaction);
        using var r = cmdselect.ExecuteReader();
        return r.Read() ? new ReleaseLog(this, r) : null;
    }
}