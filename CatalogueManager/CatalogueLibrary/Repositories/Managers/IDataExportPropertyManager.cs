namespace CatalogueLibrary.Repositories.Managers
{
    public interface IDataExportPropertyManager
    {
        /// <inheritdoc cref="DataExportPropertyManager.GetValue(string)"/>
        string GetValue(DataExportProperty property);

        /// <summary>
        /// Stores a new <paramref name="value"/> for the given <see cref="property"/> (and saves to the database)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void SetValue(DataExportProperty property, string value);
    }
}