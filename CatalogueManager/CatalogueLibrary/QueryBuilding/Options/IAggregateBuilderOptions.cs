using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.QueryBuilding.Options
{
    public interface IAggregateBuilderOptions
    {
        string GetTitleTextPrefix(AggregateConfiguration aggregate);

        //ExtractionInformations which become AggregateDimensions
        IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate);
        IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate);

        bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate);

        //TableInfos which become Forced Joins and Aggregate Configurations that qualify as Patient Index Tables which become JoinableCohortAggregateConfigurationUse when deployed into AggregateEditor
        IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate);

        ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate);
        CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate);
    }

    public enum CountColumnRequirement
    {
        CannotHaveOne,
        MustHaveOne,
        CanOptionallyHaveOne
    }

    public enum AggregateEditorSection
    {
        ExtractableTickBox,
        SELECT,
        TOPX,
        FROM,
        WHERE,
        HAVING,
        PIVOT,
        AXIS
    }
}
