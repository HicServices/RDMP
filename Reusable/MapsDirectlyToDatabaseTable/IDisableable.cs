using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Class supports a disabled state which should be consulted before running the component
    /// e.g. in a data load.
    /// </summary>
    public interface IDisableable: IRevertable
    {
        /// <summary>
        /// True to skip the component when executing (but still show it at design time).
        /// </summary>
        bool IsDisabled { get; set; }
    }
}