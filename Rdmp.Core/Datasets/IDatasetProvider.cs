using Rdmp.Core.Curation.Data.Datasets;
using System.Collections.Generic;

namespace Rdmp.Core.Datasets;
/// <summary>
/// Base interface for dataset providers to impliment
/// </summary>
public interface IDatasetProvider
{

    /// <summary>
    /// Fetch known datasets
    /// </summary>
    /// <returns></returns>
    List<Dataset> FetchDatasets();


    /// <summary>
    /// Fetch a specific dataset by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Dataset FetchDatasetByID(int id);

}
