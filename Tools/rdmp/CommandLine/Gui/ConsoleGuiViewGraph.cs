using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiViewGraph : Window
    {
        public ConsoleGuiViewGraph(IBasicActivateItems activator, AggregateConfiguration aggregateConfiguration)
        {
            Add(new Label("Graphs comming here soon"));
        }
    }
}
