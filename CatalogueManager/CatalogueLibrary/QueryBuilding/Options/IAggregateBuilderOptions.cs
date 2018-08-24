using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.QueryBuilding.Options
{
    /// <summary>
    /// Describes which parts of an <see cref="AggregateBuilder"/> are compatible with a given <see cref="AggregateConfiguration"/> under a given usage case (e.g. acting as an 
    /// aggregate graph).
    /// </summary>
    public interface IAggregateBuilderOptions
    {
        /// <summary>
        /// Human readable description of the role the <see cref="AggregateConfiguration"/> is playing e.g. as a graph, cohort set or patient index table
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        string GetTitleTextPrefix(AggregateConfiguration aggregate);

        /// <summary>
        /// All <see cref="IColumn"/> which could be added as <see cref="AggregateDimension"/> to the <see cref="AggregateConfiguration"/>
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate);

        /// <summary>
        /// All <see cref="IColumn"/> which could be referenced in <see cref="IFilter"/> SQL of the <see cref="AggregateConfiguration"/>
        /// <para>Primarily used for autocomplete etc</para>
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate);

        /// <summary>
        /// Indicates whether a given part of functionality in <see cref="AggregateBuilder"/> is compatible with the <see cref="AggregateConfiguration"/> given 
        /// it's current state and role it is playing e.g. as a graph, cohort set etc
        /// </summary>
        /// <param name="section">The functionality you want to know if is supported</param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate);

        /// <summary>
        /// <see cref="TableInfo"/> which could become <see cref="AggregateForcedJoin"/> and other compatible <see cref="AggregateConfiguration"/> that qualify as Patient Index Tables
        /// (if the <see cref="AggregateConfiguration"/> is acting as a cohort set)/
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate);

        /// <summary>
        /// Gets <see cref="ISqlParameter"/> declared by the <see cref="AggregateConfiguration"/> combined with any that exist 
        /// at a global environment level.
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate);

        /// <summary>
        /// Indicates whether or not there can be a count(*), max(x) etc column given the current configuration and role the <see cref="AggregateConfiguration"/> is playing.
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate);
    }


    /// <summary>
    /// Indicates whether or not a given <see cref="AggregateConfiguration"/> (GROUP BY) can have a count(*), max(x) etc column given the 
    /// current configuration and role it is playing.
    /// </summary>
    public enum CountColumnRequirement
    {
        /// <summary>
        /// <see cref="AggregateConfiguration"/> cannot have a count/max etc column.  
        /// </summary>
        CannotHaveOne,

        /// <summary>
        /// There must be at least one count/max column (and possibly more are allowed)
        /// </summary>
        MustHaveOne,

        /// <summary>
        /// count/max etc columns are optional
        /// </summary>
        CanOptionallyHaveOne
    }

    /// <summary>
    /// Area of functionality supported by <see cref="AggregateBuilder"/> that may or may not be compatible with an <see cref="AggregateConfiguration"/> given
    /// the role it is playing e.g cohort set, graph etc (role is modelled by <see cref="IAggregateBuilderOptions"/>)
    /// </summary>
    public enum AggregateEditorSection
    {
        /// <summary>
        /// The <see cref="AggregateConfiguration"/> is a graph which could be made available to researchers in researcher extracts
        /// </summary>
        Extractable,

        /// <summary>
        /// The <see cref="AggregateConfiguration"/> supports <see cref="IAggregateTopX"/>
        /// </summary>
        TOPX,

        /// <summary>
        /// The <see cref="AggregateConfiguration"/> supports pivoting on a columns values
        /// </summary>
        PIVOT,

        /// <summary>
        /// The <see cref="AggregateConfiguration"/> supports GROUP BY a calendar table ensuring a continuous date/time dimension to records even with sparse
        /// data.  See <see cref="AggregateContinuousDateAxis"/>
        /// </summary>
        AXIS
    }
}
