// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Repositories;

/// <summary>
/// Memory only implementation of <see cref="IDataExportRepository"/>.  Also implements <see cref="ICatalogueRepository"/>.  All objects are created in
/// dictionaries and arrays in memory instead of the database.
/// </summary>
public class MemoryDataExportRepository : MemoryCatalogueRepository, IDataExportRepository, IDataExportPropertyManager,
    IExtractableDataSetPackageManager
{
    public ICatalogueRepository CatalogueRepository => this;
    public IDataExportPropertyManager DataExportPropertyManager => this;
    public IExtractableDataSetPackageManager PackageManager => this;


    public CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c)
    {
        var eds = GetAllObjectsWithParent<ExtractableDataSet>(c).SingleOrDefault();

        return eds == null
            ? new CatalogueExtractabilityStatus(false, false)
            : new CatalogueExtractabilityStatus(true, !eds.Projects.Any());
    }

    public ISelectedDataSets[] GetSelectedDatasetsWithNoExtractionIdentifiers()
    {
        var col = GetAllObjects<ExtractableColumn>().Where(ec => ec.IsExtractionIdentifier).ToArray();

        return GetAllObjects<ISelectedDataSets>()
            .Where(sds => !col.Any(c => c.ExtractableDataSet_ID == sds.ExtractableDataSet_ID
                                        && c.ExtractionConfiguration_ID == sds.ExtractionConfiguration_ID)).ToArray();
    }


    #region IDataExportPropertyManager

    protected Dictionary<DataExportProperty, string> PropertiesDictionary = new();

    public virtual string GetValue(DataExportProperty property) =>
        PropertiesDictionary.GetValueOrDefault(property);

    public virtual void SetValue(DataExportProperty property, string value)
    {
        PropertiesDictionary[property] = value;
    }

    #endregion


    #region IExtractableDataSetPackageManager

    protected Dictionary<IExtractableDataSetPackage, HashSet<IExtractableDataSet>> PackageDictionary { get; set; } =
        new();

    public IExtractableDataSet[] GetAllDataSets(IExtractableDataSetPackage package, IExtractableDataSet[] allDataSets)
    {
        if (!PackageDictionary.ContainsKey(package))
            PackageDictionary.Add(package, new HashSet<IExtractableDataSet>());

        return PackageDictionary[package].ToArray();
    }

    public virtual void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        if (!PackageDictionary.ContainsKey(package))
            PackageDictionary.Add(package, new HashSet<IExtractableDataSet>());

        PackageDictionary[package].Add(dataSet);
    }

    public virtual void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet)
    {
        if (!PackageDictionary.ContainsKey(package))
            PackageDictionary.Add(package, new HashSet<IExtractableDataSet>());

        if (!PackageDictionary[package].Contains(dataSet))
            throw new ArgumentException($"dataSet {dataSet} is not part of package {package} so cannot be removed",
                nameof(dataSet));

        PackageDictionary[package].Remove(dataSet);
    }

    public Dictionary<int, List<int>> GetPackageContentsDictionary()
    {
        return PackageDictionary.ToDictionary(k => k.Key.ID, v => v.Value.Select(o => o.ID).ToList());
    }

    public IEnumerable<ICumulativeExtractionResults> GetAllCumulativeExtractionResultsFor(
        IExtractionConfiguration configuration, IExtractableDataSet dataset)
    {
        return GetAllObjects<CumulativeExtractionResults>().Where(e =>
            e.ExtractionConfiguration_ID == configuration.ID && e.ExtractableDataSet_ID == dataset.ID);
    }

    public IReleaseLog GetReleaseLogEntryIfAny(CumulativeExtractionResults cumulativeExtractionResults) =>
        GetAllObjectsWhere<ReleaseLog>("CumulativeExtractionResults_ID", cumulativeExtractionResults.ID)
            .SingleOrDefault();

    #endregion
}