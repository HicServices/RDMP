using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Changes the pivot dimension of an aggregate graph
    /// </summary>
    public class ExecuteCommandSetPivot : BasicCommandExecution
    {
        private readonly AggregateConfiguration aggregate;
        private readonly string column;
        private readonly bool askAtRuntime;

        public ExecuteCommandSetPivot(IBasicActivateItems basicActivator, AggregateConfiguration aggregate):base(basicActivator)
        {
            this.aggregate = aggregate;

            // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
            if (aggregate.IsCohortIdentificationAggregate)
            {
                SetImpossible($"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
                return;
            }

            askAtRuntime = true;
        }

        
        [UseWithObjectConstructor]
        public ExecuteCommandSetPivot(IBasicActivateItems basicActivator, AggregateConfiguration aggregate, string column ) : base(basicActivator)
        {
            this.aggregate = aggregate;
            this.column = column;

            if (!string.IsNullOrWhiteSpace(column))
            {
                // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
                if(aggregate.IsCohortIdentificationAggregate)
                {
                    SetImpossible($"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
                    return;
                }
            }
            else
            {
                if(aggregate.PivotOnDimensionID == null)
                {
                    SetImpossible($"AggregateConfiguration {aggregate} does not have a pivot to clear");
                }
            }

        }
        public override void Execute()
        {
            base.Execute();

            if (string.IsNullOrWhiteSpace(column) && !askAtRuntime)
            {
                aggregate.PivotOnDimensionID = null;
                aggregate.SaveToDatabase();
            }
            else
            {
                var opts = new AggregateBuilderOptionsFactory().Create(aggregate);
                AggregateDimension match = null;

                if (askAtRuntime)
                {
                    var possible = aggregate.AggregateDimensions.Where(d => !d.IsDate()).ToArray();

                    if (!possible.Any())
                    {
                        throw new Exception($"There are no AggregateDimensions in {aggregate} that can be used as a Pivot");
                    }

                    match = (AggregateDimension)BasicActivator.SelectOne("Choose pivot dimension", possible);
                    
                    if(match == null)
                    {
                        return;
                    }
                }
                else
                {
                    match = aggregate.AggregateDimensions.FirstOrDefault(a => string.Equals(column, a.ToString()));
                    if (match == null)
                    {
                        throw new Exception($"Could not find AggregateDimension {column} in Aggregate {aggregate} so could not set it as a pivot dimension.  Try adding the column to the aggregate first");
                    }
                }

                if (match.IsDate())
                {
                    throw new Exception($"AggregateDimension {match} is a Date so cannot set it as a Pivot for Aggregate {aggregate}");
                }

                var enable = opts.ShouldBeEnabled(AggregateEditorSection.PIVOT, aggregate);

                if (!enable)
                {
                    throw new Exception($"Current state of Aggregate {aggregate} does not support having a Pivot Dimension");
                }

                aggregate.PivotOnDimensionID = match.ID;
                aggregate.SaveToDatabase();
            }

            Publish(aggregate);
        }
    }
}
