using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Adds a new <see cref="AggregateDimension"/> to a <see cref="AggregateConfiguration"/> based on one of the associated <see cref="Catalogue"/> <see cref="ExtractionInformation"/>
    /// </summary>
    public class ExecuteCommandAddDimension : BasicCommandExecution
    {
        private readonly AggregateConfiguration aggregate;
        private readonly string column;
        private readonly bool askAtRuntime;

        public ExecuteCommandAddDimension(IBasicActivateItems basicActivator, AggregateConfiguration aggregate) : base(basicActivator)
        {
            this.aggregate = aggregate;
            askAtRuntime = true;
        }

        [UseWithObjectConstructor]
        public ExecuteCommandAddDimension(IBasicActivateItems basicActivator, AggregateConfiguration aggregate, string column) : base(basicActivator)
        {
            this.aggregate = aggregate;
            this.column = column;

            if (!string.IsNullOrWhiteSpace(column))
            {
                // don't let them try to set a pivot on a cohort aggregate configuration but do let them clear it if it somehow ended up with one
                if (aggregate.IsCohortIdentificationAggregate)
                {
                    SetImpossible($"AggregateConfiguration {aggregate} is a cohort identification aggregate and so cannot have a pivot");
                    return;
                }
            }
            else
            {
                if (aggregate.PivotOnDimensionID == null)
                {
                    SetImpossible($"AggregateConfiguration {aggregate} does not have a pivot to clear");
                }
            }
        }

        public override void Execute()
        {
            base.Execute();

            var opts = new AggregateBuilderOptionsFactory().Create(aggregate);
            ExtractionInformation match = null;

            var possible = opts.GetAvailableSELECTColumns(aggregate).OfType<ExtractionInformation>().ToArray();

            if (askAtRuntime)
            {
                if (!possible.Any())
                {
                    throw new Exception($"There are no ExtractionInformation that can be added as new dimensions to {aggregate}");
                }

                match = (ExtractionInformation)BasicActivator.SelectOne("Choose dimension to add", possible);

                if (match == null)
                {
                    return;
                }
            }
            else
            {
                match = possible.FirstOrDefault(a => string.Equals(column, a.ToString()));
                if (match == null)
                {
                    throw new Exception($"Could not find ExtractionInformation {column} in as an addable column to {aggregate}");
                }
            }

            var dim = new AggregateDimension(BasicActivator.RepositoryLocator.CatalogueRepository, match, aggregate);
            dim.SaveToDatabase();

            Publish(aggregate);
        }
    }
}
