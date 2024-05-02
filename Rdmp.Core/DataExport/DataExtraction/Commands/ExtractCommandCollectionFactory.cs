// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
///     Identifies all extractable components of a given ExtractionConfiguration (all datasets).  These are returned as an
///     ExtractCommandCollection.
/// </summary>
public class ExtractCommandCollectionFactory
{
    public static ExtractCommandCollection Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ExtractionConfiguration configuration)
    {
        var cohort = configuration.Cohort;
        var datasets = configuration.GetAllExtractableDataSets();

        var datasetBundles = datasets.Select(ds => CreateDatasetCommand(repositoryLocator, ds, configuration));

        return new ExtractCommandCollection(datasetBundles);
    }

    private static ExtractDatasetCommand CreateDatasetCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IExtractableDataSet dataset, IExtractionConfiguration configuration)
    {
        var catalogue = dataset.Catalogue;

        //get all extractable locals AND extractable globals first time then just extractable locals
        var docs = catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableLocals);
        var sqls = catalogue.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableLocals);

        //Now find all the lookups and include them into the bundle
        catalogue.GetTableInfos(out _, out var lookupsFound);

        //bundle consists of:
        var bundle = new ExtractableDatasetBundle(
            dataset, //the dataset
            docs, //all non global extractable docs (SupportingDocuments)
            sqls.Where(sql => sql.IsGlobal == false).ToArray(), //all non global extractable sql (SupportingSQL)
            lookupsFound.ToArray()); //all lookups associated with the Catalogue (the one behind the ExtractableDataset)

        return new ExtractDatasetCommand(configuration, bundle);
    }

    public static ExtractDatasetCommand Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSets)
    {
        return CreateDatasetCommand(repositoryLocator,
            selectedDataSets.ExtractableDataSet, selectedDataSets.ExtractionConfiguration);
    }
}