// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.TransparentHelpSystem;
using Rdmp.UI.Tutorials;

namespace Rdmp.UI.SimpleDialogs.ForwardEngineering;

/// <summary>
/// This dialog is shown when the RDMP learns about a new data table in your data repository that you want it to curate.  This can be either following a the successful flat file import
/// or after selecting an existing table for importing metadata from (See ImportSQLTable).
/// 
/// <para>If you click 'Cancel' then no dataset (Catalogue) will be created and you will only have the TableInfo/ColumnInfo collection stored in your RDMP database, you will need to manually wire
/// these up to a Catalogue or delete them if you decied you want to make the dataset extractable later on. </para>
/// 
/// <para>Alternatively you can create a new Catalogue, this will result in a Catalogue (dataset) of the same name as the table and a CatalogueItem being created for each ColumnInfo imported.
/// If you choose to you can make these CatalogueItems extractable by creating ExtractionInformation too or you may choose to do this by hand later on (in CatalogueItemUI).  It is likely that
/// you don't want to release every column in the dataset to researchers so make sure to review the extractability of the columns created. </para>
/// 
/// <para>You can choose a single extractable column to be the Patient Identifier (e.g. CHI / NHS number etc). This column must be the same (logically/datatype) across all your datasets i.e.
/// you can use either CHI number or NHS Number but you can't mix and match (but you could have fields with different names e.g. PatCHI, PatientCHI, MotherCHI, FatherChiNo etc).</para>
/// 
/// <para>The final alternative is to add the imported Columns to another already existing Catalogue.  Only use this option if you know it is possible to join the new table with the other
/// table(s) that underlie the selected Catalogue (e.g. if you are importing a Results table which joins to a Header table in the dataset Biochemistry on primary/foreign key LabNumber).
/// If you choose this option you must configure the JoinInfo logic (See JoinConfiguration)</para>
/// </summary>
public partial class ConfigureCatalogueExtractabilityUI : RDMPForm, ISaveableUI
{
    private object[] _extractionCategories;

    private string NotExtractable = "Not Extractable";
    private ICatalogue _catalogue;
    private ITableInfo _tableInfo;
    private bool _choicesFinalised;
    private HelpWorkflow _workflow;
    private CatalogueItem[] _catalogueItems;
    private bool _ddChangeAllChanged;
    private bool _importedNewTable;

    /// <summary>
    /// the Project to associate the Catalogue with to make it ProjectSpecific (probably null)
    /// </summary>
    private IProject _projectSpecific;

    public ICatalogue CatalogueCreatedIfAny => _catalogue;
    public ITableInfo TableInfoCreated => _tableInfo;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DiscoveredTable TableCreated { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TargetFolder { get; set; }

    private BinderWithErrorProviderFactory _binder;
    private ObjectSaverButton objectSaverButton1 = new();

    /// <summary>
    /// True if we are making programmatic changes to values and shouldn't respond to control events (e.g. dropdown changes)
    /// </summary>
    private bool isLoading;

    private const string None = "<<None>>";

    public ConfigureCatalogueExtractabilityUI(IActivateItems activator, ITableInfo tableInfo, string initialDescription,
        IProject projectSpecificIfAny) : this(activator)
    {
        _tableInfo = tableInfo;
        Initialize(activator, initialDescription, projectSpecificIfAny);
    }

    public ConfigureCatalogueExtractabilityUI(IActivateItems activator, ITableInfoImporter importer,
        string initialDescription, IProject projectSpecificIfAny) : this(activator)
    {
        importer.DoImport(out _tableInfo, out var cols);

        _importedNewTable = true;

        Initialize(activator, initialDescription, projectSpecificIfAny);
    }

    private ConfigureCatalogueExtractabilityUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();

        ContextMenuStrip = new ContextMenuStrip();
        ContextMenuStrip.Items.Add(new SaveMenuItem(this));
    }

    private void Initialize(IActivateItems activator, string initialDescription, IProject projectSpecificIfAny)
    {
        CommonFunctionality.SetItemActivator(activator);
        var cols = _tableInfo.ColumnInfos;

        var forwardEngineer = new ForwardEngineerCatalogue(_tableInfo, cols);
        forwardEngineer.ExecuteForwardEngineering(out _catalogue, out _catalogueItems, out var eis);

        tbDescription.Text = $"{initialDescription} ({Environment.UserName} - {DateTime.Now})";
        tbTableName.Text = _tableInfo.Name;
        _catalogue.SaveToDatabase();
        objectSaverButton1.SetupFor(this, (DatabaseEntity)_catalogue, activator);

        if (_binder == null)
        {
            _binder = new BinderWithErrorProviderFactory(activator);
            _binder.Bind(tbCatalogueName, "Text", _catalogue, "Name", false, DataSourceUpdateMode.OnPropertyChanged,
                c => c.Name);
            _binder.Bind(tbAcronym, "Text", _catalogue, "Acronym", false, DataSourceUpdateMode.OnPropertyChanged,
                c => c.Acronym);
            _binder.Bind(tbDescription, "Text", _catalogue, "Description", false,
                DataSourceUpdateMode.OnPropertyChanged, c => c.Description);
        }

        //Every CatalogueItem is either mapped to a ColumnInfo (not extractable) or a ExtractionInformation (extractable).  To start out with they are not extractable
        foreach (var ci in _catalogueItems)
            olvColumnExtractability.AddObject(new ColPair(ci, cols.Single(col => ci.ColumnInfo_ID == col.ID),
                eis.SingleOrDefault(e => e.CatalogueItem_ID == ci.ID)));

        _extractionCategories = new object[]
        {
            NotExtractable,
            ExtractionCategory.Core,
            ExtractionCategory.Supplemental,
            ExtractionCategory.SpecialApprovalRequired,
            ExtractionCategory.Internal,
            ExtractionCategory.Deprecated
        };

        ddCategoriseMany.Items.AddRange(_extractionCategories);

        olvExtractionCategory.AspectGetter += ExtractionCategoryAspectGetter;
        olvColumnExtractability.AlwaysGroupByColumn = olvExtractionCategory;

        olvColumnExtractability.CellEditStarting += TlvColumnExtractabilityOnCellEditStarting;
        olvColumnExtractability.CellEditFinishing += TlvColumnExtractabilityOnCellEditFinishing;
        olvColumnExtractability.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

        olvIsExtractionIdentifier.AspectPutter += IsExtractionIdentifier_AspectPutter;
        olvIsExtractionIdentifier.AspectGetter += IsExtractionIdentifier_AspectGetter;


        olvIsHashOnRelease.AspectPutter += IsHashOnRelease_AspectPutter;
        olvIsHashOnRelease.AspectGetter += IsHashOnRelease_AspectGetter;

        olvColumnInfoName.ImageGetter = ImageGetter;
        olvColumnExtractability.RebuildColumns();

        if (Activator.RepositoryLocator.DataExportRepository == null)
        {
            gbProjectSpecific.Enabled = false;
        }
        else
        {
            SelectProject(projectSpecificIfAny);
            pbProject.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Project).ImageToBitmap();
        }

        ddIsExtractionIdentifier.Items.Add(None);
        ddIsExtractionIdentifier.Items.AddRange(olvColumnExtractability.Objects.OfType<ColPair>().ToArray());

        RDMPControlCommonFunctionality.DisableMouseWheel(ddIsExtractionIdentifier);

        CommonFunctionality.AddHelp(btnPickProject, "IExtractableDataSet.Project_ID", "Project Specific Datasets");
        CommonFunctionality.AddHelpString(btnAddToExisting, "Add to existing catalogue",
            "Use this option if you want to create a Catalogue which extracts from multiple tables (via a JOIN).  Once used you will still need to configure a JoinInfo between column(s) in all the tables the Catalogue draws data from.");

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvColumnExtractability, olvColumnInfoName,
            new Guid("86e7bf77-8829-4f72-aa47-29a30710621f"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvColumnExtractability, olvExtractionCategory,
            new Guid("ad106fe4-bea0-4670-9ded-61967699269f"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvColumnExtractability, olvIsExtractionIdentifier,
            new Guid("f9a87191-9676-4c86-a292-608dc95e8eef"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvColumnExtractability, olvIsHashOnRelease,
            new Guid("d3590a68-ee78-40c6-b073-eea32cac49ec"));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (!string.IsNullOrWhiteSpace(TargetFolder))
        {
            _catalogue.Folder = TargetFolder;
            _catalogue.SaveToDatabase();
        }
    }

    private void IsExtractionIdentifier_AspectPutter(object rowobject, object newvalue)
    {
        var n = (ColPair)rowobject;

        if (n.ExtractionInformation == null)
            MakeExtractable(n, true, ExtractionCategory.Core);

        Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
        n.ExtractionInformation.IsExtractionIdentifier = (bool)newvalue;
        n.ExtractionInformation.SaveToDatabase();


        // if there is now exactly 1 IsExtractionIdentifier
        var identifiers = olvColumnExtractability.Objects.OfType<ColPair>()
            .Where(c => c.ExtractionInformation?.IsExtractionIdentifier ?? false).ToArray();
        isLoading = true;

        if (identifiers.Length == 1)
        {
            // update the combo box to show that this is the only one
            ddIsExtractionIdentifier.SelectedItem = identifiers[0];
            ddIsExtractionIdentifier.Enabled = true;
        }
        else
        {
            // if there are 2+ or 0 then clear the dropdown because that control only works
            // for selecting a single patient identifier
            ddIsExtractionIdentifier.SelectedItem = null;


            // let them pick one with the dropdown if it is 0 but if it is 2+ then they have to use the checkboxes
            ddIsExtractionIdentifier.Enabled = identifiers.Length < 2;
        }

        isLoading = false;
    }

    private Bitmap ImageGetter(object rowObject)
    {
        var n = (ColPair)rowObject;

        return Activator.CoreIconProvider.GetImage((object)n.ExtractionInformation ?? n.ColumnInfo).ImageToBitmap();
    }

    private object IsExtractionIdentifier_AspectGetter(object rowObject)
    {
        var n = (ColPair)rowObject;

        return n.ExtractionInformation == null ? false : (object)n.ExtractionInformation.IsExtractionIdentifier;
    }


    private void IsHashOnRelease_AspectPutter(object rowobject, object newvalue)
    {
        var n = (ColPair)rowobject;

        if (n.ExtractionInformation == null)
            MakeExtractable(n, true, ExtractionCategory.Core);

        Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
        n.ExtractionInformation.HashOnDataRelease = (bool)newvalue;

        // if we are turning on hashing then ensure the column has an alias
        if ((bool)newvalue) n.ExtractionInformation.Alias = n.ExtractionInformation.GetRuntimeName();

        n.ExtractionInformation.SaveToDatabase();
    }

    private object IsHashOnRelease_AspectGetter(object rowObject)
    {
        var n = (ColPair)rowObject;

        return n.ExtractionInformation == null ? false : (object)n.ExtractionInformation.HashOnDataRelease;
    }

    private void MakeExtractable(object o, bool shouldBeExtractable, ExtractionCategory? category = null)
    {
        var n = (ColPair)o;

        //if it has extraction information
        if (n.ExtractionInformation != null)
        {
            if (shouldBeExtractable)
            {
                //if they want to change the extraction category
                if (category.HasValue && n.ExtractionInformation.ExtractionCategory != category.Value)
                {
                    n.ExtractionInformation.ExtractionCategory = category.Value;
                    n.ExtractionInformation.Order = olvColumnExtractability.IndexOf(n);
                    n.ExtractionInformation.SaveToDatabase();
                    olvColumnExtractability.RefreshObject(n);
                }

                return;
            }

            //make it not extractable by deleting the extraction information
            n.ExtractionInformation.DeleteInDatabase();
            n.ExtractionInformation = null;
        }
        else
        {
            //it doesn't have ExtractionInformation

            if (!shouldBeExtractable) //it's already not extractable job done
                return;
            //make it extractable
            var newExtractionInformation = new ExtractionInformation((ICatalogueRepository)n.ColumnInfo.Repository,
                n.CatalogueItem, n.ColumnInfo, n.ColumnInfo.Name);

            if (category.HasValue)
            {
                newExtractionInformation.ExtractionCategory = category.Value;
                newExtractionInformation.Order = olvColumnExtractability.IndexOf(n);
                newExtractionInformation.SaveToDatabase();
            }

            n.ExtractionInformation = newExtractionInformation;
        }

        olvColumnExtractability.RefreshObject(n);
    }


    private object ExtractionCategoryAspectGetter(object rowobject)
    {
        var n = (ColPair)rowobject;

        return n.ExtractionInformation == null ? "Not Extractable" : n.ExtractionInformation.ExtractionCategory;
    }


    private void TlvColumnExtractabilityOnCellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
    {
        var n = (ColPair)cellEditEventArgs.RowObject;

        if (cellEditEventArgs.Column == olvColumnInfoName)
            cellEditEventArgs.Cancel = true;

        if (cellEditEventArgs.Column == olvExtractionCategory)
        {
            var cbx = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds = cellEditEventArgs.CellBounds
            };

            cbx.Items.AddRange(_extractionCategories);
            cbx.SelectedItem = n.ExtractionInformation != null
                ? (object)n.ExtractionInformation.ExtractionCategory
                : NotExtractable;
            cellEditEventArgs.Control = cbx;
        }
    }

    private void TlvColumnExtractabilityOnCellEditFinishing(object sender, CellEditEventArgs cellEditEventArgs)
    {
        var n = (ColPair)cellEditEventArgs.RowObject;

        if (cellEditEventArgs.Column == olvExtractionCategory)
        {
            var cbx = (ComboBox)cellEditEventArgs.Control;

            if (Equals(cbx.SelectedItem, NotExtractable))
                MakeExtractable(n, false, null);
            else
                MakeExtractable(n, true, (ExtractionCategory)cbx.SelectedItem);
        }
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        olvColumnExtractability.UseFiltering = true;

        var textFilter = new TextMatchFilter(olvColumnExtractability, tbFilter.Text)
        {
            Columns = new[] { olvColumnInfoName }
        };
        olvColumnExtractability.ModelFilter = textFilter;
    }

    private void ddCategoriseMany_SelectedIndexChanged(object sender, EventArgs e)
    {
        var filteredObjects = olvColumnExtractability.FilteredObjects.Cast<ColPair>().ToArray();
        var toChangeTo = ddCategoriseMany.SelectedItem;
        var itemsToChange = filteredObjects.Where(obj => obj.ExtractionInformation is null || !obj.ExtractionInformation.ExtractionCategory.Equals(toChangeTo)).Select(static cp => cp.CatalogueItem.Name).ToArray();
        var columnChangeDetails = itemsToChange.Length < 3
            ? new StringBuilder().AppendJoin(", ", itemsToChange[..^1])
                .Append(itemsToChange.Length > 1 ? " and " : "").Append(itemsToChange[^1]).ToString()
            : $"{(itemsToChange.Length == filteredObjects.Length ? "all " : "")}{itemsToChange.Length} columns";

        if (MessageBox.Show($"Set {columnChangeDetails} to '{toChangeTo}'?",
                "Confirm Overwrite?", MessageBoxButtons.OKCancel) != DialogResult.OK) return;

        foreach (var o in filteredObjects)
            if (toChangeTo.Equals(NotExtractable))
                MakeExtractable(o, false);
            else
                MakeExtractable(o, true, (ExtractionCategory)toChangeTo);

        _ddChangeAllChanged = true;
    }

    private void FinaliseExtractability()
    {
        _ = new ExtractableDataSet(Activator.RepositoryLocator.DataExportRepository, _catalogue);

        if (_projectSpecific == null) return;

        IAtomicCommandWithTarget cmd =
            new ExecuteCommandMakeCatalogueProjectSpecific(Activator, _catalogue, _projectSpecific,false);

        if (cmd.IsImpossible)
            MessageBox.Show($"Could not make Catalogue ProjectSpecific:{cmd.ReasonCommandImpossible}");
        else
            cmd.Execute();
    }

    private void btnAddToExisting_Click(object sender, EventArgs e)
    {
        var eis = GetExtractionInformations();

        if (!eis.Any())
        {
            MessageBox.Show(
                "You must set at least one column to extractable before you can add them to another Catalogue");
            return;
        }

        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "You are about to add the newly imported table columns to an existing Catalogue.  This will mean that your Catalogue draws data from 2+ tables.  You will need to also create a join between the underlying columns for this to work properly."
        }, Activator.CoreChildProvider.AllCatalogues, out var selected))
            AddToExistingCatalogue(selected, eis);
    }

    private void AddToExistingCatalogue(Catalogue addToInstead, ExtractionInformation[] eis)
    {
        var existingTables = addToInstead.GetTableInfosIdeallyJustFromMainTables();

        //move all the CatalogueItems to the other Catalogue instead
        foreach (var ei in eis)
        {
            var ci = ei.CatalogueItem;
            ci.Catalogue_ID = addToInstead.ID;
            ci.SaveToDatabase();
        }

        _choicesFinalised = true;
        _catalogue.DeleteInDatabase();

        // prevents wrappers thinking no Catalogue was created and offering to drop the TableInfo!
        _catalogue = addToInstead;

        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(addToInstead));

        Close();

        ITableInfo joinFrom;

        // if user is adding a second table and there's currently only 1 TableInfo
        // associated with the Catalogue
        if (existingTables.Length == 1)
        {
            // mark the TableInfo as primary
            existingTables[0].IsPrimaryExtractionTable = true;
            existingTables[0].SaveToDatabase();

            // let the user configure how to join to the second table
            joinFrom = existingTables[0];
        }
        else
        {
            joinFrom = (ITableInfo)Activator.SelectOne(new DialogArgs
            {
                WindowTitle = "Pick Table for Join",
                TaskDescription =
                    $"Your Catalogue '{addToInstead}' has {existingTables.Length} tables already associated with it.  Which of these can be joined to your new table '{TableInfoCreated}'"
            }, existingTables);

            // user cancelled joining
            if (joinFrom == null)
                return;
        }

        var existingJoins = Activator.RepositoryLocator.CatalogueRepository.JoinManager
            .GetAllJoinInfosBetweenColumnInfoSets(joinFrom.ColumnInfos,
                TableInfoCreated.ColumnInfos);

        // we already know how to join these tables
        if (existingJoins.Any())
            return;

        // let the user configure how to join to the second table
        var cmdAddJoin = new ExecuteCommandAddJoinInfo(Activator, (TableInfo)joinFrom);
        cmdAddJoin.SetInitialJoinToTableInfo((TableInfo)TableInfoCreated);

        if (!cmdAddJoin.IsImpossible)
        {
            Activator.Show("Configure Join",
                "You will now be taken to the join configuration screen to define how your new table is linked to in SQL queries");
            cmdAddJoin.Execute();
        }
        else
        {
            Activator.Show("Could not generate JoinInfo screen",
                $"Failed to show JoinInfo configuration screen:{cmdAddJoin.ReasonCommandImpossible}");
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        var eis = GetExtractionInformations();
        objectSaverButton1.Save();

        if (!eis.Any())
        {
            if (!Activator.YesNo(
                    "You have not marked any columns as extractable, are you sure you want to create a dataset with no extractable columns?",
                    "Create with no extractable columns?"))
                return;
        }
        else if (eis.Any(ei => ei.IsExtractionIdentifier))
        {
            FinaliseExtractability();
        }
        else
        {
            if (MessageBox.Show("You have not chosen a column to be IsExtractionIdentifier, do you wish to continue?",
                    "Confirm", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;
        }

        _choicesFinalised = true;
        DialogResult = DialogResult.OK;

        if (CatalogueCreatedIfAny != null)
            objectSaverButton1.CheckForUnsavedChangesAnOfferToSave();

        Close();
    }

    private ExtractionInformation[] GetExtractionInformations()
    {
        return olvColumnExtractability.Objects.Cast<ColPair>()
            .Where(n => n.ExtractionInformation != null)
            .Select(ei => ei.ExtractionInformation)
            .ToArray();
    }

    private void ConfigureCatalogueExtractabilityUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_choicesFinalised)
        {
            if (_importedNewTable)
            {
                var confirm = MessageBox.Show(
                    "The database table was created as part of this import. Do you want to keep that?",
                    "Confirm", MessageBoxButtons.YesNoCancel);
                if (confirm != DialogResult.Cancel)
                {
                    DialogResult = DialogResult.Cancel;
                    _catalogue.DeleteInDatabase();
                    _catalogue = null;
                    if (confirm == DialogResult.No)
                    {
                        _tableInfo.DeleteInDatabase();
                        _tableInfo = null;
                        if (TableCreated != null && TableCreated.Exists())
                            TableCreated.Drop();
                    }
                    else
                    {
                        Activator.Publish(TableInfoCreated);
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                if (MessageBox.Show(
                        "Are you sure you want to Cancel?",
                        "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DialogResult = DialogResult.Cancel;
                    _catalogue.DeleteInDatabase();
                    _catalogue = null;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        else
        {
            if (CatalogueCreatedIfAny != null)
                Activator.Publish(CatalogueCreatedIfAny);
        }
    }

    private void ConfigureCatalogueExtractabilityUI_Load(object sender, EventArgs e)
    {
        _workflow = new HelpWorkflow(this, new Guid("74e6943e-1ed8-4c43-89c2-96158c1360fa"),
            new TutorialTracker(Activator));
        var stage1 = new HelpStage(olvColumnExtractability,
            "This is a collection of all the column definitions imported, change the Extractable status of one of the columns to make it extractable",
            () => GetExtractionInformations().Any());
        var stage2 = new HelpStage(ddIsExtractionIdentifier,
            "One of your columns should contain a patient identifier, select it here",
            () => GetExtractionInformations().Any(ei => ei.IsExtractionIdentifier));
        var stage3 = new HelpStage(pChangeAll, "Change this dropdown to change all at once", () => _ddChangeAllChanged);
        var stage4 = new HelpStage(pFilter, "Type in here if you are trying to find a specific column",
            () => !string.IsNullOrWhiteSpace(tbFilter.Text));

        stage1.SetNext(stage2);
        stage2.SetNext(stage3);
        stage2.OptionButtonText = "I don't have one of those";
        stage2.OptionDestination = stage3;
        stage3.SetNext(stage4);

        _workflow.RootStage = stage1;

        helpIcon1.SetHelpText("Configure Extractability", "Click for tutorial", _workflow);
    }

    private class ColPair
    {
        public CatalogueItem CatalogueItem;
        public ColumnInfo ColumnInfo;
        public ExtractionInformation ExtractionInformation;

        public ColPair(CatalogueItem ci, ColumnInfo col, ExtractionInformation ei)
        {
            CatalogueItem = ci;
            ColumnInfo = col;
            ExtractionInformation = ei;
        }

        public override string ToString() => CatalogueItem.Name;
    }

    private void SelectProject(IProject projectSpecificIfAny)
    {
        if (projectSpecificIfAny == null)
        {
            lblProject.Text = None;
            btnPickProject.Text = "Pick...";
            _projectSpecific = null;
        }
        else
        {
            lblProject.Text = projectSpecificIfAny.Name;
            btnPickProject.Text = "Clear";
            _projectSpecific = projectSpecificIfAny;
        }
    }

    private void btnPickProject_Click(object sender, EventArgs e)
    {
        if (_projectSpecific != null)
        {
            SelectProject(null);
        }
        else
        {
            if (Activator.SelectObject(new DialogArgs
            {
                TaskDescription =
                            "Which Project should this Catalogue be associated with? ProjectSpecific Catalogues can only be extracted in extractions of that Project and will not appear in the main Catalogues tree."
            }, Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().ToArray(),
                    out var selected))
                SelectProject(selected);
        }
    }

    private void ddIsExtractionIdentifier_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (isLoading) return;


        //turn off all IsExtractionIdentifierness
        foreach (var node in ddIsExtractionIdentifier.Items.OfType<ColPair>())
            if (node.ExtractionInformation != null && node.ExtractionInformation.IsExtractionIdentifier)
            {
                node.ExtractionInformation.IsExtractionIdentifier = false;
                node.ExtractionInformation.SaveToDatabase();
            }

        //we cleared them all, now did they want one selected (i.e. they selected anything except <<None>>)
        if (ddIsExtractionIdentifier.SelectedItem is ColPair n)
        {
            if (n.ExtractionInformation == null)
                MakeExtractable(n, true, ExtractionCategory.Core);

            Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
            n.ExtractionInformation.IsExtractionIdentifier = true;
            n.ExtractionInformation.SaveToDatabase();
        }

        olvColumnExtractability.RebuildColumns();
    }

    public ObjectSaverButton GetObjectSaverButton() => objectSaverButton1;

    private void BtnRenameTableInfo_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandAlterTableName(Activator, _tableInfo);

        if (cmd.IsImpossible)
        {
            MessageBox.Show(cmd.ReasonCommandImpossible);
        }
        else
        {
            cmd.Execute();
            tbTableName.Text = _tableInfo.GetFullyQualifiedName();
        }
    }
}