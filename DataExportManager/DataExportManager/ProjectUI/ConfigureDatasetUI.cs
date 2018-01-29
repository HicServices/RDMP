using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableUIComponents;

using ScintillaNET;
using Clipboard = System.Windows.Forms.Clipboard;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to choose which columns you want to extract from a given dataset (Catalogue) for a specific research project extraction.  For example Researcher A wants prescribing
    /// dataset including all the Core columns but he also has obtained governance approval to receive Supplemental column 'PrescribingGP' so the configuration would need to include this
    /// column.
    /// 
    /// On the left you can see all the available columns and transforms in the selected dataset (see ExtractionConfigurationUI for selecting datasets).  You can add these by selecting them
    /// and pressing the '>' button.  On the right the QueryBuilder will show you what the extraction SQL will be for the dataset when it is executed.  
    /// 
    /// Depending on which columns you have selected the QueryBuilder may be unable to generate a query (for example if you do not add the IsExtractionIdentifier column - See 
    /// ExtractionInformationUI).
    /// 
    /// You can click the 'Filters' button to configure extraction filters for the dataset.  For example a researcher might only have governance approval to receive prescriptions for 
    /// specific drugs relevant to his research question and not all prescriptions.  This launches a DeployedExtractionFilterUI. 
    /// </summary>
    public partial class ConfigureDatasetUI : ConfigureDatasetUI_Design
    {
        public SelectedDataSets SelectedDataSet { get; private set; }
        private ExtractableDataSet _dataSet;
        private ExtractionConfiguration _config;

        private List<ExtractionInformation> ColumnsFromCatalogue = new List<ExtractionInformation>();
        private List<ConcreteColumn> ColumnsAlreadyInConfiguration = new List<ConcreteColumn>();

        private bool loading = false;
        
        //constructor
        public ConfigureDatasetUI()
        {
            InitializeComponent();

            if (VisualStudioDesignMode)
                return;
        }

        
        /// <summary>
        /// The left list contains ExtractionInformation from the Data Catalogue, this is columns in the database which could be extracted
        /// The right list contains ExtractableColumn which is a more advanced class that contains runtime configurations such as order to be outputed in etc.
        /// </summary>
        public void RefreshUIFromDatabase()
        {
            if(RepositoryLocator == null)
                return;
            
            SetupUserInterface();
        }

        private void SetupUserInterface()
        {
            //clear the UI
            lbAvailableColumns.Items.Clear();
            lbSelectedColumns.Items.Clear();
            ColumnsAlreadyInConfiguration.Clear();
            ColumnsFromCatalogue.Clear();

            //get the catalogue and then all the items
            ICatalogue cata;
            try
            {
                cata = _dataSet.Catalogue;
            }
            catch (Exception e)
            {
                //catalogue has probably been deleted!
                ExceptionViewer.Show("Unable to find Catalogue for ExtractableDataSet",e);
                return;
            }

            //then get all the extractable columns from each item (some items have multiple extractable columns)
            ColumnsFromCatalogue.AddRange(cata.GetAllExtractionInformation(ExtractionCategory.Core));

            if (cbShowSupplemental.Checked)
                ColumnsFromCatalogue.AddRange(cata.GetAllExtractionInformation(ExtractionCategory.Supplemental));

            if (cbShowSpecialApproval.Checked)
                ColumnsFromCatalogue.AddRange(cata.GetAllExtractionInformation(ExtractionCategory.SpecialApprovalRequired));

            if (cbShowDeprecatedColumns.Checked)
                ColumnsFromCatalogue.AddRange(cata.GetAllExtractionInformation(ExtractionCategory.Deprecated));

            //sort it (get's default order)
            ColumnsFromCatalogue.Sort();

            var allExtractableColumns = _config.GetAllExtractableColumnsFor(_dataSet);

            //now get all the ExtractableColumns that are already configured for this configuration (previously)
            ColumnsAlreadyInConfiguration.AddRange(allExtractableColumns);

            ColumnsAlreadyInConfiguration.Sort();

            //add the stuff they have selected and configured into the UI
            foreach (ConcreteColumn item in ColumnsAlreadyInConfiguration)
            {
                if (string.IsNullOrWhiteSpace(item.SelectSQL))
                    if (DialogResult.Yes ==
                        MessageBox.Show("ExtractableColumn ID=" + item.ID + " has no extraction text, delete it?",
                                        "Broken Extraction Information", MessageBoxButtons.YesNo))
                    {
                        item.DeleteInDatabase(); //column is broken so delete it
                        RefreshUIFromDatabase(); //refresh the whole UI after the delete 
                        return;
                    }
                    else
                        continue; //column is broken so don't add it

                lbSelectedColumns.Items.Add(item);
            }

            //add the potential stuff (in the catalogue) that they could choose from
            foreach (ExtractionInformation info in ColumnsFromCatalogue)
            {
                //don't add stuff that we are already using
                if (!IsAlreadySelected(info))
                    lbAvailableColumns.Items.Add(info);
            }

            //add the stuff that is in the cohort table so they can pick these too
            if (_config.Cohort_ID != null)
            {
                var cohort = _config.Cohort;

                try
                {
                    foreach (var cohortCustomColumn in cohort.CustomCohortColumns)
                        if (
                            !IsAlreadySelected(cohortCustomColumn) &&

                            //if the column has the same name as CHI (or whatever the private field is then don't add it)
                            !cohortCustomColumn.GetRuntimeName().ToLower().EndsWith(cohort.GetPrivateIdentifier(true).ToLower())

                            //if the show custom columns is checked
                            && cbShowCohortColumns.Checked
                            )
                            //it can be legally added because its not selected or private
                            lbAvailableColumns.Items.Add(cohortCustomColumn);
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show("Error occurred while trying to enumerate the custom cohort columns:" + e.Message, e);
                }
            }

            
        }


        /// <summary>
        /// Determines whether this potential extractable column (identified by the catalogue) is already selected and configured
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsAlreadySelected(IColumn info)
        {
            //compare custom columns on select sql
            if (info is CohortCustomColumn)
                return ColumnsAlreadyInConfiguration.Any(ec => ec.SelectSQL == info.SelectSQL);
            
            
            //compare regular columns on their ID in the catalogue
            return ColumnsAlreadyInConfiguration.OfType<ExtractableColumn>().Any(ec => ec.CatalogueExtractionInformation_ID == info.ID);
        }

    

        private void TryToPopulateSelectedColumnsListBoxWithTheFollowingColumnNames(string[] userDesiredColumns)
        {
            string couldNotFindColumns = "";
            int couldFind = 0;

            foreach (string toAdd in userDesiredColumns)
            {
                bool found = false;

                for (int i = 0; i < lbAvailableColumns.Items.Count;i++ )
                {
                    if (IsFuzzyMatch(lbAvailableColumns.Items[i] as ExtractionInformation,toAdd))
                    {
                        AddColumnToExtraction(lbAvailableColumns.Items[i] as ExtractionInformation);
                        found = true;
                        couldFind++;
                        break;
                    }
                }

                if (!found)
                    couldNotFindColumns += toAdd + Environment.NewLine;
            }

            MessageBox.Show("Found " + couldFind + "/" + userDesiredColumns.Length + ", could not find:" +
                            Environment.NewLine + couldNotFindColumns);

        }

        private bool IsFuzzyMatch(ExtractionInformation info, string toMatch)
        {
            char[] thingsToTrimOff = new char[] {',', '\n', '\r'};

            toMatch = toMatch.TrimEnd(thingsToTrimOff);
            try
            {
                var syntax = new MicrosoftQuerySyntaxHelper();

                return info.GetRuntimeName().Equals(syntax.GetRuntimeName(toMatch));
            }
            catch (Exception)
            {
                //something was text that couldn't result in a runtime name
                return false;
            }
        }

        /// <summary>
        /// The user has selected an extractable thing in the catalogue and opted to include it in the extraction
        /// So we have to convert it to an ExtractableColumn (which has configuration specific stuff - and lets
        /// data analyst override stuff for this extraction only)
        /// 
        /// Then add it to the right hand list
        /// </summary>
        /// <param name="item"></param>
        private void AddColumnToExtraction(IColumn item)
        {
            var addMe = _config.AddColumnToExtraction(_dataSet,item);

            ColumnsAlreadyInConfiguration.Add(addMe);

            int i;
            //work where to add it to the listbox
            for (i = 0; i < lbSelectedColumns.Items.Count; i++)
            {
                ConcreteColumn c = (ConcreteColumn)lbSelectedColumns.Items[i];
                if (c.Order <= addMe.Order)
                    continue;

                break;
            }

            lbSelectedColumns.Items.Insert(i,addMe);
        }

        
        private void btnInclude_Click(object sender, EventArgs e)
        {
            
            foreach (IColumn item in lbAvailableColumns.SelectedItems)
                AddColumnToExtraction(item);

            //remove from left box
            for (int i = lbAvailableColumns.SelectedIndices.Count - 1; i >= 0; i--)
                lbAvailableColumns.Items.RemoveAt(lbAvailableColumns.SelectedIndices[i]);

            SaveColumnPositions();
            RefreshUIFromDatabase();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
        }

        private void btnExclude_Click(object sender, EventArgs e)
        {
            if (lbSelectedColumns.SelectedItem != null)
            {
                RemoveColumnFromExtraction(lbSelectedColumns.SelectedItem as ConcreteColumn);
                lbSelectedColumns.Items.Remove(lbSelectedColumns.SelectedItem);
                RefreshUIFromDatabase();
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
            }

        }
        
        private void btnExcludeAll_Click(object sender, EventArgs e)
        {
            for (int i = lbSelectedColumns.Items.Count - 1; i >= 0; i--)
            {
                RemoveColumnFromExtraction(lbSelectedColumns.Items[i] as ConcreteColumn);
            }

            RefreshUIFromDatabase();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
        }

        private void RemoveColumnFromExtraction(ConcreteColumn concreteColumn)
        {
            if (concreteColumn != null)
                concreteColumn.DeleteInDatabase();
        }

        private void SaveColumnPositions()
        {
            int position = 0;
            foreach (ConcreteColumn col in lbSelectedColumns.Items)
            {
                col.Order = position++;
                col.SaveToDatabase();
            }
        }


        private void lbSelectedColumns_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Control)
            {
                //the user is trying to paste in a list of headers, try to find them
                string toPaste = Clipboard.GetText();

                string[] userDesiredColumns = toPaste.Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

                TryToPopulateSelectedColumnsListBoxWithTheFollowingColumnNames(userDesiredColumns);

                SaveColumnPositions();

                RefreshUIFromDatabase();
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
            }
        }

        
        #region Drag and Drop reordering
        private void lbSelectedColumns_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.lbSelectedColumns.SelectedItem == null) return;
            this.lbSelectedColumns.DoDragDrop(this.lbSelectedColumns.SelectedItem, DragDropEffects.Move);
        }

        
        private Point draggingOldLeftPoint;
        private Point draggingOldRightPoint;
        
        private void lbSelectedColumns_DragOver(object sender, DragEventArgs e)
        {

            e.Effect = DragDropEffects.Move;
            int idxHoverOver = lbSelectedColumns.IndexFromPoint(lbSelectedColumns.PointToClient(new Point(e.X, e.Y)));

            Graphics g = lbSelectedColumns.CreateGraphics();

            int top = lbSelectedColumns.Font.Height * idxHoverOver;

            top += lbSelectedColumns.AutoScrollOffset.Y;

            //this seems to count up the number of items that have been skipped rather than the pixels... wierdo crazy
            int barpos = NativeMethods.GetScrollPos(lbSelectedColumns.Handle, Orientation.Vertical);
            barpos *= lbSelectedColumns.Font.Height;
            top -= barpos;


            //calculate where we should be drawing our horizontal line
            Point left = new Point(0, top);
            Point right = new Point(lbSelectedColumns.Width, top);

            //draw over the old one in the background colour (incase it has moved) - we don't want to leave trails
            g.DrawLine(new System.Drawing.Pen(lbSelectedColumns.BackColor, 2), draggingOldLeftPoint, draggingOldRightPoint);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), left, right);

            draggingOldLeftPoint = left;
            draggingOldRightPoint = right;

        }

        private void lbSelectedColumns_DragDrop(object sender, DragEventArgs e)
        {
            Point point = lbSelectedColumns.PointToClient(new Point(e.X, e.Y));
            int index = this.lbSelectedColumns.IndexFromPoint(point);

            //if they are dragging it way down the bottom of the list
            if (index < 0)
                index = this.lbSelectedColumns.Items.Count;

            var formats = e.Data.GetFormats();

            if(formats == null ||formats.Length == 0)
                return;

            //get the thing they are dragging
            object data = e.Data.GetData(formats[0]);

            //find original index because if we are dragging down then we will want to adjust the index so that insert point is correct even after removing the object further up the list
            int originalIndex = this.lbSelectedColumns.Items.IndexOf(data);

            this.lbSelectedColumns.Items.Remove(data);

            if (originalIndex < index)
                this.lbSelectedColumns.Items.Insert(Math.Max(0, index - 1), data);
            else
                this.lbSelectedColumns.Items.Insert(index, data);

            SaveColumnPositions();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(SelectedDataSet));
        }


        #endregion

        
        private void btnSelectCore_Click(object sender, EventArgs e)
        {
            for(int i=0;i<lbAvailableColumns.Items.Count;i++)
            {
                ExtractionInformation column = lbAvailableColumns.Items[i] as ExtractionInformation;

                //could be a Cohort column! so check for null
                if (column != null)
                    if(column.ExtractionCategory == ExtractionCategory.Core)
                        lbAvailableColumns.SetSelected(i,true);
            }
        }

        private void cbAnyShowFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RefreshUIFromDatabase();
        }

        private void lbAvailableColumns_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;

            // draw the background color you want
            g.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);

            if (e.Index != -1)
                if (lbAvailableColumns.Items[e.Index] is CohortCustomColumn)
                {
                    CohortCustomColumn toDisplay = lbAvailableColumns.Items[e.Index] as CohortCustomColumn;
                    
                    g.DrawString(toDisplay.ToString(), e.Font, new SolidBrush(Color.Blue), new PointF(e.Bounds.X, e.Bounds.Y));
                }
                else
                {
                    ExtractionInformation toDisplay = lbAvailableColumns.Items[e.Index] as ExtractionInformation;

                    string textToDisplay = toDisplay.ToString();

                    if (toDisplay.ExtractionCategory == ExtractionCategory.Core)
                        g.DrawString(textToDisplay, e.Font, new SolidBrush(Color.Green), new PointF(e.Bounds.X, e.Bounds.Y));
                    else if (toDisplay.ExtractionCategory == ExtractionCategory.Supplemental)
                        g.DrawString(textToDisplay, e.Font, new SolidBrush(Color.Orange),
                                        new PointF(e.Bounds.X, e.Bounds.Y));
                    else if (toDisplay.ExtractionCategory == ExtractionCategory.Deprecated)
                        g.DrawString(textToDisplay, e.Font, new SolidBrush(Color.Red),
                                        new PointF(e.Bounds.X, e.Bounds.Y));
                    else if (toDisplay.ExtractionCategory == ExtractionCategory.SpecialApprovalRequired)
                        g.DrawString(textToDisplay, e.Font, new SolidBrush(Color.Tan),
                                        new PointF(e.Bounds.X, e.Bounds.Y));
                    
                }
        }

        private void lbSelectedColumns_DrawItem(object sender, DrawItemEventArgs e)
        {
             Graphics g = e.Graphics;

            // draw the background color you want
            g.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);

            if (e.Index != -1)
            {
                var col = (ExtractableColumn)lbSelectedColumns.Items[e.Index];
                g.DrawString(col.ToString(), e.Font, new SolidBrush(col.HasOriginalExtractionInformationVanished()?Color.Red:Color.Black), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }
        
        public override void SetDatabaseObject(IActivateItems activator, SelectedDataSets databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            SelectedDataSet = databaseObject;
            _dataSet = SelectedDataSet.ExtractableDataSet;
            _config = SelectedDataSet.ExtractionConfiguration;
            RefreshUIFromDatabase();
            
        }

        public override string GetTabName()
        {
            return "Edit" + base.GetTabName();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConfigureDatasetUI_Design, UserControl>))]
    public abstract class ConfigureDatasetUI_Design : RDMPSingleDatabaseObjectControl<SelectedDataSets>
    {
        
    }
}
