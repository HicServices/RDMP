using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories.Managers
{
    /// <summary>
    /// List of all Keys that can be stored in the <see cref="IDataExportPropertyManager"/> table of the data export database
    /// </summary>
    public enum DataExportProperty
    {
        /// <summary>
        /// What to do in order to produce a 'Hash' when a column is marked <see cref="ConcreteColumn.HashOnDataRelease"/>
        /// </summary>
        HashingAlgorithmPattern,

        /// <summary>
        /// What text to write into the release document when releasing datasets
        /// </summary>
        ReleaseDocumentDisclaimer
    }
}