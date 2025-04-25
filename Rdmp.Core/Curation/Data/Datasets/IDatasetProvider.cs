using System.Collections.Generic;

namespace Rdmp.Core.Curation.Data.Datasets;
/// <summary>
/// Base interface for dataset providers to impliment
/// </summary>
public interface IDatasetProvider
{

    /// <summary>
    /// Fetch known datasets
    /// </summary>
    /// <returns></returns>
    List<Curation.Data.Datasets.Dataset> FetchDatasets();


    /// <summary>
    /// Fetch a specific dataset by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Curation.Data.Datasets.Dataset FetchDatasetByID(int id);

}