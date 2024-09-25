using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandReorderFilter : BasicUICommandExecution
    {
        private ConcreteFilter _source;
        private ConcreteFilter _target;
        private InsertOption _insertOption;

        public ExecuteCommandReorderFilter(IActivateItems activator, ConcreteFilter source, ConcreteFilter destination, InsertOption insertOption) : base(activator)
        {
            _source = source;
            _target = destination;
            _insertOption = insertOption;
            //what if there are no filter containers?
            if (_source.FilterContainer_ID != _target.FilterContainer_ID)
            {
                SetImpossible("Cannot reorder as they do not share a parent");
            }
        }

        public override void Execute()
        {
            var order = _target.Order;

            var filters = _target.FilterContainer.GetFilters().Where(f => f is ConcreteFilter).Select(f => (ConcreteFilter)f).ToArray();
            Array.Sort(
               filters,
                delegate (ConcreteFilter a, ConcreteFilter b) { return a.Order.CompareTo(b.Order); }
            );
            if (!filters.All(c => c.Order != order))
            {
                foreach(var orderable in filters)
                {
                    if (orderable.Order < order)
                        orderable.Order--;
                    else if (orderable.Order > order)
                        orderable.Order++;
                    else //collision on order
                        orderable.Order += _insertOption == InsertOption.InsertAbove ? 1 : -1;
                    ((ISaveable)orderable).SaveToDatabase();
                }
            }
            _source.Order = order;
            _source.SaveToDatabase();
            Publish(_target.FilterContainer);
        }
    }

}
