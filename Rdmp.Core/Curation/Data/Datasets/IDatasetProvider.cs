using System.Collections.Generic;

namespace Rdmp.Core.Curation.Data.Datasets;
/// <summary>
/// Base interface for dataset providers to impliment
/// </summary>
public interface IDatasetProvider
{

    /// <summary>
    /// Fetch a specific dataset by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Dataset FetchDatasetByID(int id);
    void UpdateUsingCatalogue(Dataset dataset, Catalogue catalogue);
    Dataset Create(Catalogue catalogue);

    Dataset AddExistingDatasetWithReturn(string name, string url);
    void AddExistingDataset(string name, string url);
}