using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.AutoComplete;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace CatalogueManager.AggregationUIs.Advanced
{
    /// <summary>
    /// Allows you to pick which columns are used to build an AggregateConfiguration.  This includes all AggregateDimensions (and the count column) as well as all available columns which could
    /// be included.  This UI handles column selection / editting for both regular Aggregate Graphs, Cohort Sets and Patient Index Tables (because they are all actually just AggregateConfiguration
    /// objects anyway). 
    /// 
    /// Ticking a column includes it in the configuration, unticking it deletes it.  If you have ticked an ExtractionInformation it will become an AggregateDimension which means when you change it's
    /// SQL implementation it will not affect the main extraction implementation.  This means that if you tick a column and modify it then untick it you will loose the changes.
    /// 
    /// The count column appears in Wheat background color and can be modified to any GROUP BY aggregate function e.g. max(dt)
    /// </summary>
    public partial class SelectColumnUI : RDMPUserControl
    {
        private IAggregateEditorOptions _options;
        private AggregateConfiguration _aggregate;
        private IActivateItems _activator;

        private List<IColumn> _availableColumns;
        private List<IColumn> _includedColumns;

        public SelectColumnUI()
        {
            InitializeComponent();
            
            olvSelectColumns.ButtonClick += ButtonClick;
            
            _availableColumns = new List<IColumn>();
            _includedColumns = new List<IColumn>();

            olvEditInPopup.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
            olvEditInPopup.AspectGetter = rowObject => _includedColumns.Contains(rowObject)?"Edit...":null;
            
            olvIncluded.AspectGetter = rowObject => _includedColumns.Contains(rowObject)? "Included": "Not Included";
            olvSelectColumns.AlwaysGroupByColumn = olvIncluded;
            olvSelectColumns.RowFormatter += RowFormatter;

            olvSelectColumns.CellEditStarting += CellEditStarting;
            olvSelectColumns.CellEditFinished += CellEditFinished;

            olvAddRemove.ImageGetter += ImageGetter;
            olvSelectColumns.CellClick += olvSelectColumns_CellClick;
            
            _add = FamFamFamIcons.add;
            _delete = FamFamFamIcons.delete;

        }

        void olvSelectColumns_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Column == olvAddRemove)
            {
                var countColumn = e.Model as AggregateCountColumn;
                var importableColumn = e.Model as ExtractionInformation;
                var dimensionColumn = e.Model as AggregateDimension;

                if (countColumn == null && importableColumn == null && dimensionColumn == null)
                    throw new Exception("Object in list view of type that wasn't IColumn, it was " + e.Model.GetType().Name);

                //if it is an add
                if (_availableColumns.Contains(e.Model))
                {
                    //count column added
                    if (countColumn != null)
                        if (_options.GetCountColumnRequirement(_aggregate) == CountColumnRequirement.CannotHaveOne)
                        {
                            WideMessageBox.Show("Cohort Sets cannot have Count columns");
                        }
                        else
                            Save(countColumn);

                    //regular column added
                    if (importableColumn != null)
                    {

                        //if it's a normal aggregate then don't let the user have more than 2 columns
                        if (!_aggregate.IsCohortIdentificationAggregate && _includedColumns.OfType<AggregateDimension>().Count() >= 2)
                        {
                            WideMessageBox.Show("You can only have a maximum of 2 columns in any graph (plus a count)");
                            return;
                        }

                        var dimension = new AggregateDimension(RepositoryLocator.CatalogueRepository, importableColumn, _aggregate);

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

                        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
                    }


                    //user is trying to remove a dimension
                    if (dimensionColumn != null)
                    {

                        //get the master it was based on
                        var extractionInformation = dimensionColumn.ExtractionInformation;
                        try
                        {
                            if (dimensionColumn.ID == _aggregate.PivotOnDimensionID)
                            {
                                _aggregate.PivotOnDimensionID = null;
                                _aggregate.SaveToDatabase();
                            }

                            //delete it in the database
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

        private object ImageGetter(object rowObject)
        {
            if (_availableColumns.Contains(rowObject))
                return _add;

            return _delete;
        }

        private void CellEditFinished(object sender, CellEditEventArgs cellEditEventArgs)
        {
            var col = (IColumn) cellEditEventArgs.RowObject;

            //user deleted it's value
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
            var col = olvItem.RowObject as IColumn;

            if (col != null)
            {
                olvItem.BackColor = col is AggregateCountColumn ? Color.Wheat : Color.White;
                olvItem.ForeColor = _availableColumns.Contains(col) ? Color.Gray : Color.Black;
            }
        }

        private void ButtonClick(object sender, CellClickEventArgs cellClickEventArgs)
        {
            if(cellClickEventArgs.Column == olvEditInPopup)
            {
                var col = cellClickEventArgs.Model as IColumn;
                if(col != null)
                {
                    var dialog = new SetSQLDialog(col.SelectSQL, new RDMPCommandFactory());

                    var hasDependencies = col as IHasDependencies ?? _aggregate; //if col isn't IHasDependencies then it's count col presumably? use the aggregate for autocomplete

                    var autoComplete = new AutoCompleteProviderFactory(_activator).Create(hasDependencies);
                    autoComplete.RegisterForEvents(dialog.QueryEditor);

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        col.SelectSQL = dialog.Result;
                        Save(col);
                    }
                }
            }
        }

        /// <summary>
        /// saves changes to an IColumn made by the user and publishes that the AggregateConfiguration has changed
        /// </summary>
        /// <param name="col"></param>
        private void Save(IColumn col)
        {  
            var countCol = col as AggregateCountColumn;

            if (countCol != null)
            {
                _aggregate.CountSQL = countCol.GetFullSelectLineStringForSavingIntoAnAggregate();
                _aggregate.SaveToDatabase();
                if(!_includedColumns.Contains(countCol))
                {
                    _includedColumns.Add(countCol);
                    _availableColumns.Remove(countCol);

                    olvSelectColumns.RemoveObject(countCol);
                    olvSelectColumns.AddObject(countCol);
                    olvSelectColumns.EnsureModelVisible(countCol);
                }
                
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_aggregate));
                return;
            }

            var saveable = col as ISaveable;
            if (saveable != null)
                saveable.SaveToDatabase();

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_aggregate));
        }


        private bool _alreadySetup = false;
        private Bitmap _add;
        private Bitmap _delete;

        public void SetUp(IActivateItems activator, IAggregateEditorOptions options, AggregateConfiguration aggregate)
        {
            //record new states so we don't accidentally erase names of stuff
            _activator = activator;
            _options = options;
            _aggregate = aggregate;

            if(_alreadySetup)
                return;
            
            _availableColumns.Clear();
            _includedColumns.Clear();
            olvSelectColumns.ClearObjects();

            _availableColumns.AddRange(_options.GetAvailableSELECTColumns(_aggregate));
            _includedColumns.AddRange(_aggregate.AggregateDimensions);

            //add count option unless it cannot have one
            if (_options.GetCountColumnRequirement(_aggregate) != CountColumnRequirement.CannotHaveOne)
                if (!string.IsNullOrEmpty(_aggregate.CountSQL))
                    _includedColumns.Add(new AggregateCountColumn(_aggregate.CountSQL));
                else
                    _availableColumns.Add(new AggregateCountColumn("count(*) as CountColumn"));

            olvSelectColumns.AddObjects(_includedColumns);
            olvSelectColumns.AddObjects(_availableColumns);

            _alreadySetup = true;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvSelectColumns.UseFiltering = true;
            olvSelectColumns.ModelFilter = new TextMatchFilterWithWhiteList(_includedColumns,olvSelectColumns,tbFilter.Text,StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
