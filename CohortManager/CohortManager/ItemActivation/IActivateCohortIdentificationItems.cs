using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CohortManager.SubComponents;
using CohortManager.SubComponents.Graphs;

namespace CohortManager.ItemActivation
{
    public interface IActivateCohortIdentificationItems:IActivateItems
    {
        void ExecuteCohortSummaryGraph(object sender, CohortSummaryAggregateGraphObjectCollection objectCollection);
    }
}
