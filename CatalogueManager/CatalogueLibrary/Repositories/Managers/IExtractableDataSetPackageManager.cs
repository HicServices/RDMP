using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories.Managers
{
    public interface IExtractableDataSetPackageManager
    {
        /// <summary>
        /// Returns the subset of <paramref name="allDataSets"/> which are part of the <paramref name="package"/>.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="allDataSets"></param>
        /// <returns></returns>
        IExtractableDataSet[] GetAllDataSets(IExtractableDataSetPackage package, IExtractableDataSet[] allDataSets);

        /// <summary>
        /// Adds the given <paramref name="dataSet"/> to the <paramref name="package"/>
        /// </summary>
        /// <param name="package"></param>
        /// <param name="dataSet"></param>
        void AddDataSetToPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet);

        /// <summary>
        /// Removes the given <paramref name="dataSet"/> from the <paramref name="package"/> and updates the cached package contents 
        /// in memory.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="dataSet"></param>
        void RemoveDataSetFromPackage(IExtractableDataSetPackage package, IExtractableDataSet dataSet);
    }
}