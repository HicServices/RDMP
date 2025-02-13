// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.Collections;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.AggregationUIs.Advanced;

/// <summary>
/// Allows you to pick which columns are used to build an AggregateConfiguration.  This includes all AggregateDimensions (and the count column) as well as all available columns which could
/// be included.  This UI handles column selection / editing for both regular Aggregate Graphs, Cohort Sets and Patient Index Tables (because they are all actually just AggregateConfiguration
/// objects anyway).
/// 
/// <para>Ticking a column includes it in the configuration, unticking it deletes it.  If you have ticked an ExtractionInformation it will become an AggregateDimension which means when you change its
/// SQL implementation it will not affect the main extraction implementation.  This means that if you tick a column and modify it then untick it you will loose the changes.</para>
/// 
/// <para>The count column appears in Wheat background color and can be modified to any GROUP BY aggregate function e.g. max(dt)</para>
/// </summary>
public partial class SelectColumnUI : RDMPUserControl
{
    private IAggregateBuilderOptions _options;
    private AggregateConfiguration _aggregate;

    private readonly List<IColumn> _availableColumns;
    private readonly List<IColumn> _includedColumns;

    internal IReadOnlyCollection<IColumn> AvailableColumns => new ReadOnlyCollection<IColumn>(_availableColumns);
    internal IReadOnlyCollection<IColumn> IncludedColumns => new ReadOnlyCollection<IColumn>(_includedColumns);

    private readonly QuerySyntaxHelper _querySyntaxHelper = MicrosoftQuerySyntaxHelper.Instance;

    private readonly Bitmap _add;
    private readonly Bitmap _delete;
    private CountColumnRequirement _countColumnRequirement;

    public SelectColumnUI()
    {
        InitializeComponent();

        olvSelectColumns.ButtonClick += ButtonClick;

        _availableColumns = new List<IColumn>();
        _includedColumns = new List<IColumn>();

        olvEditInPopup.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        olvEditInPopup.AspectGetter = rowObject => _includedColumns.Contains(rowObject) ? "Edit..." : null;

        olvIncluded.AspectGetter = rowObject => _includedColumns.Contains(rowObject) ? "Included" : "Not Included";


        olvGroupBy.AspectGetter = static rowObject =>
        {
            return rowObject switch
            {
                AggregateDimension ad => ad.GroupBy,
                ExtractionInformation ei => ei.GroupBy,
                AggregateCountColumn acc => acc.GroupBy,
                _ => null
            };
        };

        olvSelectColumns.AlwaysGroupByColumn = olvIncluded;
        olvSelectColumns.RowFormatter += RowFormatter;

        olvSelectColumns.CellEditStarting += CellEditStarting;
        olvSelectColumns.CellEditFinished += CellEditFinished;


        olvSelectColumns.SubItemChecking += (object _, SubItemCheckingEventArgs args) =>
        {
            if (args.Column != olvGroupBy) return;

            switch (args.RowObject)
            {
                case AggregateDimension ad:
                    ad.GroupBy = !ad.GroupBy;
                    ad.SaveToDatabase();
                    return;
                case ExtractionInformation ei:
                    ei.GroupBy = !ei.GroupBy;
                    ei.SaveToDatabase();
                    return;
            }
        };

        olvAddRemove.ImageGetter += ImageGetter;
        olvSelectColumns.CellClick += olvSelectColumns_CellClick;

        _add = FamFamFamIcons.add.ImageToBitmap();
        _delete = FamFamFamIcons.delete.ImageToBitmap();
    }

    private void olvSelectColumns_CellClick(object sender, CellClickEventArgs e)
    {
        if (e.Column == olvAddRemove)
        {
            var countColumn = e.Model as AggregateCountColumn;
            var importableColumn = e.Model as ExtractionInformation;
            var dimensionColumn = e.Model as AggregateDimension;

            if (countColumn == null && importableColumn == null && dimensionColumn == null)
                throw new Exception(
                    $"Object in list view of type that wasn't IColumn, it was {e.Model.GetType().Name}");

            //if it is an add
            if (_availableColumns.Contains(e.Model))
            {
                //count column added
                if (countColumn != null)
                    if (_options.GetCountColumnRequirement(_aggregate) == CountColumnRequirement.CannotHaveOne)
                        WideMessageBox.Show("Cohort Sets cannot have count columns",
                            "A count column is a SELECT column with an aggregate function (count(*), sum(x) etc).  The SELECT component for cohort identification must be the patient id column only.");
                    else
                        Save(countColumn);

                //regular column added
                if (importableColumn != null)
                {
                    var dimension = new AggregateDimension(Activator.RepositoryLocator.CatalogueRepository,
                        importableColumn, _aggregate);

                    _availableColumns.Remove(importableColumn);
                    _includedColumns.Add(dimension);

                    olvSelectColumns.RemoveObject(importableColumn);
                    olvSelectColumns.AddObject(dimension);
                    olvSelectColumns.EnsureModelVisible(dimension);

                    Save(dimension);

                    //object doesn't exist, that might cause problems
                    return;
                }
            }
            else
            {
                //it is a removal

                //user is trying to remove count column
                if (countColumn != null)
                {
                    if (_options.GetCountColumnRequirement(_aggregate) == CountColumnRequirement.MustHaveOne)
                        return; //leave it checked - removal is forbidden

                    _aggregate.CountSQL = "";
                    _aggregate.SaveToDatabase();
                    _includedColumns.Remove(countColumn);
                    _availableColumns.Add(countColumn);

                    olvSelectColumns.RemoveObject(countColumn);
                    olvSelectColumns.AddObject(countColumn);
                    olvSelectColumns.EnsureModelVisible(countColumn);

                    Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
                }


                //user is trying to remove a dimension
                if (dimensionColumn != null)
                {
                    dimensionColumn.DeleteInDatabase();

                    //get the master it was based on
                    var extractionInformation = dimensionColumn.ExtractionInformation;
                    try
                    {
                        dimensionColumn.DeleteInDatabase();
                    }
                    catch (Exception ex)
                    {
                        //couldn't delete it so don't update the UI just tell the user why
                        ExceptionViewer.Show(ex);
                        return;
                    }

                    //remove it from the inclusion list
                    _includedColumns.Remove(dimensionColumn);
                    olvSelectColumns.RemoveObject(dimensionColumn);

                    //add the master importable version it was based on into available columns again
                    _availableColumns.Add(extractionInformation);
                    olvSelectColumns.AddObject(extractionInformation);

                    Save(extractionInformation);
                }
            }
        }
    }

    private Bitmap ImageGetter(object rowObject)
    {
        if (_availableColumns.Contains(rowObject))
            return _add;

        //if we are getting an icon for the count(*) column and it cannot be removed then don't show the icon for removal
        return _countColumnRequirement == CountColumnRequirement.MustHaveOne && rowObject is AggregateCountColumn
            ? null
            : _delete;
    }

    private void CellEditFinished(object sender, CellEditEventArgs cellEditEventArgs)
    {
        var col = (IColumn)cellEditEventArgs.RowObject;

        //user deleted its value
        if (string.IsNullOrWhiteSpace(col.SelectSQL) && col.ColumnInfo != null)
        {
            col.SelectSQL = col.ColumnInfo.Name;
            cellEditEventArgs.NewValue = col.SelectSQL;
        }

        if (cellEditEventArgs.RowObject != null)
            Save(col);
    }

    private void CellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
    {
        //don't let them rename the edit button!
        if (cellEditEventArgs.Column == olvEditInPopup)
            cellEditEventArgs.Cancel = true;

        //don't let them edit the masters
        if (cellEditEventArgs.RowObject is ExtractionInformation)
            cellEditEventArgs.Cancel = true;
    }

    private void RowFormatter(OLVListItem olvItem)
    {
        if (olvItem.RowObject is IColumn col)
        {
            olvItem.BackColor = col is AggregateCountColumn ? Color.Wheat : Color.White;
            olvItem.ForeColor = _availableColumns.Contains(col) ? Color.Gray : Color.Black;
        }
    }

    private void ButtonClick(object sender, CellClickEventArgs cellClickEventArgs)
    {
        if (cellClickEventArgs.Column == olvEditInPopup)
            if (cellClickEventArgs.Model is IColumn col)
            {
                var dialog = new SetSQLDialog(col.SelectSQL, new RDMPCombineableFactory());

                var querySyntaxSource = col as IHasQuerySyntaxHelper ?? _aggregate;

                var autoComplete = new AutoCompleteProviderWin(querySyntaxSource.GetQuerySyntaxHelper());
                autoComplete.Add(col);
                autoComplete.Add(_aggregate);

                autoComplete.RegisterForEvents(dialog.QueryEditor);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    col.SelectSQL = dialog.Result;
                    Save(col);
                }
            }
    }

    /// <summary>
    /// saves changes to an IColumn made by the user and publishes that the AggregateConfiguration has changed
    /// </summary>
    /// <param name="col"></param>
    private void Save(IColumn col)
    {
        if (col is AggregateCountColumn countCol)
        {
            _aggregate.CountSQL = countCol.GetFullSelectLineStringForSavingIntoAnAggregate();
            _aggregate.SaveToDatabase();
            if (!_includedColumns.Contains(countCol))
            {
                _includedColumns.Add(countCol);
                _availableColumns.Remove(countCol);

                olvSelectColumns.RemoveObject(countCol);
                olvSelectColumns.AddObject(countCol);
                olvSelectColumns.EnsureModelVisible(countCol);
            }

            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
            return;
        }

        if (col is ISaveable saveable)
            saveable.SaveToDatabase();

        _aggregate.SaveToDatabase();
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
    }


    public void SetUp(IActivateItems activator, IAggregateBuilderOptions options, AggregateConfiguration aggregate)
    {
        //record new states so we don't accidentally erase names of stuff
        SetItemActivator(activator);
        _options = options;
        _countColumnRequirement = _options.GetCountColumnRequirement(aggregate);
        _aggregate = aggregate;

        _availableColumns.Clear();
        _includedColumns.Clear();
        olvSelectColumns.ClearObjects();

        _availableColumns.AddRange(_options.GetAvailableSELECTColumns(_aggregate));
        _includedColumns.AddRange(_aggregate.AggregateDimensions);

        //add count option unless it cannot have one
        if (_options.GetCountColumnRequirement(_aggregate) != CountColumnRequirement.CannotHaveOne)
        {
            AggregateCountColumn countCol;

            if (!string.IsNullOrEmpty(_aggregate.CountSQL))
            {
                countCol = new AggregateCountColumn(_aggregate.CountSQL);
                _includedColumns.Add(countCol);
            }
            else
            {
                countCol = new AggregateCountColumn("count(*) as CountColumn");
                _availableColumns.Add(countCol);
            }

            countCol.SetQuerySyntaxHelper(_querySyntaxHelper, true);
        }

        olvSelectColumns.AddObjects(_includedColumns);
        olvSelectColumns.AddObjects(_availableColumns);
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        olvSelectColumns.UseFiltering = true;
        olvSelectColumns.ModelFilter = new TextMatchFilterWithAlwaysShowList(_includedColumns, olvSelectColumns,
            tbFilter.Text, StringComparison.CurrentCultureIgnoreCase);
    }
}