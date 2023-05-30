// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.UI.AggregationUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.ExtractionUIs.FilterUIs;

/// <summary>
/// Shows a given Aggregate Graph with an additional IFilter applied.  This can be used for checking that a filter SQL is working how you intend by giving you a view you are already 
/// familiar with (the graph you created) but with the addition of the filter.  You can also launch the graph normally (See AggregateGraph) to see a side by side comparison
/// </summary>
public partial class FilterGraphUI : AggregateGraphUI, IObjectCollectionControl
{
    private FilterGraphObjectCollection _collection;

    public FilterGraphUI()
    {
        InitializeComponent();
    }
        
    protected override AggregateBuilder GetQueryBuilder(AggregateConfiguration aggregateConfiguration)
    {
        var basicQueryBuilder =  base.GetQueryBuilder(aggregateConfiguration);
            
        var rootContainer = basicQueryBuilder.RootFilterContainer;

        //stick our IFilter into the root container (actually create a new root container with our filter in it and move the old root if any into it)
        rootContainer =
            new SpontaneouslyInventedFilterContainer(new MemoryCatalogueRepository(),rootContainer == null ? null : new[] { rootContainer },
                new[] {_collection.GetFilter()}, FilterContainerOperation.AND);

        basicQueryBuilder.RootFilterContainer = rootContainer;

        return basicQueryBuilder;
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        _collection.HandleRefreshObject(e);
    }

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        _collection = (FilterGraphObjectCollection)collection;
        SetItemActivator(activator);
            
        BuildMenu(activator);

        SetAggregate(Activator,_collection.GetGraph());
        LoadGraphAsync();
    }

    public IPersistableObjectCollection GetCollection()
    {
        return _collection;
    }
    public override string GetTabName()
    {
        return $"Filter Graph '{_collection.GetFilter()}'";
    }
}