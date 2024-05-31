// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.AggregationUIs.Advanced;

/// <summary>
/// Allows you to adjust an Aggregate.  This can either be a breakdown of your dataset by columns possibly including a graph (Basic Aggregate), a list of patient identifiers (Identifier
/// List) or a patient index table (See AggregateConfiguration). The image in the top left tells you what type of AggregateConfiguration it is.
///  
/// <para>Clicking the 'Parameters' button will launch the ParameterCollectionUI dialogue which will let you edit which SQL Parameters @startDate etc are available for use in filters on the
/// AggregateConfiguration</para>
/// 
/// <para>If you are editing a Basic Aggregate that does not include any patient identifier columns (IsExtractionIdentifier) then you can tick IsExtractable to make it available for use and
/// extraction for researchers who use the underlying dataset and receive a data extraction (they will receive the 'master' aggregate run on the entire data repository and a 'personal'
/// version which is the same query run against their project extraction only) See ExtractionAggregateGraphObjectCollection.</para>
/// 
/// <para>You can click in the SQL and Alias columns to rename columns or change their SQL.  You can also click in the 'Join Direction' column to edit the direction (LEFT or RIGHT) of
/// any supplemental JOINs.</para>
/// 
/// <para>If your Catalogue has multiple underlying TableInfos you can pick which ones to include in the query generated in the FROM section (any Columns included in the SELECT section
/// will be automatically included)</para>
/// 
/// <para>Typing into the HAVING block will make the Query Builder add the SQL into the HAVING section of a GROUP BY SQL statement</para>
/// 
/// <para>You can (if it is a Basic Aggregate) choose a single column to PIVOT on.  This will turn row values into new column headers.  For example if you have a dataset with columns 'Date, Gender,
/// Result' then you could pivot on Gender and the result set would have columns Date,Male,Female,Other,NumberOfResults' assuming your count SQL was called NumberOfResults.  Do not pick
/// a column with thousands of unique values or you will end up with a very unwieldy result set that will probably crash the AggregateGraph when run.</para>
/// 
/// <para>One (DATE!) column can be marked as an Axis.  See AggregateContinuousDateAxisUI for description.</para>
/// 
/// </summary>
public partial class AggregateEditorUI : AggregateEditor_Design, ISaveableUI
{
    private IAggregateBuilderOptions _options;
    private AggregateConfiguration _aggregate;

    private List<ITableInfo> _forcedJoins;

    private IQuerySyntaxHelper _querySyntaxHelper;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private Scintilla _queryHaving;

    private readonly ErrorProvider _errorProviderAxis = new();

    //Constructor
    public AggregateEditorUI()
    {
        InitializeComponent();

        //Stop mouse wheel scroll from scrolling the combobox when it's closed to avoid the value being changed without user noticing.
        RDMPControlCommonFunctionality.DisableMouseWheel(ddAxisDimension);
        RDMPControlCommonFunctionality.DisableMouseWheel(ddPivotDimension);

        if (VisualStudioDesignMode)
            return;

        olvJoin.CheckStateGetter += ForceJoinCheckStateGetter;
        olvJoin.CheckStatePutter += ForceJoinCheckStatePutter;
        olvJoinTableName.ImageGetter += ImageGetter;

        olvJoinDirection.AspectGetter += JoinDirectionGetter;
        olvJoinDirection.AspectPutter += JoinDirectionPutter;

        olvJoin.AddDecoration(new EditingCellBorderDecoration { UseLightbox = true });
    }

    private void JoinDirectionPutter(object rowObject, object newValue)
    {
        if (rowObject is not JoinableCohortAggregateConfigurationUse j) return;
        if (j.JoinType == (ExtractionJoinType)newValue)
            return;

        j.JoinType = (ExtractionJoinType)newValue;
        j.SaveToDatabase();
        Publish();
    }

    private static object JoinDirectionGetter(object rowObject) =>
        rowObject is JoinableCohortAggregateConfigurationUse j ? j.JoinType : null;

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    private CheckState ForceJoinCheckStatePutter(object rowObject, CheckState newValue)
    {
        var ti = rowObject as TableInfo;

        var joiner = _aggregate.CatalogueRepository.AggregateForcedJoinManager;

        //user is trying to use a joinable something
        if (newValue == CheckState.Checked)
        {
            //user is trying to turn on usage of a TableInfo
            if (ti != null)
            {
                joiner.CreateLinkBetween(_aggregate, ti);
                _forcedJoins.Add(ti);
            }

            // user is trying to join to a Patient Index table
            if (rowObject is JoinableCohortAggregateConfiguration patientIndexTable)
            {
                if (_aggregate.Catalogue.IsApiCall())
                {
                    Activator.Show("API calls cannot join to patient index tables");
                    return CheckState.Unchecked;
                }

                // if other check boxes are ticked
                if (olvJoin.CheckedObjects.OfType<JoinableCohortAggregateConfigurationUse>().Any())
                {
                    Activator.Show("You cannot join to more than 1 patient index table at once");
                    return CheckState.Unchecked;
                }

                var joinUse = patientIndexTable.AddUser(_aggregate);
                olvJoin.RemoveObject(patientIndexTable);
                olvJoin.AddObject(joinUse);
            }
        }
        else
        {
            //user is trying to turn off usage of a TableInfo
            if (ti != null)
            {
                joiner.BreakLinkBetween(_aggregate, ti);
                _forcedJoins.Remove(ti);
            }

            if (rowObject is JoinableCohortAggregateConfigurationUse patientIndexTableUse)
            {
                var joinable = patientIndexTableUse.JoinableCohortAggregateConfiguration;

                patientIndexTableUse.DeleteInDatabase();
                olvJoin.RemoveObject(patientIndexTableUse);
                olvJoin.AddObject(joinable);
            }
        }

        SetDatabaseObject(Activator, _aggregate);
        Publish();

        return newValue;
    }

    private CheckState ForceJoinCheckStateGetter(object rowObject)
    {
        if (_forcedJoins == null)
            return CheckState.Indeterminate;

        return rowObject switch
        {
            TableInfo => _forcedJoins.Contains(rowObject) ? CheckState.Checked : CheckState.Unchecked,
            JoinableCohortAggregateConfiguration => CheckState.Unchecked,
            JoinableCohortAggregateConfigurationUse => CheckState.Checked,
            _ => CheckState.Indeterminate
        };
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, AggregateConfiguration databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbDescription, "Text", "Description", static a => a.Description);
        Bind(cbExtractable, "Checked", "IsExtractable", static a => a.IsExtractable);
    }

    private void PopulateTopX()
    {
        _aggregateTopXui1.SetUp(Activator, _options, _aggregate);
    }

    private void DetermineFromTables()
    {
        //implicit use
        var uniqueUsedTables = _aggregate.AggregateDimensions.Select(d =>
                d.ExtractionInformation.ColumnInfo ?? throw new Exception(
                    $"Aggregate Configuration {_aggregate} (Catalogue '{_aggregate.Catalogue}') has a Dimension '{d}' which is an orphan (someone deleted the ColumnInfo)"))
            .Select(static colInfo => colInfo.TableInfo.ToString()).Distinct().ToList();

        lblFromTable.Text = string.Join(",", uniqueUsedTables);

        //explicit use
        olvJoin.ClearObjects();

        //explicit forced joins
        _forcedJoins = _aggregate.ForcedJoins.ToList();

        olvJoin.AddObjects(_forcedJoins);

        //available joinables
        var joinables = _options.GetAvailableJoinables(_aggregate);

        if (joinables != null)
            olvJoin.AddObjects(joinables);

        //and patient index tables too
        olvJoin.AddObjects(_aggregate.PatientIndexJoinablesUsed);
    }

    private void SetNameText()
    {
        if (_aggregate.IsJoinablePatientIndexTable())
            pictureBox1.Image = CatalogueIcons.BigPatientIndexTable.ImageToBitmap();
        else if (_aggregate.IsCohortIdentificationAggregate)
            pictureBox1.Image = CatalogueIcons.BigCohort.ImageToBitmap();
        else
            pictureBox1.Image = CatalogueIcons.BigGraph.ImageToBitmap();

        //set the name to the tostring not the .Name so that we ignore the cic prefix
        tbName.Text = _aggregate.ToString();
    }

    private void olvAny_CellEditFinishing(object sender, CellEditEventArgs e)
    {
        e.Column.PutAspectByName(e.RowObject, e.NewValue);

        switch (e.RowObject)
        {
            case AggregateCountColumn countColumn:
                _aggregate.CountSQL =
                    $"{countColumn.SelectSQL}{(countColumn.Alias != null ? $" as {countColumn.Alias}" : "")}";
                break;
            case IRevertable revertable:
                {
                    if (revertable.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                        revertable.SaveToDatabase();
                    break;
                }
            default:
                throw new NotSupportedException("Why is user editing something that isn't IRevertable?");
        }
    }

    #region Having

    private void HavingTextChanged(object sender, EventArgs e)
    {
        _aggregate.HavingSQL = _queryHaving.Text;
    }

    private void PopulateHavingText()
    {
        if (_queryHaving == null)
        {
            var querySyntaxHelper = _aggregate.GetQuerySyntaxHelper();

            _queryHaving = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory(), SyntaxLanguage.SQL,
                querySyntaxHelper);
            _queryHaving.TextChanged += HavingTextChanged;
            gbHaving.Controls.Add(_queryHaving);

            var autoComplete = new AutoCompleteProviderWin(querySyntaxHelper);
            autoComplete.RegisterForEvents(_queryHaving);
            autoComplete.Add(_aggregate);
        }

        _queryHaving.Text = _aggregate.HavingSQL;
    }

    #endregion

    #region Pivot

    private void PopulatePivotDropdown(AggregateDimension axisIfAny, AggregateDimension pivotIfAny)
    {
        ddPivotDimension.Items.Clear();

        var dimensions = _aggregate.AggregateDimensions;

        //if there's an axis
        if (axisIfAny != null &&
            !axisIfAny.Equals(pivotIfAny)) //<- if this second thing is the case then the graph is totally messed up!
            dimensions = dimensions.Except(new[] { axisIfAny }).ToArray(); //don't offer the axis as a pivot dimension!

        //don't let them pivot on a date, that's just a bad idea
        ddPivotDimension.Items.AddRange(dimensions.Where(d => !d.IsDate()).ToArray());

        if (pivotIfAny != null)
            ddPivotDimension.SelectedItem = pivotIfAny;
    }

    private void ddPivotDimension_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isRefreshing)
            return;

        if (ddPivotDimension.SelectedItem is AggregateDimension dimension && _aggregate != null)
        {
            EnsureCountHasAlias();
            EnsurePivotHasAlias(dimension);

            _aggregate.PivotOnDimensionID = dimension.ID;
            _aggregate.SaveToDatabase();
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
        }

        PublishToSelfOnly();
    }

    private static void EnsurePivotHasAlias(AggregateDimension dimension)
    {
        if (!string.IsNullOrWhiteSpace(dimension.Alias)) return;

        dimension.Alias = dimension.GetRuntimeName();
        dimension.SaveToDatabase();
    }

    private void EnsureCountHasAlias()
    {
        _querySyntaxHelper.SplitLineIntoSelectSQLAndAlias(_aggregate.CountSQL, out var col, out var alias);

        if (string.IsNullOrWhiteSpace(alias))
            _aggregate.CountSQL = $"{col}{_querySyntaxHelper.AliasPrefix} MyCount";
    }

    private void btnClearPivotDimension_Click(object sender, EventArgs e)
    {
        if (_aggregate == null) return;

        _aggregate.PivotOnDimensionID = null;
        ddPivotDimension.SelectedItem = null;

        if (sender.GetType() != typeof(Button) && (Button)sender != btnClearPivotDimension) return;

        _aggregate.SaveToDatabase();
        Publish();
    }

    #endregion

    private void PopulateAxis(AggregateDimension axisIfAny, AggregateDimension pivotIfAny)
    {
        var allDimensions = _aggregate.AggregateDimensions.ToArray();

        //if there's a pivot then don't advertise that as an axis
        if (pivotIfAny != null && !pivotIfAny.Equals(axisIfAny))
            allDimensions = allDimensions.Except(new[] { pivotIfAny }).ToArray();

        ddAxisDimension.Items.Clear();
        ddAxisDimension.Items.AddRange(allDimensions.Where(d => d.IsDate()).ToArray());

        //should only be one
        var axisDimensions = allDimensions.Where(d => d.AggregateContinuousDateAxis != null).ToArray();

        if (axisDimensions.Length > 1)
            if (Activator.YesNo(
                    $"Aggregate {_aggregate} has more than 1 dimension, this is highly illegal, shall I delete all the axis configurations for you?",
                    "Delete all axis?"))
                foreach (var a in axisDimensions)
                    a.AggregateContinuousDateAxis.DeleteInDatabase();
            else
                return;

        if (axisIfAny == null)
        {
            aggregateContinuousDateAxisUI1.Dimension = null;
            return;
        }

        ddAxisDimension.SelectedItem = axisIfAny;
        aggregateContinuousDateAxisUI1.Dimension = axisIfAny;

        if (!axisIfAny.IsDate())
            _errorProviderAxis.SetError(ddAxisDimension, "Column is not a DateTime");
        else
            _errorProviderAxis.Clear();
    }

    private void ddAxisDimension_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isRefreshing)
            return;

        if (ddAxisDimension.SelectedItem is not AggregateDimension selectedDimension)
            return;

        //is there already an axis? if so keep the old start/end dates
        var existing = _aggregate.GetAxisIfAny();

        //create a new one
        var axis = new AggregateContinuousDateAxis(Activator.RepositoryLocator.CatalogueRepository, selectedDimension)
        {
            AxisIncrement = AxisIncrement.Month
        };

        //copy over old values of start/end/increment
        if (existing != null && existing.AggregateDimension_ID != selectedDimension.ID)
        {
            axis.StartDate = existing.StartDate;
            axis.EndDate = existing.EndDate;
            axis.AxisIncrement = existing.AxisIncrement;
            existing.DeleteInDatabase();
        }

        axis.SaveToDatabase();
        PublishToSelfOnly();
    }


    private void btnClearAxis_Click(object sender, EventArgs e)
    {
        var existing = _aggregate.GetAxisIfAny();
        existing?.DeleteInDatabase();

        //also clear the pivot
        btnClearPivotDimension_Click(this, e);

        _errorProviderAxis.Clear();
        PublishToSelfOnly();
    }

    private bool _isRefreshing;

    public override void SetDatabaseObject(IActivateItems activator, AggregateConfiguration databaseObject)
    {
        _aggregate = databaseObject;

        base.SetDatabaseObject(activator, databaseObject);

        try
        {
            _querySyntaxHelper = databaseObject.GetQuerySyntaxHelper();
        }
        catch (AmbiguousDatabaseTypeException e)
        {
            activator.KillForm(ParentForm, e);
            return;
        }

        _isRefreshing = true;

        //find out what is legal for the aggregate
        _options = AggregateBuilderOptionsFactory.Create(_aggregate);

        //set enablednesss based on legality
        cbExtractable.Enabled = _options.ShouldBeEnabled(AggregateEditorSection.Extractable, _aggregate);
        cbExtractable.Checked = _aggregate.IsExtractable;
        ddPivotDimension.Enabled = _options.ShouldBeEnabled(AggregateEditorSection.PIVOT, _aggregate);
        gbAxis.Enabled = _options.ShouldBeEnabled(AggregateEditorSection.AXIS, _aggregate);

        //add included/excluded dimensions
        selectColumnUI1.SetUp(Activator, _options, _aggregate);

        tbID.Text = _aggregate.ID.ToString();

        SetNameText();

        DetermineFromTables();

        PopulateHavingText();

        var axisIfAny = _aggregate.GetAxisIfAny();
        var axisDimensionIfAny = axisIfAny?.AggregateDimension;
        var pivotIfAny = _aggregate.PivotDimension;

        PopulatePivotDropdown(axisDimensionIfAny, pivotIfAny);

        PopulateAxis(axisDimensionIfAny, pivotIfAny);

        PopulateTopX();

        if (databaseObject.IsCohortIdentificationAggregate)
        {
            var cic = databaseObject.GetCohortIdentificationConfigurationIfAny();
            if (cic != null)
                CommonFunctionality.AddToMenu(new ExecuteCommandActivate(activator, cic), "Open Cohort Query...");
        }
        else
        {
            CommonFunctionality.AddToMenu(new ExecuteCommandShow(activator, databaseObject.Catalogue, 0, true));
        }

        CommonFunctionality.Add(new ExecuteCommandExecuteAggregateGraph(activator, databaseObject));
        CommonFunctionality.Add(new ExecuteCommandViewData(activator, databaseObject));

        CommonFunctionality.AddChecks(databaseObject);
        CommonFunctionality.StartChecking();

        //enforcing the naming convention on cic aggregates can result in ObjectSaverButton incorrectly getting enabled
        GetObjectSaverButton()?.Enable(false);

        tbName.Enabled = !ReadOnly;
        _queryHaving.ReadOnly = ReadOnly;
        olvJoin.Enabled = !ReadOnly;
        selectColumnUI1.Enabled = !ReadOnly;

        _isRefreshing = false;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        selectColumnUI1.SetItemActivator(activator);
    }


    private void tbName_TextChanged(object sender, EventArgs e)
    {
        _aggregate.Name = tbName.Text;

        var cic = _aggregate.GetCohortIdentificationConfigurationIfAny();

        cic?.EnsureNamingConvention(_aggregate);
    }

    private void olvJoin_ItemActivate(object sender, EventArgs e)
    {
        if (olvJoin.SelectedObject is TableInfo t)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(t));
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AggregateEditor_Design, UserControl>))]
public abstract class AggregateEditor_Design : RDMPSingleDatabaseObjectControl<AggregateConfiguration>
{
}