namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Used to define non Type semantically important things about a [DemandsInitialization] which cannot be determined from the Type of the property.  For example if a
    /// System.String property is expected to contain Sql then this DemandType can be specified which will result in a better user experience than a basic Textbox when it
    /// comes time to provide a value at Design time.
    /// </summary>
    public enum DemandType
    {
        /// <summary>
        /// There is no special subcategory
        /// </summary>
        Unspecified,

        /// <summary>
        /// The property is String but it should be rendered/edited the user interface as a SQL syntax (e.g. big editor with highlighting)
        /// </summary>
        SQL
    }
}