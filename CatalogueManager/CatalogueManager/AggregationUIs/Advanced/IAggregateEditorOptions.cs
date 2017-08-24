using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents.Annotations;

namespace CatalogueManager.AggregationUIs.Advanced
{
    public interface IAggregateEditorOptions
    {
        string GetTitleTextPrefix(AggregateConfiguration aggregate);

        string GetRefreshSQL(AggregateConfiguration aggregate);

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
