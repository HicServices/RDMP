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
using Microsoft.SqlServer.Management.Smo;
using ReusableUIComponents;

namespace CatalogueManager.ANOEngineeringUIs
{
    public partial class ForwardEngineerANOVersionOfCatalogueUI : ForwardEngineerANOVersionOfCatalogueUI_Design
    {

        private bool _setup = false;
        private RDMPCollectionCommonFunctionality tlvANOTablesCommonFunctionality;
        private RDMPCollectionCommonFunctionality tlvTableInfoMigrationsCommonFunctionality;
        private ForwardEngineerANOVersionOfCatalogue _migrator;

        public ForwardEngineerANOVersionOfCatalogueUI()
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
        }

        #region Aspect Getters and Setters

        private object MigrationPlanAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
                return _migrator.GetPlanForColumnInfo(col);

            return null;
        }

        private object PickedANOTable_ImageGetter(object rowObject)
        {
            var ci = rowObject as ColumnInfo;

            if (ci != null && _migrator.GetPlannedANOTable(ci) != null)
                return imageList1.Images["ANOTable"];

            return null;
        }

        private object PickedANOTableAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var ano = _migrator.GetPlannedANOTable(col);

                if (ano != null)
                    return ano.ToString();

                if (_migrator.GetPlanForColumnInfo(col) == ForwardEngineerANOVersionOfCatalogue.Plan.ANO)
                    return "pick";
            }

            return null;
        }

        private object DilutionAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var dilution = _migrator.GetPlannedDilution(col);

                if (dilution != null)
                    return dilution;

                if (_migrator.GetPlanForColumnInfo(col) == ForwardEngineerANOVersionOfCatalogue.Plan.Dillute)
                    return "pick";
            }

            return null;
        }
        private object Dilution_ImageGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                if (_migrator.GetPlannedDilution(col) != null)
                    return "PreLoadDiscardedColumn";
            }

            return null;
        }

        private object DestinationTypeAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;

            if (ci != null)
                return _migrator.GetEndpointDataType(ci);

            return null;
        }
        #endregion

        void tlvTableInfoMigrations_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is TableInfo)
                e.Cancel = true;

            var col = e.RowObject as ColumnInfo;

            if (e.Column == olvMigrationPlan)
                e.Control.Bounds = e.CellBounds;

            if (col != null && e.Column == olvPickedANOTable)
            {
                if(_migrator.GetPlanForColumnInfo(col) != ForwardEngineerANOVersionOfCatalogue.Plan.ANO)
                {
                    e.Cancel = true;
                    return;
                }

                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.CoreChildProvider.AllANOTables, true, false);
                try
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        _migrator.SetPlannedANOTable(col, dialog.Selected as ANOTable);
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }

                e.Cancel = true;
            }

            if (col != null && e.Column == olvDilution)
            {

                if (_migrator.GetPlanForColumnInfo(col) != ForwardEngineerANOVersionOfCatalogue.Plan.Dillute)
                {
                    e.Cancel = true;
                    return;
                }

                var cbx = new ComboBox();
                cbx.DropDownStyle = ComboBoxStyle.DropDownList;
                cbx.Bounds = e.CellBounds;
                cbx.Items.AddRange(_migrator.DilutionOperations.ToArray());
                e.Control = cbx;
            }
        }
        
        void tlvTableInfoMigrations_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            try
            {
                var col = e.RowObject as ColumnInfo;

                if(e.Column == olvMigrationPlan)
                    _migrator.SetPlan(col,(ForwardEngineerANOVersionOfCatalogue.Plan) e.NewValue);

                if(e.Column == olvDilution)
                {
                    var cbx = (ComboBox)e.Control;
                    _migrator.SetPlannedDilution(col,(IDilutionOperation)cbx.SelectedItem);
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
        
        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _migrator = new ForwardEngineerANOVersionOfCatalogue(databaseObject);
            if (!_setup)
            {
                //Set up tree view to show ANO Tables that are usable
                tlvANOTablesCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvANOTablesCommonFunctionality.SetUp(tlvANOTables,activator,olvANOTablesName,olvANOTablesName,false,false);
                tlvANOTables.AddObject(activator.CoreChildProvider.AllANOTablesNode);
                tlvANOTables.ExpandAll();
                
                //Setup tree view to show all TableInfos that you are trying to Migrate
                tlvTableInfoMigrationsCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvTableInfoMigrationsCommonFunctionality.SetUp(tlvTableInfoMigrations,activator,olvTableInfoName,olvTableInfoName,false,false);
                
                //don't display anything below ColumnInfo
                tlvTableInfoMigrationsCommonFunctionality.AxeChildren = new[] {typeof (ColumnInfo)};
                
                //Add them and expand them
                tlvTableInfoMigrations.AddObjects(databaseObject.GetTableInfoList(false));
                tlvTableInfoMigrations.ExpandAll();
                
                rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
                rdmpObjectsRibbonUI1.Add(databaseObject);
                _setup = true;
            }

        }

        private void tlvTableInfoMigrations_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            var ci = e.Model as ColumnInfo;

            if (ci != null && _migrator.IsMandatoryForMigration(ci))
                e.Item.BackColor = tbMandatory.BackColor;
        }

        private void tlvTableInfoMigrations_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.CellValue as string == "pick")
            {
                e.SubItem.ForeColor = Color.Blue;
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ForwardEngineerANOVersionOfCatalogueUI_Design, UserControl>))]
    public abstract class ForwardEngineerANOVersionOfCatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
