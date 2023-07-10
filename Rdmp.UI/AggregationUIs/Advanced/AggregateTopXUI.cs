// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.AggregationUIs.Advanced;

/// <summary>
/// Allows you to limit the graph generated to X bars (in the case of a graph without an axis) or restrict the number of Pivot values returned.  For example you can graph 'Top 10 most
/// prescribed drugs'.  Top X is meaningless without an order by statement, therefore you must also configure a dimension to order by (and a direction).  In most cases you should leave
/// the Dimension at 'Count Column' this will mean that whatever your count dimension is (usually count(*)) will be used to determine the TOP X.  Setting to Ascending will give you the
/// lowest number e.g. 'Top 10 LEAST prescribed drugs' instead.  If you change the dimension from the 'count column' to one of your dimensions then the TOP X will apply to that column
/// instead.  e.g. the 'The first 10 prescribed drugs alphabetically' (not particularly useful).
/// </summary>
public partial class AggregateTopXUI : RDMPUserControl
{
    private AggregateTopX _topX;
    private AggregateConfiguration _aggregate;

    private const string CountColumn  = "Count Column";

    public AggregateTopXUI()
    {
        InitializeComponent();

        //Stop mouse wheel scroll from scrolling the combobox when it's closed to avoid the value being changed without user noticing.
        RDMPControlCommonFunctionality.DisableMouseWheel(ddAscOrDesc);
        RDMPControlCommonFunctionality.DisableMouseWheel(ddOrderByDimension);
    }

    private bool bLoading;

    public void SetUp(IActivateItems activator, IAggregateBuilderOptions options, AggregateConfiguration aggregate)
    {
        SetItemActivator(activator);

        Enabled = options.ShouldBeEnabled(AggregateEditorSection.TOPX, aggregate);
        _aggregate = aggregate;
        _topX = aggregate.GetTopXIfAny();

        //if a TopX exists and control is disabled
        if (!Enabled && _topX != null)
        {
            _topX.DeleteInDatabase();
            _topX = null;
        }

        RefreshUIFromDatabase();
    }

    private void RefreshUIFromDatabase()
    {
        bLoading = true;
        ddOrderByDimension.Items.Clear();
        ddOrderByDimension.Items.Add(CountColumn);
        ddOrderByDimension.Items.AddRange(_aggregate.AggregateDimensions);

        if (_topX != null)
        {
            ddOrderByDimension.Enabled = true;
            ddAscOrDesc.Enabled = true;

            tbTopX.Text = _topX.TopX.ToString();
            ddAscOrDesc.DataSource = Enum.GetValues(typeof(AggregateTopXOrderByDirection));
            ddAscOrDesc.SelectedItem = _topX.OrderByDirection;

            if (_topX.OrderByDimensionIfAny_ID == null)
                ddOrderByDimension.SelectedItem = CountColumn;
            else
                ddOrderByDimension.SelectedItem = _topX.OrderByDimensionIfAny;
        }
        else
        {
            ddOrderByDimension.Enabled = false;
            ddAscOrDesc.Enabled = false;
        }

        bLoading = false;
    }

    private void tbTopX_TextChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        //user is trying to delete an existing TopX
        if (_topX != null && string.IsNullOrWhiteSpace(tbTopX.Text))
        {
            _topX.DeleteInDatabase();
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
            return;
        }

        //user is typing something illegal like 'ive got a lovely bunch o coconuts'
        if (!int.TryParse(tbTopX.Text, out var i))
        {
            //not an int
            tbTopX.ForeColor = Color.Red;
            return;
        }

        //user put in a negative
        if (i <= 0)
        {
            tbTopX.ForeColor = Color.Red;
            return;
        }

        tbTopX.ForeColor = Color.Black;

        //there isn't one yet
        if (_topX == null)
        {
            _topX = new AggregateTopX(Activator.RepositoryLocator.CatalogueRepository, _aggregate, i);
        }
        else
        {
            //there is one so change its topX
            _topX.TopX = i;
            _topX.SaveToDatabase();
        }

        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
    }

    private void ddOrderByDimension_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        if (_topX == null || ddOrderByDimension.SelectedItem == null)
            return;

        if (ddOrderByDimension.SelectedItem is AggregateDimension dimension)
            _topX.OrderByDimensionIfAny_ID = dimension.ID;
        else
            _topX.OrderByDimensionIfAny_ID = null; //means use count column

        _topX.SaveToDatabase();
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
    }

    private void ddAscOrDesc_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        if (_topX == null || ddAscOrDesc.SelectedItem == null)
            return;

        _topX.OrderByDirection = (AggregateTopXOrderByDirection)ddAscOrDesc.SelectedItem;
        _topX.SaveToDatabase();
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
    }
}