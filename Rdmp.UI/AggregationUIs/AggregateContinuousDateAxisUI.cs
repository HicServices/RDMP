// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding.SyntaxChecking;

namespace Rdmp.UI.AggregationUIs;

/// <summary>
/// One Dimension (Group By Column) can be set to be a 'Continuous Date Axis'.  This must be a DateTime or Date field or a transform that yields a Date/DateTime.  Normally with SQL
/// when you include a Date in a GroupBy it will group it by unique value (just like any other field), most SQL users will get around this by using a function such as Year(MyDateCol) 
/// to produce Aggregate of records per year.  However this approach will not fill in years where no date exists.
/// 
/// <para>Setting an AggregateContinuousDateAxis will generate a continuous record set for the date field specified even when there are no records.  For example you can set up an axis that
/// goes from 2001-01-01 to 2016-01-01 in increments of 1 month without having to worry about gaps in the axis or outlier dates (e.g. freaky dates like 1900-01-01).</para>
/// </summary>
public partial class AggregateContinuousDateAxisUI : UserControl
{
    private AggregateDimension _dimension;
    private AggregateContinuousDateAxis _axis;

    private ErrorProvider _errorProvider = new();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AggregateDimension Dimension
    {
        get => _dimension;
        set
        {
            _dimension = value;

            if (value == null)
            {
                groupBox1.Enabled = false;
                return;
            }

            groupBox1.Enabled = true;

            _axis = value.AggregateContinuousDateAxis;

            UpdateFormStateToMatchAxisState();
            
        }
    }

    private bool updating;
        
    private void UpdateFormStateToMatchAxisState()
    {
        updating = true;
        if (_axis == null)
        {
            tbEndDate.Visible = false;
            tbStartDate.Visible = false;
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            ddIncrement.Visible = false;
        }
        else
        {
            tbEndDate.Visible = true;
            tbStartDate.Visible = true;
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            ddIncrement.Visible = true;

            tbStartDate.Text = _axis.StartDate;
            tbEndDate.Text = _axis.EndDate;
            ddIncrement.SelectedItem = _axis.AxisIncrement;
        }

        updating = false;
    }

    public AggregateContinuousDateAxisUI()
    {
        InitializeComponent();
        ddIncrement.DataSource = new object[]{AxisIncrement.Month,AxisIncrement.Quarter,AxisIncrement.Year}; //don't offer day, that was a terrible idea
    }


    private void ddIncrement_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (updating || _axis == null)
            return;

        _axis.AxisIncrement = (AxisIncrement) ddIncrement.SelectedValue;
        _axis.SaveToDatabase();
    }
        
    private void tbDates_TextChanged(object sender, EventArgs e)
    {
        if (updating || _axis == null)
            return;

        var s = (TextBox) sender;

        if (string.IsNullOrWhiteSpace(s.Text))
        {
            _errorProvider.SetError(s, "Field cannot be blank");
            _errorProvider.Tag = s;
        }
        else if(_errorProvider.Tag == s)
            _errorProvider.Clear();

        //if user enters a date then put 
        if (DateTime.TryParse(s.Text, out var dt))
        {
            updating = true;
            s.Text = $"'{dt:yyyy-MM-dd}'";
            updating = false;
        }

        if(s == tbStartDate)
            _axis.StartDate = s.Text;
        else
            _axis.EndDate = s.Text;

        try
        {
            SyntaxChecker.ParityCheckCharacterPairs(new[] {'(', '\''}, new[] {')', '\''}, s.Text);
            s.ForeColor = Color.Black;

            _axis.SaveToDatabase();
            ((TextBox) sender).Focus();
        }
        catch (SyntaxErrorException)
        {
            s.ForeColor = Color.Red;
        }
    }
}