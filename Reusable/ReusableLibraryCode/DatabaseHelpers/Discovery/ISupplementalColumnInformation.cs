namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Interface for all objects which describe a column e.g. <see cref="DiscoveredColumn"/> and record/request relevant DDL level flags
    /// e.g. <see cref="IsPrimaryKey"/>.
    /// </summary>
    public interface ISupplementalColumnInformation
    {
        /// <summary>
        /// Records whether the column in the underlying database table this record points at is part of the primary key or not.
        /// </summary>
        bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Records whether the column in the underlying database table this record points at is an anto increment identity column
        /// </summary>
        bool IsAutoIncrement { get; set; }

        /// <summary>
        /// Records the collation of the column in the underlying database table this record points at if explicitly declared by dbms (only applicable for char datatypes)
        /// </summary>
        string Collation { get; set; }
    }
}