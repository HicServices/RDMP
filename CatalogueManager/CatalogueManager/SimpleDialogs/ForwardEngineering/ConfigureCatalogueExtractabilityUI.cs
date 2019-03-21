// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Tutorials;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.TransparentHelpSystem;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{

    /// <summary>
    /// This dialog is shown when the RDMP learns about a new data table in your data repository that you want it to curate.  This can be either following a the successful flat file import
    /// or after selecting an existing table for importing metadata from (See ImportSQLTable).
    /// 
    /// <para>If you click 'Cancel' then no dataset (Catalogue) will be created and you will only have the TableInfo/ColumnInfo collection stored in your RDMP database, you will need to manually wire
    /// these up to a Catalogue or delete them if you decied you want to make the dataset extractable later on. </para>
    /// 
    /// <para>Alternatively you can create a new Catalogue, this will result in a Catalogue (dataset) of the same name as the table and a CatalogueItem being created for each ColumnInfo imported.
    /// If you choose to you can make these CatalogueItems extractable by creating ExtractionInformation too or you may choose to do this by hand later on (in CatalogueItemTab).  It is likely that
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
        private Catalogue _catalogue;
        private TableInfo _tableInfo;
        private bool _choicesFinalised;
        private HelpWorkflow _workflow;
        private CatalogueItem[] _catalogueItems;
        private bool _ddChangeAllChanged = false;
        
        /// <summary>
        /// the Project to associate the Catalogue with to make it ProjectSpecific (probably null)
        /// </summary>
        private Project _projectSpecific;

        public Catalogue CatalogueCreatedIfAny { get { return _catalogue; }}
        public TableInfo TableInfoCreated{get { return _tableInfo; }}

        private BinderWithErrorProviderFactory _binder;
        
        ObjectSaverButton objectSaverButton1 = new ObjectSaverButton();

        public ConfigureCatalogueExtractabilityUI(IActivateItems activator, TableInfo tableInfo,string initialDescription, Project projectSpecificIfAny):this(activator)
        {
            _tableInfo = tableInfo;
            Initialize(activator, initialDescription, projectSpecificIfAny);
        }

        public ConfigureCatalogueExtractabilityUI(IActivateItems activator, ITableInfoImporter importer, string initialDescription, Project projectSpecificIfAny):this(activator)
        {
            ColumnInfo[] cols;
            importer.DoImport(out _tableInfo, out cols);

            Initialize(activator,initialDescription,projectSpecificIfAny);
        }

        private ConfigureCatalogueExtractabilityUI(IActivateItems activator):base(activator)
        {
            InitializeComponent();
        }

        private void Initialize(IActivateItems activator,  string initialDescription, Project projectSpecificIfAny)
        {
            CommonFunctionality.SetItemActivator(activator);

            var cols = _tableInfo.ColumnInfos;
            
            var forwardEngineer = new ForwardEngineerCatalogue(_tableInfo, cols, false);
            ExtractionInformation[] eis;
            forwardEngineer.ExecuteForwardEngineering(out _catalogue, out _catalogueItems, out eis);

            tbDescription.Text = initialDescription + " (" + Environment.UserName + " - " + DateTime.Now + ")";
            tbTableName.Text = _tableInfo.Name;
            _catalogue.SaveToDatabase();
            
            if (_binder == null)
            {
                _binder = new BinderWithErrorProviderFactory(activator);
                _binder.Bind(tbCatalogueName,"Text",_catalogue,"Name",false,DataSourceUpdateMode.OnPropertyChanged, c=>c.Name);
                _binder.Bind(tbAcronym, "Text", _catalogue, "Acronym", false, DataSourceUpdateMode.OnPropertyChanged, c => c.Acronym);
                _binder.Bind(tbDescription, "Text", _catalogue, "Description", false, DataSourceUpdateMode.OnPropertyChanged, c => c.Description);
            }

            //Every CatalogueItem is either mapped to a ColumnInfo (not extractable) or a ExtractionInformation (extractable).  To start out with they are not extractable
            foreach (CatalogueItem ci in _catalogueItems)
                olvColumnExtractability.AddObject(new Node(ci, cols.Single(col => ci.ColumnInfo_ID == col.ID)));

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

            olvColumnInfoName.ImageGetter = ImageGetter;
            olvColumnExtractability.RebuildColumns();
            
            if (Activator.RepositoryLocator.DataExportRepository == null)
                gbProjectSpecific.Enabled = false;
            else
            {
                SelectProject(projectSpecificIfAny);
                pbProject.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Project);
            }

            ddIsExtractionIdentifier.Items.Add("<<None>>");
            ddIsExtractionIdentifier.Items.AddRange(olvColumnExtractability.Objects.OfType<Node>().ToArray());

            CommonFunctionality.AddHelp(btnPickProject, "IExtractableDataSet.Project_ID", "Project Specific Datasets");
            
            objectSaverButton1.SetupFor(this,_catalogue,Activator.RefreshBus);
        }


        private void IsExtractionIdentifier_AspectPutter(object rowobject, object newvalue)
        {
            var n = (Node) rowobject;

            if (n.ExtractionInformation == null)
                MakeExtractable(n, true, ExtractionCategory.Core);

            Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
            n.ExtractionInformation.IsExtractionIdentifier = (bool)newvalue;
            n.ExtractionInformation.SaveToDatabase();
        }

        private object ImageGetter(object rowObject)
        {
            var n = (Node) rowObject;

            return Activator.CoreIconProvider.GetImage((object) n.ExtractionInformation ?? n.ColumnInfo);
        }

        private object IsExtractionIdentifier_AspectGetter(object rowObject)
        {
            var n = (Node)rowObject;

            if (n.ExtractionInformation == null)
                return false;

            return n.ExtractionInformation.IsExtractionIdentifier;
        }


        private void MakeExtractable(object o, bool shouldBeExtractable, ExtractionCategory? category = null)
        {
            var n = (Node)o;
            
            //if it has extraction information
            if(n.ExtractionInformation != null)
            {
                if(shouldBeExtractable)
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
                else
                {
                    //make it not extractable by deleting the extraction information
                    n.ExtractionInformation.DeleteInDatabase();
                    n.ExtractionInformation = null;
                }
            }
            else
            {
                //it doesn't have ExtractionInformation

                if(!shouldBeExtractable) //it's already not extractable job done
                    return;
                else
                {
                   //make it extractable
                    var newExtractionInformation = new ExtractionInformation((ICatalogueRepository) n.ColumnInfo.Repository, n.CatalogueItem, n.ColumnInfo,n.ColumnInfo.Name);

                    if (category.HasValue)
                    {
                        newExtractionInformation.ExtractionCategory = category.Value;
                        newExtractionInformation.Order = olvColumnExtractability.IndexOf(n);
                        newExtractionInformation.SaveToDatabase();
                    }

                    n.ExtractionInformation = newExtractionInformation;
                }
            }

            olvColumnExtractability.RefreshObject(n);
        }


        private object ExtractionCategoryAspectGetter(object rowobject)
        {
            var n = (Node)rowobject;

            if (n.ExtractionInformation == null)
                return "Not Extractable";

            return n.ExtractionInformation.ExtractionCategory;
        }


        private void TlvColumnExtractabilityOnCellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
        {
            var n = (Node)cellEditEventArgs.RowObject;

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
                cbx.SelectedItem = n.ExtractionInformation != null ? (object) n.ExtractionInformation.ExtractionCategory : NotExtractable;
                cellEditEventArgs.Control = cbx;
            }
        }

        private void TlvColumnExtractabilityOnCellEditFinishing(object sender, CellEditEventArgs cellEditEventArgs)
        {
            var n = (Node) cellEditEventArgs.RowObject;

            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = (ComboBox) cellEditEventArgs.Control;

                if (Equals(cbx.SelectedItem, NotExtractable))
                    MakeExtractable(n, false, null);
                else
                    MakeExtractable(n, true, (ExtractionCategory) cbx.SelectedItem);
            }
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvColumnExtractability.UseFiltering = true;

            var textFilter = new TextMatchFilter(olvColumnExtractability, tbFilter.Text);
            textFilter.Columns = new[] {olvColumnInfoName};
            olvColumnExtractability.ModelFilter = textFilter;
        }

        private void ddCategoriseMany_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filteredObjects = olvColumnExtractability.FilteredObjects.Cast<Node>().ToArray();
            object toChangeTo = ddCategoriseMany.SelectedItem;
            
            if (MessageBox.Show("Set " + filteredObjects.Length + " to '" + toChangeTo + "'?",
                    "Confirm Overwrite?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {

                foreach (object o in filteredObjects)
                {
                    if (toChangeTo.Equals(NotExtractable))
                        MakeExtractable(o, false);
                    else
                        MakeExtractable(o, true, (ExtractionCategory) toChangeTo);
                }

                _ddChangeAllChanged = true;
            }
        }

        private void FinaliseExtractability()
        {
            var eds = new ExtractableDataSet(Activator.RepositoryLocator.DataExportRepository, _catalogue);

            IAtomicCommandWithTarget cmd;
            if(_projectSpecific != null)
            {
                cmd = new ExecuteCommandMakeCatalogueProjectSpecific(Activator).SetTarget(_projectSpecific).SetTarget(_catalogue);
            
                if (!cmd.IsImpossible)
                    cmd.Execute();
                else
                    MessageBox.Show("Could not make Catalogue ProjectSpecific:" + cmd.ReasonCommandImpossible);
            }
        }

        private void btnAddToExisting_Click(object sender, EventArgs e)
        {
            var eis = GetExtractionInformations();

            if (!eis.Any())
            {
                MessageBox.Show("You must set at least one column to extractable before you can add them to another Catalogue");
                return;
            }
            

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(Activator.CoreChildProvider.AllCatalogues, false, false);
                if (dialog.ShowDialog() == DialogResult.OK)

                    if (MessageBox.Show("This will add " + eis.Length + " new columns to " + dialog.Selected + ". Are you sure this is what you want?","Add to existing", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        AddToExistingCatalogue((Catalogue) dialog.Selected,eis);
        }

        private void AddToExistingCatalogue(Catalogue addToInstead, ExtractionInformation[] eis)
        {
            //move all the CatalogueItems to the other Catalogue instead
            foreach (ExtractionInformation ei in eis)
            {
                var ci = ei.CatalogueItem;
                ci.Catalogue_ID = addToInstead.ID;
                ci.SaveToDatabase();
            }
            
            _choicesFinalised = true;
            _catalogue.DeleteInDatabase();
            _catalogue = null;

            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(addToInstead));

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            var eis = GetExtractionInformations();

            if (!eis.Any())
            {
                if(MessageBox.Show("You have not marked any columns as extractable, are you sure you want to create a dataset with no extractable columns?","Create with no extractable columns?",MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            else
            if (eis.Any(ei=>ei.IsExtractionIdentifier))
                FinaliseExtractability();
            else
            {
                if (MessageBox.Show("You have not chosen a column to be IsExtractionIdentifier, do you wish to continue?", "Confirm", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            }

            _choicesFinalised = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private ExtractionInformation[] GetExtractionInformations()
        {
            return olvColumnExtractability.Objects.Cast<Node>()
                .Where(n => n.ExtractionInformation != null)
                .Select(ei => ei.ExtractionInformation)
                .ToArray();
        }

        private void ConfigureCatalogueExtractabilityUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!_choicesFinalised)
            {
                if (MessageBox.Show("Your data table will still exist but no Catalogue will be created",
                    "Confirm", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    DialogResult = DialogResult.Cancel;
                    _catalogue.DeleteInDatabase();
                    _catalogue = null;

                    Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(TableInfoCreated));
                }
                else
                    e.Cancel = true;
            }
            else
            {
                if(CatalogueCreatedIfAny != null)
                    Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(CatalogueCreatedIfAny));
            }
        }

        private void ConfigureCatalogueExtractabilityUI_Load(object sender, EventArgs e)
        {
            _workflow = new HelpWorkflow(this, new Guid("74e6943e-1ed8-4c43-89c2-96158c1360fa"), new TutorialTracker(Activator));
            var stage1 = new HelpStage(olvColumnExtractability, "This is a collection of all the column definitions imported, change the Extractable status of one of the columns to make it extractable", () => GetExtractionInformations().Any());
            var stage2 = new HelpStage(ddIsExtractionIdentifier, "One of your columns should contain a patient identifier, select it here", () => GetExtractionInformations().Any(ei=>ei.IsExtractionIdentifier));
            var stage3 = new HelpStage(pChangeAll, "Change this dropdown to change all at once", () =>  _ddChangeAllChanged);
            var stage4 = new HelpStage(pFilter, "Type in here if you are trying to find a specific column", () => !string.IsNullOrWhiteSpace(tbFilter.Text));

            stage1.SetNext(stage2);
            stage2.SetNext(stage3);
            stage2.OptionButtonText = "I don't have one of those";
            stage2.OptionDestination = stage3;
            stage3.SetNext(stage4);

            _workflow.RootStage = stage1;

            helpIcon1.SetHelpText("Configure Extractability", "Click for tutorial", _workflow);
        }

        class Node
        {
            public CatalogueItem CatalogueItem;
            public ColumnInfo ColumnInfo;
            public ExtractionInformation ExtractionInformation;
            
            public Node(CatalogueItem ci, ColumnInfo col)
            {
                CatalogueItem = ci;
                ColumnInfo = col;
            }

            public override string ToString()
            {
                return CatalogueItem.Name;
            }
        }

        private void SelectProject(Project projectSpecificIfAny)
        {
            if (projectSpecificIfAny == null)
            {
                lblProject.Text = "<<None>>";
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
                SelectProject(null);
            else
            {

                var all = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>();
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(all, false, false);

                if (dialog.ShowDialog() == DialogResult.OK)
                    SelectProject((Project)dialog.Selected);
            }
            
        }

        private void ddIsExtractionIdentifier_SelectedIndexChanged(object sender, EventArgs e)
        {
            var n = ddIsExtractionIdentifier.SelectedItem as Node;

            //turn off all IsExtractionIdentifierness
            foreach (Node node in ddIsExtractionIdentifier.Items.OfType<Node>())
            {
                if (node.ExtractionInformation != null && node.ExtractionInformation.IsExtractionIdentifier)
                {
                    node.ExtractionInformation.IsExtractionIdentifier = false;
                    node.ExtractionInformation.SaveToDatabase();
                }
            }

            //we cleared them all, now did they want one selected (i.e. they selected anythign except <<None>>)
            if (n != null)
            {
                if(n.ExtractionInformation == null)
                    MakeExtractable(n, true, ExtractionCategory.Core);

                Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
                n.ExtractionInformation.IsExtractionIdentifier = true;
                n.ExtractionInformation.SaveToDatabase();
            }

            olvColumnExtractability.RebuildColumns();
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }
}
