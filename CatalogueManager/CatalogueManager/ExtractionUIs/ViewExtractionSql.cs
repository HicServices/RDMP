using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.ExtractionUIs
{
    /// <summary>
    /// Allows you to view the Extraction SQL that is built by the QueryBuilder when extracting or running data quality engine against a dataset (Catalogue).  Includes options for
    /// you to display only Core extraction fields or also supplemental / special approval.
    /// 
    /// <para>If you have an ExtractionFilters configured on your Catalogue then you can tick them to view their SQL implementation.  Because these are master filters and this dialog 
    /// is for previewing only, no AND/OR container trees are included in the WHERE logic (See ExtractionFilterUI for more info about filters).</para>
    /// 
    /// <para>If for some reason you see an error instead of your extraction SQL then read the description and take the steps it suggests (e.g. if it is complaining about not knowing
    /// how to JOIN two tables then configure an appropriate JoinInfo - See JoinConfiguration). </para>
    /// </summary>
    public partial class ViewExtractionSql : ViewExtractionSql_Design
    {
        private Catalogue _catalogue;
        
        private Scintilla QueryPreview;
        public ViewExtractionSql()
        {
            InitializeComponent();
            
            olvExtractionInformations.RowFormatter+= RowFormatter;
            
            #region Query Editor setup


            QueryPreview = new ScintillaTextEditorFactory().Create();
            QueryPreview.ReadOnly = true;

            this.scRadioButtonsSqlSplit.Panel2.Controls.Add(QueryPreview);
            bLoading = false;

            #endregion

            AssociatedCollection = RDMPCollection.Catalogue;
        }

        private void RowFormatter(OLVListItem olvItem)
        {
            var ei = (ExtractionInformation)olvItem.RowObject;
            switch (ei.ExtractionCategory)
            {
                case ExtractionCategory.Core:
                case ExtractionCategory.ProjectSpecific:
                    olvItem.ForeColor = Color.Green;
                    break;
                case ExtractionCategory.Supplemental:
                    olvItem.ForeColor = Color.Orange;
                    break;
                case ExtractionCategory.SpecialApprovalRequired:
                    olvItem.ForeColor = Color.Tan;
                    break;
                case ExtractionCategory.Internal:
                    olvItem.ForeColor = Color.Red;
                    break;
                case ExtractionCategory.Deprecated:
                    olvItem.ForeColor = Color.OrangeRed;
                    break;
                    case ExtractionCategory.Any:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool bLoading = false;


        private void RefreshUIFromDatabase()
        {
            try
            {
                if (bLoading)
                    return;

                //only allow reordering when all are visible or only internal are visible otherwise user could select core only and do a reorder leaving supplemental columns as freaky orphans all down at the bottom fo the SQL!
                bLoading = true;

                List<ExtractionInformation> extractionInformations = new List<ExtractionInformation>();

                if (rbInternal.Checked)
                {
                    extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Internal));
                }
                else
                {
                    //always add the project specific ones
                    extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific));
                    extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Core));

                    if (rbSupplemental.Checked || rbCoreSupplementalAndSpecialApproval.Checked)
                        extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Supplemental));

                    if (rbCoreSupplementalAndSpecialApproval.Checked)
                        extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.SpecialApprovalRequired));

                }

                //sort by Default Order
                extractionInformations.Sort();
                
                //add to listbox
                olvExtractionInformations.ClearObjects();
                olvExtractionInformations.AddObjects(extractionInformations.ToArray());
                
                //add the available filters
                MemoriseListboxState();
                
                clbFilters.Items.Clear();
                foreach (var extractionInformation in extractionInformations)
                    clbFilters.Items.AddRange(extractionInformation.ExtractionFilters.ToArray());
                
                AttemptToRestoreListboxState();

                //generate SQL -- only make it readonly after setting the .Text otherwise it ignores the .Text setting even though it is programatical
                QueryPreview.ReadOnly = false;
                QueryPreview.Text = GenerateExtractionSQLForCatalogue(extractionInformations.ToArray());
                QueryPreview.ReadOnly = true;
            }
            catch (Exception ex)
            {
                QueryPreview.ReadOnly = false;
                QueryPreview.Text = ex.ToString();
                QueryPreview.ReadOnly = true;
            }
            finally
            {
                bLoading = false;
            }

        }
        private string GenerateExtractionSQLForCatalogue(ExtractionInformation[] extractionInformations)
        {
            QueryBuilder builder = new QueryBuilder(null,null);
            builder.AddColumnRange(extractionInformations);


            List<ExtractionFilter> filters = new List<ExtractionFilter>();

            foreach (ExtractionFilter f in clbFilters.CheckedItems)
                filters.Add(f);

            builder.RootFilterContainer = new SpontaneouslyInventedFilterContainer(null,filters.ToArray(),FilterContainerOperation.AND);
            return builder.SQL;
        }
        
        #region methods for reselecting / checking the listbox after database refresh
        private List<int> _wasCheckedFilterIDs = new List<int>();
        private int _wasSelectedFilterID;

        private void AttemptToRestoreListboxState()
        {
            for (int i = 0; i < clbFilters.Items.Count; i++)
            {
                ExtractionFilter item = (ExtractionFilter)clbFilters.Items[i];
                if (item.ID == _wasSelectedFilterID)
                    clbFilters.SelectedItem = item;

                if (_wasCheckedFilterIDs.Contains(item.ID))
                    clbFilters.SetItemChecked(i, true);
            }
        }


        private void MemoriseListboxState()
        {
            if (clbFilters.SelectedItem != null)
            {
                _wasSelectedFilterID = ((ExtractionFilter)clbFilters.SelectedItem).ID;
            }
            else
                _wasSelectedFilterID = -1;

            _wasCheckedFilterIDs.Clear();

            foreach (ExtractionFilter extractionFilter in clbFilters.CheckedItems)
                _wasCheckedFilterIDs.Add(extractionFilter.ID);

        }
        #endregion

        private void RadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            RefreshUIFromDatabase();
        }

        private void clbFilters_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(bLoading)
                return;

            this.BeginInvoke((MethodInvoker)RefreshUIFromDatabase);
        }
        private int _collapseCounter = 0;
        
        private void btnCollapse_Click(object sender, EventArgs e)
        {
            if (_collapseCounter == 0)
            {
                scFiltersAndColumns.Panel1Collapsed = false;
                scFiltersAndColumns.Panel2Collapsed = true;
            }
            if (_collapseCounter == 1)
            {
                scFiltersAndColumns.Panel1Collapsed = true;
                scFiltersAndColumns.Panel2Collapsed = false;
            }
            if (_collapseCounter == 2)
            {
                scFiltersAndColumns.Panel1Collapsed = false;
                scFiltersAndColumns.Panel2Collapsed = false;
            }

            _collapseCounter = (_collapseCounter + 1) %3;
        }

   

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogue = databaseObject;
            RefreshUIFromDatabase();
        }

        private void btnAdvancedReorder_Click(object sender, EventArgs e)
        {
            _activator.ActivateReOrderCatalogueItems(_catalogue);
        }

        public override string GetTabName()
        {
            return base.GetTabName() + "(SQL)";
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewExtractionSql_Design, UserControl>))]
    public abstract class ViewExtractionSql_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
