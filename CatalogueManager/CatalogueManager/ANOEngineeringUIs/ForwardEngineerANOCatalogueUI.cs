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
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.ANOEngineeringUIs
{
    public partial class ForwardEngineerANOCatalogueUI : ForwardEngineerANOCatalogueUI_Design
    {

        private bool _setup = false;
        private RDMPCollectionCommonFunctionality tlvANOTablesCommonFunctionality;
        private RDMPCollectionCommonFunctionality tlvTableInfoMigrationsCommonFunctionality;
        private ForwardEngineerANOCataloguePlanManager _planManager;

        public ForwardEngineerANOCatalogueUI()
        {
            InitializeComponent();
            serverDatabaseTableSelector1.HideTableComponents();

            olvMigrationPlan.AspectGetter += MigrationPlanAspectGetter;
            
            olvPickedANOTable.HeaderImageKey = "ANOTable";
            olvPickedANOTable.AspectGetter += PickedANOTableAspectGetter;
            olvPickedANOTable.ImageGetter += PickedANOTable_ImageGetter;

            olvDilution.HeaderImageKey = "PreLoadDiscardedColumn";
            olvDilution.AspectGetter += DilutionAspectGetter;
            olvDilution.ImageGetter += Dilution_ImageGetter;
            
            olvDestinationType.AspectGetter += DestinationTypeAspectGetter;
            
            tlvTableInfoMigrations.CellEditStarting += tlvTableInfoMigrations_CellEditStarting;
            tlvTableInfoMigrations.CellEditFinishing += tlvTableInfoMigrations_CellEditFinishing;

            tlvTableInfoMigrations.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
        }

        #region Aspect Getters and Setters

        private object MigrationPlanAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
                return _planManager.GetPlanForColumnInfo(col);

            return null;
        }

        private object PickedANOTable_ImageGetter(object rowObject)
        {
            var ci = rowObject as ColumnInfo;

            if (ci != null && _planManager.GetPlannedANOTable(ci) != null)
                return imageList1.Images["ANOTable"];

            return null;
        }

        private object PickedANOTableAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var ano = _planManager.GetPlannedANOTable(col);

                if (ano != null)
                    return ano.ToString();

                if (_planManager.GetPlanForColumnInfo(col) == ForwardEngineerANOCataloguePlanManager.Plan.ANO)
                    return "pick";
            }

            return null;
        }

        private object DilutionAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var dilution = _planManager.GetPlannedDilution(col);

                if (dilution != null)
                    return dilution;

                if (_planManager.GetPlanForColumnInfo(col) == ForwardEngineerANOCataloguePlanManager.Plan.Dillute)
                    return "pick";
            }

            return null;
        }
        private object Dilution_ImageGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                if (_planManager.GetPlannedDilution(col) != null)
                    return "PreLoadDiscardedColumn";
            }

            return null;
        }

        private object DestinationTypeAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;
            try
            {
                if (ci != null)
                    return _planManager.GetEndpointDataType(ci);
            }
            catch (Exception)
            {
                return "Error";
            }

            return null;
        }
        #endregion

        void tlvTableInfoMigrations_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is TableInfo)
                e.Cancel = true;

            if (e.Column == olvDestinationType)
                e.Cancel = true;

            var col = e.RowObject as ColumnInfo;

            if (e.Column == olvMigrationPlan)
                e.Control.Bounds = e.CellBounds;

            if (col != null && e.Column == olvPickedANOTable)
            {
                if(_planManager.GetPlanForColumnInfo(col) != ForwardEngineerANOCataloguePlanManager.Plan.ANO)
                {
                    e.Cancel = true;
                    return;
                }

                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.CoreChildProvider.AllANOTables, true, false);
                try
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        _planManager.SetPlannedANOTable(col, dialog.Selected as ANOTable);
                    
                    Check();
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }

                e.Cancel = true;
            }

            if (col != null && e.Column == olvDilution)
            {

                if (_planManager.GetPlanForColumnInfo(col) != ForwardEngineerANOCataloguePlanManager.Plan.Dillute)
                {
                    e.Cancel = true;
                    return;
                }

                var cbx = new ComboBox();
                cbx.DropDownStyle = ComboBoxStyle.DropDownList;
                cbx.Bounds = e.CellBounds;
                cbx.Items.AddRange(_planManager.DilutionOperations.ToArray());
                e.Control = cbx;
            }
        }
        
        void tlvTableInfoMigrations_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            try
            {
                var col = e.RowObject as ColumnInfo;

                if(e.Column == olvMigrationPlan)
                    _planManager.SetPlan(col,(ForwardEngineerANOCataloguePlanManager.Plan) e.NewValue);

                if(e.Column == olvDilution)
                {
                    var cbx = (ComboBox)e.Control;
                    _planManager.SetPlannedDilution(col,(IDilutionOperation)cbx.SelectedItem);
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

            Check();
        }
        
        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            if (!_setup)
            {
                _planManager = new ForwardEngineerANOCataloguePlanManager(databaseObject);

                //Set up tree view to show ANO Tables that are usable
                tlvANOTablesCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvANOTablesCommonFunctionality.SetUp(tlvANOTables,activator,olvANOTablesName,null,false,false);
                
                tlvANOTables.AddObject(activator.CoreChildProvider.AllANOTablesNode);
                tlvANOTables.ExpandAll();
                
                //Setup tree view to show all TableInfos that you are trying to Migrate
                tlvTableInfoMigrationsCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvTableInfoMigrationsCommonFunctionality.SetUp(tlvTableInfoMigrations,activator,olvTableInfoName,null,false,false);
                
                //don't display anything below ColumnInfo
                tlvTableInfoMigrationsCommonFunctionality.AxeChildren = new[] {typeof (ColumnInfo)};
                
                rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
                rdmpObjectsRibbonUI1.Add(databaseObject);
                _setup = true;
            }
            else
                _planManager.RefreshTableInfos();

            //Add them and expand them
            tlvTableInfoMigrations.ClearObjects();
            tlvTableInfoMigrations.AddObjects(_planManager.TableInfos);
            tlvTableInfoMigrations.ExpandAll();

            ddDateColumn.DataSource =_planManager.TableInfos.SelectMany(t => t.ColumnInfos).Where(c => c.Data_type != null && c.Data_type.Contains("date")).ToArray();

            Check();
        }

        private void Check()
        {
            ragSmiley1.StartChecking(_planManager);
        }

        private void tlvTableInfoMigrations_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            var ci = e.Model as ColumnInfo;

            if (ci != null && _planManager.IsMandatoryForMigration(ci))
                e.Item.BackColor = tbMandatory.BackColor;
        }

        private void tlvTableInfoMigrations_FormatCell(object sender, FormatCellEventArgs e)
        {
            if(e.Column == olvMigrationPlan)
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);

            if (e.CellValue as string == "pick")
            {
                e.SubItem.ForeColor = Color.Blue;
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
            }
        }

        private void btnRefreshChecks_Click(object sender, EventArgs e)
        {
            Check();
        }

        private void serverDatabaseTableSelector1_SelectionChanged()
        {
            _planManager.TargetDatabase = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            
            if (_planManager.TargetDatabase != null)
            {
                Exception ex;
                if(_planManager.TargetDatabase.Exists())
                {
                    _planManager.SkippedTables.Clear();

                    foreach (var t in _planManager.TableInfos)
                    {
                        var existing = _planManager.TargetDatabase.DiscoverTables(true);

                        //it is already migrated
                        if(existing.Any(e => e.GetRuntimeName().Equals(t.GetRuntimeName())))
                            _planManager.SkippedTables.Add(t);
                    }
                }
            }

            DisableObjects();
            
            Check();
        }

        private void DisableObjects()
        {
            List<object> toDisable = new List<object>();

            toDisable.AddRange(_planManager.SkippedTables);
            toDisable.AddRange(_planManager.SkippedTables.SelectMany(t=>t.ColumnInfos));

            tlvTableInfoMigrations.DisabledObjects = toDisable;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                var engine = new ForwardEngineerANOCatalogueEngine(_activator.RepositoryLocator.CatalogueRepository, _planManager);
                engine.Execute();

                if(engine.NewCatalogue != null)
                {

                    Publish(engine.NewCatalogue);

                    if(MessageBox.Show("Successfully created Catalogue '" + engine.NewCatalogue + "', close form?","Success",MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ParentForm.Close();
                }
                else
                    throw new Exception("Engine did not create a NewCatalogue...");
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void ddDateColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            _planManager.DateColumn = ddDateColumn.SelectedItem as ColumnInfo;
        }

        private void cbDateBasedLoad_CheckedChanged(object sender, EventArgs e)
        {
            ddDateColumn.Enabled = cbDateBasedLoad.Checked;
            tbStartDate.Enabled = cbDateBasedLoad.Checked;
            _planManager.DateColumn = cbDateBasedLoad.Checked ? ddDateColumn.SelectedItem as ColumnInfo : null;
            _planManager.StartDate = GetStartDate();
            Check();
        }

        private void tbStartDate_TextChanged(object sender, EventArgs e)
        {
            _planManager.StartDate = GetStartDate();
        }

        private DateTime? GetStartDate()
        {

            if (cbDateBasedLoad.Checked)
            {
                try
                {
                    var dt = DateTime.Parse(tbStartDate.Text);
                    tbStartDate.ForeColor = Color.Black;
                    return dt;
                }
                catch (Exception)
                {
                    tbStartDate.ForeColor = Color.Red;
                }
            }

            return null;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ForwardEngineerANOCatalogueUI_Design, UserControl>))]
    public abstract class ForwardEngineerANOCatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
