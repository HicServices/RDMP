// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FAnsi;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.ANOEngineeringUIs;

/// <summary>
/// Allows you to create an anonymous version of a Catalogue by selecting which columns to anonymise and which to drop etc.  This will create a new table in the
/// database of your choice which will be imported as a new Catalogue and a new LoadMetadata will be created that will migrate and apply the anonymisations to the
/// original Catalogue's data.
/// </summary>
public partial class ForwardEngineerANOCatalogueUI : ForwardEngineerANOCatalogueUI_Design
{
    private bool _setup;
    private RDMPCollectionCommonFunctionality tlvANOTablesCommonFunctionality;
    private RDMPCollectionCommonFunctionality tlvTableInfoMigrationsCommonFunctionality;
    private ForwardEngineerANOCataloguePlanManager _planManager;

    public ForwardEngineerANOCatalogueUI()
    {
        InitializeComponent();
        serverDatabaseTableSelector1.HideTableComponents();


        olvSuffix.AspectGetter = o => o is ANOTable anoTable ? anoTable.Suffix : null;
        olvNumberOfCharacters.AspectGetter = o =>
            o is ANOTable anoTable ? anoTable.NumberOfCharactersToUseInAnonymousRepresentation : null;
        olvNumberOfDigits.AspectGetter = o =>
            o is ANOTable anoTable ? anoTable.NumberOfIntegersToUseInAnonymousRepresentation : null;

        olvMigrationPlan.AspectGetter += MigrationPlanAspectGetter;

        olvPickedANOTable.HeaderImageKey = "ANOTable";
        olvPickedANOTable.AspectGetter += PickedANOTableAspectGetter;
        olvPickedANOTable.ImageGetter += PickedANOTable_ImageGetter;

        olvDilution.HeaderImageKey = "PreLoadDiscardedColumn";
        olvDilution.AspectGetter += DilutionAspectGetter;
        olvDilution.ImageGetter += Dilution_ImageGetter;

        olvDestinationType.AspectGetter += DestinationTypeAspectGetter;

        olvDestinationExtractionCategory.AspectGetter += DestinationExtractionCategoryAspectGetter;

        tlvTableInfoMigrations.CellEditStarting += tlvTableInfoMigrations_CellEditStarting;
        tlvTableInfoMigrations.CellEditFinishing += tlvTableInfoMigrations_CellEditFinishing;

        tlvTableInfoMigrations.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

        AssociatedCollection = RDMPCollection.Catalogue;

        btnLoadPlan.Image = FamFamFamIcons.page_white_get.ImageToBitmap();
        btnSavePlan.Image = FamFamFamIcons.page_white_put.ImageToBitmap();
    }

    #region Aspect Getters and Setters

    private object MigrationPlanAspectGetter(object rowobject)
    {
        var table = rowobject as TableInfo;

        if (rowobject is ColumnInfo col)
            return _planManager.GetPlanForColumnInfo(col).Plan;

        return _planManager.SkippedTables.Contains(table) ? "Already Exists" : (object)null;
    }

    private Image PickedANOTable_ImageGetter(object rowObject) =>
        rowObject is ColumnInfo ci && _planManager.GetPlanForColumnInfo(ci).ANOTable != null
            ? imageList1.Images["ANOTable"]
            : null;

    private object PickedANOTableAspectGetter(object rowobject)
    {
        if (rowobject is ColumnInfo col)
        {
            var plan = _planManager.GetPlanForColumnInfo(col);

            if (plan.ANOTable != null)
                return plan.ANOTable.ToString();

            if (plan.Plan == Plan.ANO)
                return "pick";
        }

        return null;
    }

    private object DilutionAspectGetter(object rowobject)
    {
        if (rowobject is ColumnInfo col)
        {
            var plan = _planManager.GetPlanForColumnInfo(col);

            if (plan.Dilution != null)
                return plan.Dilution;

            if (plan.Plan == Plan.Dilute)
                return "pick";
        }

        return null;
    }

    private string Dilution_ImageGetter(object rowobject)
    {
        if (rowobject is ColumnInfo col)
        {
            var plan = _planManager.GetPlanForColumnInfo(col);

            if (plan.Dilution != null)
                return "PreLoadDiscardedColumn";
        }

        return null;
    }

    private object DestinationTypeAspectGetter(object rowobject)
    {
        try
        {
            if (rowobject is ColumnInfo ci)
                return _planManager.GetPlanForColumnInfo(ci).GetEndpointDataType();
        }
        catch (Exception)
        {
            return "Error";
        }

        return null;
    }


    private object DestinationExtractionCategoryAspectGetter(object rowobject)
    {
        try
        {
            if (rowobject is ColumnInfo ci)
            {
                var plan = _planManager.GetPlanForColumnInfo(ci);

                return plan.ExtractionCategoryIfAny;
            }
        }
        catch (Exception)
        {
            return "Error";
        }

        return null;
    }

    #endregion

    private void tlvTableInfoMigrations_CellEditStarting(object sender, CellEditEventArgs e)
    {
        if (e.RowObject is TableInfo)
            e.Cancel = true;

        if (e.Column == olvDestinationType)
            e.Cancel = true;


        if (e.Column == olvMigrationPlan)
            e.Control.Bounds = e.CellBounds;

        if (e.RowObject is ColumnInfo col)
        {
            var plan = _planManager.GetPlanForColumnInfo(col);

            if (e.Column == olvPickedANOTable)
            {
                if (plan.Plan != Plan.ANO)
                {
                    e.Cancel = true;
                    return;
                }


                if (Activator.SelectObject(new DialogArgs
                {
                    TaskDescription =
                            "Choose an ANOTable into which to put the identifiable values stored in this column"
                }, Activator.CoreChildProvider.AllANOTables, out var selected))
                    try
                    {
                        plan.ANOTable = selected;
                        Check();
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                    }

                e.Cancel = true;
            }

            if (e.Column == olvDilution)
            {
                if (plan.Plan != Plan.Dilute)
                {
                    e.Cancel = true;
                    return;
                }

                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = e.CellBounds
                };
                cbx.Items.AddRange(_planManager.DilutionOperations.ToArray());
                e.Control = cbx;
            }

            if (e.Column == olvDestinationExtractionCategory)
            {
                //if the plan is to drop
                if (plan.Plan == Plan.Drop)
                {
                    //don't let them edit it
                    e.Cancel = true;
                    return;
                }

                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = e.CellBounds
                };

                var list = Enum.GetValues(typeof(ExtractionCategory)).Cast<object>().Select(s => s.ToString()).ToList();
                list.Add("Clear");

                cbx.Items.AddRange(list.ToArray());
                e.Control = cbx;

                if (plan.ExtractionCategoryIfAny.HasValue)
                    cbx.SelectedItem = plan.ExtractionCategoryIfAny.Value.ToString();
            }
        }
    }

    private void tlvTableInfoMigrations_CellEditFinishing(object sender, CellEditEventArgs e)
    {
        try
        {
            if (e.RowObject is not ColumnInfo col)
                return;

            var plan = _planManager.GetPlanForColumnInfo(col);

            if (e.Column == olvMigrationPlan)
                plan.Plan = (Plan)e.NewValue;

            if (e.Column == olvDilution)
            {
                var cbx = (ComboBox)e.Control;
                plan.Dilution = (IDilutionOperation)cbx.SelectedItem;
            }

            if (e.Column == olvDestinationExtractionCategory)
            {
                var cbx = (ComboBox)e.Control;
                if ((string)cbx.SelectedItem == "Clear")
                {
                    plan.ExtractionCategoryIfAny = null;
                }
                else
                {
                    Enum.TryParse((string)cbx.SelectedItem, out ExtractionCategory c);
                    plan.ExtractionCategoryIfAny = c;
                }
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

        serverDatabaseTableSelector1.SetItemActivator(activator);
        try
        {
            _planManager = new ForwardEngineerANOCataloguePlanManager(activator.RepositoryLocator, databaseObject);
        }
        catch (QueryBuildingException e)
        {
            CommonFunctionality.Fatal("Could not generate a valid query for the Catalogue", e);
            return;
        }

        if (!_setup)
        {
            var settings = new RDMPCollectionCommonFunctionalitySettings
            { AddFavouriteColumn = false, AddCheckColumn = false };

            //Set up tree view to show ANO Tables that are usable
            tlvANOTablesCommonFunctionality = new RDMPCollectionCommonFunctionality();
            tlvANOTablesCommonFunctionality.SetUp(RDMPCollection.None, tlvANOTables, activator, olvANOTablesName, null,
                settings);

            tlvANOTables.AddObject(activator.CoreChildProvider.AllANOTablesNode);
            tlvANOTables.ExpandAll();

            //Setup tree view to show all TableInfos that you are trying to Migrate
            tlvTableInfoMigrationsCommonFunctionality = new RDMPCollectionCommonFunctionality();
            tlvTableInfoMigrationsCommonFunctionality.SetUp(RDMPCollection.None, tlvTableInfoMigrations, activator,
                olvTableInfoName, null, settings);

            //don't display anything below ColumnInfo
            tlvTableInfoMigrationsCommonFunctionality.AxeChildren = new[] { typeof(ColumnInfo) };

            _setup = true;
        }

        //Add them and expand them
        tlvTableInfoMigrations.ClearObjects();
        tlvTableInfoMigrations.AddObjects(_planManager.TableInfos);
        tlvTableInfoMigrations.ExpandAll();

        ddDateColumn.DataSource = _planManager.TableInfos.SelectMany(t => t.ColumnInfos)
            .Where(c => c.Data_type != null && c.Data_type.Contains("date")).ToArray();

        Check();
    }

    private void Check()
    {
        if (_planManager.TargetDatabase != null)
            if (_planManager.TargetDatabase.Exists())
            {
                _planManager.SkippedTables.Clear();

                foreach (var t in _planManager.TableInfos)
                {
                    var existing = _planManager.TargetDatabase.DiscoverTables(true);

                    //it is already migrated
                    if (existing.Any(e => e.GetRuntimeName().Equals(t.GetRuntimeName())))
                        _planManager.SkippedTables.Add(t);
                }
            }

        ragSmiley1.StartChecking(_planManager);

        DisableObjects();
    }

    private void tlvTableInfoMigrations_FormatCell(object sender, FormatCellEventArgs e)
    {
        if (e.Model is ColumnInfo ci)
            if (_planManager.GetPlanForColumnInfo(ci).IsMandatory)
                e.SubItem.BackColor = lblMandatory.BackColor;

        if (e.Column == olvMigrationPlan)
            if (e.Model is ColumnInfo)
            {
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
            }
            else
            {
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Italic);
                e.SubItem.ForeColor = Color.Gray;
            }

        if (e.CellValue as string == "pick")
        {
            e.SubItem.ForeColor = Color.Blue;
            e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
        }
    }

    private void btnRefreshChecks_Click(object sender, EventArgs e)
    {
        _planManager.TargetDatabase = serverDatabaseTableSelector1.GetDiscoveredDatabase();
        Check();
    }

    private void serverDatabaseTableSelector1_SelectionChanged()
    {
        _planManager.TargetDatabase = serverDatabaseTableSelector1.GetDiscoveredDatabase();

        Check();
    }

    private void DisableObjects()
    {
        var toDisable = new List<object>();

        toDisable.AddRange(_planManager.SkippedTables);
        toDisable.AddRange(_planManager.SkippedTables.SelectMany(t => t.ColumnInfos));

        tlvTableInfoMigrations.DisabledObjects = toDisable;
    }

    private void btnExecute_Click(object sender, EventArgs e)
    {
        try
        {
            var engine = new ForwardEngineerANOCatalogueEngine(Activator.RepositoryLocator, _planManager);
            engine.Execute();

            if (engine.NewCatalogue != null && engine.LoadMetadata != null)
            {
                foreach (var sqls in engine.SelectSQLForMigrations)
                    CreateAttacher(sqls.Key, sqls.Value, engine.LoadMetadata,
                        sqls.Key.IsLookupTable() ? null : engine.LoadProgressIfAny);

                foreach (var dilutionOps in engine.DilutionOperationsForMigrations)
                    CreateDilutionMutilation(dilutionOps, engine.LoadMetadata);

                Publish(engine.NewCatalogue);

                if (Activator.YesNo($"Successfully created Catalogue '{engine.NewCatalogue}', close form?", "Success"))
                    Activator.WindowArranger.SetupEditAnything(this, engine.LoadMetadata);
            }
            else
            {
                throw new Exception("Engine did not create a NewCatalogue/LoadMetadata");
            }
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    private void CreateAttacher(ITableInfo t, QueryBuilder qb, LoadMetadata lmd, LoadProgress loadProgressIfAny)
    {
        var pt = new ProcessTask(Activator.RepositoryLocator.CatalogueRepository, lmd, LoadStage.Mounting)
        {
            ProcessTaskType = ProcessTaskType.Attacher,
            Name = $"Read from {t}",
            Path = typeof(RemoteTableAttacher).FullName
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RemoteTableAttacher>();


        pt.SetArgumentValue("RemoteServer", t.Server);
        pt.SetArgumentValue("RemoteDatabaseName", t.GetDatabaseRuntimeName(LoadStage.PostLoad));
        pt.SetArgumentValue("RemoteTableName", t.GetRuntimeName());
        pt.SetArgumentValue("DatabaseType", DatabaseType.MicrosoftSQLServer);
        pt.SetArgumentValue("RemoteSelectSQL", qb.SQL);

        pt.SetArgumentValue("RAWTableName", t.GetRuntimeName(LoadBubble.Raw));

        if (loadProgressIfAny != null)
        {
            pt.SetArgumentValue("Progress", loadProgressIfAny);
            //              pt.SetArgumentValue("ProgressUpdateStrategy", DataLoadProgressUpdateStrategy.UseMaxRequestedDay);
            pt.SetArgumentValue("LoadNotRequiredIfNoRowsRead", true);
        }

        /*

            public DataLoadProgressUpdateInfo { get; set; }
        */
    }

    private void CreateDilutionMutilation(KeyValuePair<PreLoadDiscardedColumn, IDilutionOperation> dilutionOp,
        LoadMetadata lmd)
    {
        var pt = new ProcessTask(Activator.RepositoryLocator.CatalogueRepository, lmd, LoadStage.AdjustStaging);
        pt.CreateArgumentsForClassIfNotExists<Dilution>();
        pt.ProcessTaskType = ProcessTaskType.MutilateDataTable;
        pt.Name = $"Dilute {dilutionOp.Key.GetRuntimeName()}";
        pt.Path = typeof(Dilution).FullName;
        pt.SaveToDatabase();

        pt.SetArgumentValue("ColumnToDilute", dilutionOp.Key);
        pt.SetArgumentValue("Operation", dilutionOp.Value.GetType());
    }

    private void ddDateColumn_SelectedIndexChanged(object sender, EventArgs e)
    {
        _planManager.DateColumn = cbDateBasedLoad.Checked ? ddDateColumn.SelectedItem as ColumnInfo : null;
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
        _planManager.StartDate = cbDateBasedLoad.Checked ? GetStartDate() : null;
    }

    private DateTime? GetStartDate()
    {
        if (cbDateBasedLoad.Checked)
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

        return null;
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        tlvTableInfoMigrations.UseFiltering = true;
        tlvTableInfoMigrations.ModelFilter = new TextMatchFilter(tlvTableInfoMigrations, tbFilter.Text);
    }

    private void btnSavePlan_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "Plans (*.plan)|*.plan"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            var fi = new FileInfo(sfd.FileName);

            var cmdAnoTablesToo = new ExecuteCommandExportObjectsToFile(Activator,
                Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ANOTable>().ToArray(), fi.Directory)
            {
                ShowInExplorer = false
            };

            if (!cmdAnoTablesToo.IsImpossible)
                cmdAnoTablesToo.Execute();

            var json = JsonConvertExtensions.SerializeObject(_planManager, Activator.RepositoryLocator);
            File.WriteAllText(fi.FullName, json);
        }
    }

    private void btnLoadPlan_Click(object sender, EventArgs e)
    {
        try
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Plans (*.plan)|*.plan"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var fi = new FileInfo(ofd.FileName);
            var json = File.ReadAllText(fi.FullName);
            _planManager = (ForwardEngineerANOCataloguePlanManager)
                JsonConvertExtensions.DeserializeObject(json, typeof(ForwardEngineerANOCataloguePlanManager),
                    Activator.RepositoryLocator);

            if (_planManager.StartDate != null)
                tbStartDate.Text = _planManager.StartDate.Value.ToString(CultureInfo.CurrentCulture);

            cbDateBasedLoad.Checked = _planManager.DateColumn != null;
            ddDateColumn.SelectedItem = _planManager.DateColumn;
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ForwardEngineerANOCatalogueUI_Design, UserControl>))]
public abstract class ForwardEngineerANOCatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>;