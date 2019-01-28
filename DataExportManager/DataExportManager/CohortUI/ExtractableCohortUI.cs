using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using CatalogueManager.Copying;
using DataExportLibrary.Providers;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace DataExportManager.CohortUI
{
    
    /// <summary>
    /// Describes a named cohort in one of your Cohort Databases (you might only have 1 cohort database - See ExternalCohortTable).  Each cohort is associated with a specific
    /// Project.  Cohorts can have 'custom data', these are researcher datasets or datasets specific to the project extraction that are not needed for any other project (for example 
    /// questionnaire data which relates to the cohort).  
    /// 
    /// <para>The SQL window will show what SQL the QueryBuilder has produced to view the cohort and any accompanying custom data tables.  You can use this SQL to check that cohorts have the 
    /// correct identifiers in them etc.</para>
    ///  
    /// <para>You can upload new files as custom data for the selected cohort by clicking 'Import New Custom Data File For Cohort...'  This will let you select a file and run it through a 
    /// Pipeline to create a new data table in the cohort database that is like a project specific dataset.</para>
    /// 
    /// <para>A cohort is implemented as a private and release identifier column set and joined at data extraction time to your data repository datasets (the private identifiers are striped out
    /// and replaced with the corresponding patients project specific release identifier).  You can specify a 'OverrideReleaseIdentifierSQL', this will hijack what it says in the cohort
    /// database and do the release identifier substitution with the specific SQL you type instead (this is not recommended).  The use case for overriding would be if you have added some
    /// additional release identifier columns into your cohort table and want to use that column instead of the listed release identifier column (again this is a really bad idea).</para>
    /// 
    /// </summary>
    public partial class ExtractableCohortUI :ExtractableCohortUI_Design, ISaveableUI
    {
        private ExtractableCohort _extractableCohort; 
        
        RDMPCollectionCommonFunctionality _commonFunctionality1 = new RDMPCollectionCommonFunctionality();
        RDMPCollectionCommonFunctionality _commonFunctionality2 = new RDMPCollectionCommonFunctionality();

        private void GenerateSQLPreview()
        {
            if(VisualStudioDesignMode)
                return;
            
            QueryPreview.ReadOnly = false;
            try
            {
                string toShow = "";

                DiscoveredDatabase location = _extractableCohort.GetDatabaseServer();
                //tell user about connection string (currently we don't support usernames/passwords so it's fine
                toShow += "/*Cohort is stored in Server " + location.Server.Name + " Database " + location.GetRuntimeName() +"*/" + Environment.NewLine;
                toShow += Environment.NewLine;

                IExternalCohortTable externalCohortTable = _extractableCohort.ExternalCohortTable;
                
                string sql = "SELECT * FROM " + externalCohortTable.TableName +
                             Environment.NewLine
                             + " WHERE " + _extractableCohort.WhereSQL();

                toShow += Environment.NewLine;
                toShow += Environment.NewLine + "/*SQL to view cohort:*/" + Environment.NewLine;
                toShow += sql;

                QueryPreview.Text = toShow;

            }
            catch (Exception ex)
            {
                QueryPreview.Text = ExceptionHelper.ExceptionToListOfInnerMessages(ex, true);
            }
            finally
            {

                QueryPreview.ReadOnly = true;
            }
        }
        
        public ExtractableCohortUI()
        {
            InitializeComponent();
            
            if (VisualStudioDesignMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            auditLogEditor = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            pDescription.Controls.Add(auditLogEditor);
            auditLogEditor.TextChanged += AuditLogEditorOnTextChanged;

            QueryPreview = new ScintillaTextEditorFactory().Create();
            QueryPreview.ReadOnly = true;

            AssociatedCollection = RDMPCollection.SavedCohorts;

            helpIcon1.SetHelpText("Override Release Identifier","Not Recommended.  Setting this lets you change which release identifier column is extracted (for this cohort only).");
        }

        private void AuditLogEditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            _extractableCohort.AuditLog = auditLogEditor.Text;
        }

        [DesignerSerializationVisibilityAttribute( DesignerSerializationVisibility.Hidden)]
        public Scintilla QueryPreview { get; set; }

        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        private Scintilla auditLogEditor;

        

        public override void SetDatabaseObject(IActivateItems activator, ExtractableCohort databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _extractableCohort = databaseObject;

            //if the object passed in was null we set it to "" otherwise we are going to set it to the Name property (unless that is null in which case it's still going to end up as "")
            tbID.Text = _extractableCohort.ID.ToString();
            tbOriginId.Text = _extractableCohort.OriginID.ToString();
            tbOverrideReleaseIdentifierSQL.Text = _extractableCohort.OverrideReleaseIdentifierSQL;
            auditLogEditor.Text = _extractableCohort.AuditLog;

            tbProjectNumber.Text = _extractableCohort.ExternalProjectNumber.ToString();
            tbVersion.Text = _extractableCohort.ExternalVersion.ToString();

            GenerateSQLPreview();

            objectSaverButton1.SetupFor(this,_extractableCohort,activator.RefreshBus);

            var dx = _activator.CoreChildProvider as DataExportChildProvider;

            if (!_commonFunctionality1.IsSetup)
            {
                _commonFunctionality1.SetUp(RDMPCollection.None, tlvCohortUsage,activator,olvUsedIn,null,
                    new RDMPCollectionCommonFunctionalitySettings()
                    {
                        AddCheckColumn = false,
                        AddFavouriteColumn = false,
                        AddIDColumn = true,
                        AllowPinning = false,
                        SuppressActivate = false,
                        SuppressChildrenAdder = true
                    }
                    );
            }

            if (!_commonFunctionality2.IsSetup)
            {
                _commonFunctionality2.SetUp(RDMPCollection.None, tlvPreviousVersions, activator, olvOtherVersions, null,
                    new RDMPCollectionCommonFunctionalitySettings()
                    {
                        AddCheckColumn = false,
                        AddFavouriteColumn = false,
                        AddIDColumn = true,
                        AllowPinning = false,
                        SuppressActivate = false,
                        SuppressChildrenAdder = true
                    }
                    );
            }

            if(dx != null)
            {
                tlvCohortUsage.ClearObjects();
                tlvCohortUsage.AddObjects(dx.ExtractionConfigurations.Where(e=>e.Cohort_ID == _extractableCohort.ID).ToArray());
                
                tlvPreviousVersions.ClearObjects();
                tlvPreviousVersions.AddObjects(
                    dx.Cohorts.Where(
                        c =>
                            c.ID != _extractableCohort.ID &&
                            c.ExternalCohortTable_ID == _extractableCohort.ExternalCohortTable_ID && 
                            c.GetExternalData().ExternalDescription ==
                            _extractableCohort.GetExternalData().ExternalDescription &&
                            c.ExternalProjectNumber == _extractableCohort.ExternalProjectNumber).ToArray());
            }
            
            AssociatedCollection = RDMPCollection.SavedCohorts;
        }

        private void tbOverrideReleaseIdentifierSQL_TextChanged(object sender, EventArgs e)
        {

            var syntax = _extractableCohort.GetQuerySyntaxHelper();

            if (
                !string.IsNullOrWhiteSpace(tbOverrideReleaseIdentifierSQL.Text)//if it has an override
                &&
                syntax.GetRuntimeName(tbOverrideReleaseIdentifierSQL.Text)
                    .Equals(syntax.GetRuntimeName(_extractableCohort.GetPrivateIdentifier())))//and that ovoerride is the same as the private identifier they are trying to release identifiable data on the sly!
            {
                //release identifier cannot be the same as private identififer (I AM THE LAW!)
                tbOverrideReleaseIdentifierSQL.ForeColor = Color.Red;
                return;
            }

            tbOverrideReleaseIdentifierSQL.ForeColor = Color.Black;
            _extractableCohort.OverrideReleaseIdentifierSQL = tbOverrideReleaseIdentifierSQL.Text;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void btnShowProject_Click(object sender, EventArgs e)
        {
            var dx = (DataExportChildProvider) _activator.CoreChildProvider;

            var projects = dx.Projects.Where(p => p.ProjectNumber == _extractableCohort.ExternalProjectNumber).ToArray();

            if (!projects.Any())
                MessageBox.Show("No Projects exist with ProjectNumber " + _extractableCohort.ExternalProjectNumber);
            else if (projects.Length == 1)
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(projects.Single(), 1));
            else
            {
                SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(projects,false,false);
                if(dialog.ShowDialog() == DialogResult.OK)
                    _activator.RequestItemEmphasis(this, new EmphasiseRequest(dialog.Selected, 1));
            }
        }

        public override string GetTabName()
        {
            return _extractableCohort + " (V" + _extractableCohort.ExternalVersion +")";
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractableCohortUI_Design, UserControl>))]
    public abstract class ExtractableCohortUI_Design : RDMPSingleDatabaseObjectControl<ExtractableCohort>
    {
    }
}
