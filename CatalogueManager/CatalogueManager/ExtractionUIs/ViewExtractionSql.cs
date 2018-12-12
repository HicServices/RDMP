using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
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
            
            #region Query Editor setup


            QueryPreview = new ScintillaTextEditorFactory().Create();
            QueryPreview.ReadOnly = true;

            this.scRadioButtonsSqlSplit.Panel2.Controls.Add(QueryPreview);
            bLoading = false;

            #endregion

            AssociatedCollection = RDMPCollection.Catalogue;

            olvColumn1.ImageGetter += ImageGetter;
        }

        private object ImageGetter(object rowObject)
        {
            return _activator.CoreIconProvider.GetImage(rowObject);
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
        private bool _firstTime=true;

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

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogue = databaseObject;
            RefreshUIFromDatabase(); 

            toolStrip1.Items.Clear();
            Add(toolStrip1,new ExecuteCommandReOrderColumns(_activator, _catalogue));
        }
        
        public override string GetTabName()
        {
            return base.GetTabName() + "(SQL)";
        }

        private void olvExtractionInformations_ItemActivate(object sender, EventArgs e)
        {
            var o = olvExtractionInformations.SelectedObject as IMapsDirectlyToDatabaseTable;
            if(o != null)
                _activator.RequestItemEmphasis(this,new EmphasiseRequest(o){ExpansionDepth = 1});
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewExtractionSql_Design, UserControl>))]
    public abstract class ViewExtractionSql_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
