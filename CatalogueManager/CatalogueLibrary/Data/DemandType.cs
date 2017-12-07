namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Used to define non Type semantically important things about a [DemandsInitialization] which cannot be determined from the Type of the property.  For example if a
    /// System.String property is expected to contain Sql then this DemandType can be specified which will result in a better user experience than a basic Textbox when it
    /// comes time to provide a value at Design time.
    /// </summary>
    public enum DemandType
    {
        Unspecified,
        SQL
    }
}