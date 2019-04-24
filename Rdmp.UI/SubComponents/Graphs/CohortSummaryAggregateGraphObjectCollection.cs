// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data.Cohort;
using Rdmp.Core.CatalogueLibrary.Data.Dashboarding;
using Rdmp.Core.CohortCreation.QueryBuilding;

namespace CohortManager.SubComponents.Graphs
{
    public class CohortSummaryAggregateGraphObjectCollection:PersistableObjectCollection
    {
        public CohortSummaryAdjustment Adjustment;
        public AggregateFilter SingleFilterOnly { get { return DatabaseObjects.OfType<AggregateFilter>().SingleOrDefault(); } }

        public CohortAggregateContainer CohortContainerIfAny { get { return DatabaseObjects[0] as CohortAggregateContainer; } }
        public AggregateConfiguration CohortIfAny { get { return DatabaseObjects[0] as AggregateConfiguration; } }

        public AggregateConfiguration Graph { get { return (AggregateConfiguration)DatabaseObjects[1]; } }

        /// <summary>
        /// Do not use this constructor, it is used only for deserialization during persistence on form loading after application closing
        /// </summary>
        public CohortSummaryAggregateGraphObjectCollection()
        {
        }

        /// <summary>
        /// Use this constructor at runtime
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="graph"></param>
        /// <param name="adjustment"></param>
        public CohortSummaryAggregateGraphObjectCollection(AggregateConfiguration cohort, AggregateConfiguration graph,CohortSummaryAdjustment adjustment):this()
        {
            if(!cohort.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter cohort was AggregateConfiguration '" + cohort + "' which is not a Cohort Aggregate (not allowed)","cohort");
            if (graph.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter graph was AggregateConfiguration '" + graph + "' which is a Cohort Aggregate (not allowed)", "graph");

            DatabaseObjects.Add(cohort);
            DatabaseObjects.Add(graph);
            Adjustment = adjustment;
        }
        /// <summary>
        /// Overload that does the operation on a container with (WhereExtractionIdentifiersIn - the only permissable option)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="graph"></param>
        public CohortSummaryAggregateGraphObjectCollection(CohortAggregateContainer container, AggregateConfiguration graph)
            : this()
        {
            if (graph.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter graph was AggregateConfiguration '" + graph + "' which is a Cohort Aggregate (not allowed)", "graph");

            DatabaseObjects.Add(container);
            DatabaseObjects.Add(graph);
            Adjustment = CohortSummaryAdjustment.WhereExtractionIdentifiersIn;
        }

        public CohortSummaryAggregateGraphObjectCollection(AggregateConfiguration cohort, AggregateConfiguration graph, CohortSummaryAdjustment adjustment, AggregateFilter singleFilterOnly):this(cohort,graph,adjustment)
        {
            DatabaseObjects.Add(singleFilterOnly);
        }

        public override string SaveExtraText()
        {
            return Adjustment.ToString();
        }

        public override void LoadExtraText(string s)
        {
            CohortSummaryAdjustment a;
            
            if(!CohortSummaryAdjustment.TryParse(s, out a))
                throw new Exception("Could not parse '" + s + "' into a valid CohortSummaryAdjustment");

            Adjustment = a;
        }

        public void RevertIfMatchedInCollectionObjects(DatabaseEntity oTriggeringRefresh, out bool shouldClose)
        {
            shouldClose = false;

            var matchingObject = DatabaseObjects.SingleOrDefault(o => o.Equals(oTriggeringRefresh)) as IRevertable;

            //matched object in our collection
            if(matchingObject != null)
                if (matchingObject.Exists()) 
                    matchingObject.RevertToDatabaseState();
                else
                    shouldClose = true;//object doesn't exist anymore so close control

            
        }
    }
}