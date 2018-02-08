using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.AggregationUIs.Advanced.Options
{
    class AggregateEditorBasicOptions : IAggregateEditorOptions
    {
        public string GetTitleTextPrefix(AggregateConfiguration aggregate)
        {
            if(aggregate.IsExtractable)
                return "Extractable 'Group By' Aggregate:";

            return "'Group By' Aggregate:";
        }

        public string GetRefreshSQL(AggregateConfiguration aggregate)
        {
            return aggregate.GetQueryBuilder().SQL;
        }

        public IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate)
        {
            var existingDimensions = aggregate.AggregateDimensions.Select(d => d.ExtractionInformation).ToArray();

            return aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any) //all columns of any extraction category
                .Except(existingDimensions)//except those that have already been added
                .Where(e => !e.IsExtractionIdentifier)//don't advertise IsExtractionIdentifier columns for use in basic aggregates
                .Cast<IColumn>()
                .ToArray();

        }

        public IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate)
        {
            //for this basic case the WHERE columns can be anything
            return aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Cast<IColumn>().ToArray();
        }

        public bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate)
        {
            switch (section)
            {
                case AggregateEditorSection.ExtractableTickBox:
                    return CanMakeExtractable(aggregate);
                case AggregateEditorSection.SELECT:
                    return true;
                case AggregateEditorSection.TOPX:
                    //can only Top X if we have a pivot (top x applies to the selection of the pivot values) or if we have nothing (no axis / pivot).  This rules out axis only queries 
                    return aggregate.PivotOnDimensionID != null || aggregate.GetAxisIfAny() == null;
                case AggregateEditorSection.FROM:
                    return true;
                case AggregateEditorSection.WHERE:
                    return true;
                case AggregateEditorSection.HAVING:
                    return true;
                case AggregateEditorSection.PIVOT:
                    return aggregate.GetAxisIfAny() != null;//can only pivot if there is an axis
                case AggregateEditorSection.AXIS:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }
        }

        public IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate)
        {
            var availableTables = aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                .Select(e => e.ColumnInfo != null? e.ColumnInfo.TableInfo:null)
                .Where( t=> t != null)
                .Distinct();

            var implicitJoins =
                aggregate.ForcedJoins.Union(
                    aggregate.AggregateDimensions.Select(d => d.ExtractionInformation.ColumnInfo.TableInfo).Distinct());

            //return all TableInfos that are not already force joined
            return availableTables.Except(implicitJoins).Cast<IMapsDirectlyToDatabaseTable>().ToArray();
        }

        private bool CanMakeExtractable(AggregateConfiguration aggregate)
        {
            //if it has any extraction identifiers then it cannot be extractable!
            if (aggregate.AggregateDimensions.Any(d => d.IsExtractionIdentifier))
            {
                aggregate.IsExtractable = false;
                aggregate.SaveToDatabase();
                return false;
            }

            return true;
        }

        public ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate)
        {
            return aggregate.GetAllParameters();
        }

        public CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate)
        {
            return CountColumnRequirement.MustHaveOne;
        }
    }
}
