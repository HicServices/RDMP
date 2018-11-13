using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Interface for database object classes with a deprecated status flag
    /// </summary>
    public interface IMightBeDeprecated:IRevertable
    {

        /// <summary>
        /// Bit flag indicating whether the object should be considered Deprecated (i.e. do not use anymore).  This is preferred to deleting it.  The implications
        /// of this are that it no longer appears in UIs by default and that warnings may appear when trying to interact with it.
        /// </summary>
        bool IsDeprecated { get; set; }
    }
}