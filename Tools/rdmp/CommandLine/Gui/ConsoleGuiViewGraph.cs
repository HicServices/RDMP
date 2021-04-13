using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataViewing;
using Rdmp.Core.QueryBuilding;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiViewGraph : ConsoleGuiSqlEditor
    {
        private GraphView graphView;
        private Tab graphTab;

        public ConsoleGuiViewGraph(IBasicActivateItems activator, AggregateConfiguration aggregate) :
            base
            (activator, new ViewAggregateExtractUICollection(aggregate) { TopX = null })
        {
            graphView = new GraphView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            TabView.AddTab(graphTab = new Tab("Graph",graphView), false);
        }

        protected override void OnQueryCompleted(DataTable dt)
        {
            base.OnQueryCompleted(dt);

            //todo add to graph
            TabView.SelectedTab = graphTab;
        }
    }
}
