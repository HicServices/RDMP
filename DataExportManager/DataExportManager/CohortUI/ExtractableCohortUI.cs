using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
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
        internal event ChangesSavedHandler ChangesSaved;

        private ExtractableCohort _extractableCohort; 
        
        public ExtractableCohort ExtractableCohort
        {
            get { return _extractableCohort; }
            private set {
                if (VisualStudioDesignMode)
                    return;

                _extractableCohort = value;

                //if the object passed in was null we set it to "" otherwise we are going to set it to the Name property (unless that is null in which case it's still going to end up as "")
                tbID.Text = value == null ? "" : value.ID.ToString();
                
                tbOverrideReleaseIdentifierSQL.Text = value == null ? "" : value.OverrideReleaseIdentifierSQL;
                
                auditLogEditor.Text = value != null ? value.AuditLog : "";
                GenerateSQLPreview();
            }
        }

        private void GenerateSQLPreview()
        {
            if(VisualStudioDesignMode)
                return;
            
            QueryPreview.ReadOnly = false;
            try
            {
                if (ExtractableCohort == null)
                {
                    QueryPreview.Text = "";
                    return;
                }

                string toShow = "";

                DiscoveredDatabase location = ExtractableCohort.GetDatabaseServer();
                //tell user about connection string (currently we don't support usernames/passwords so it's fine
                toShow += "/*Cohort is stored in Server " + location.Server.Name + " Database " + location.GetRuntimeName() +"*/" + Environment.NewLine;
                toShow += Environment.NewLine;

                IExternalCohortTable externalCohortTable = ExtractableCohort.ExternalCohortTable;
                
                string sql = "SELECT * FROM " + externalCohortTable.TableName +
                             Environment.NewLine
                             + " WHERE " + ExtractableCohort.WhereSQL();

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

            pSqlPreview.Controls.Add(QueryPreview); 

            AssociatedCollection = RDMPCollection.SavedCohorts;
        }

        private void AuditLogEditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            if (ExtractableCohort != null)
            {
                ExtractableCohort.AuditLog = auditLogEditor.Text;
                ExtractableCohort.SaveToDatabase();
            }
        }

        [DesignerSerializationVisibilityAttribute( DesignerSerializationVisibility.Hidden)]
        public Scintilla QueryPreview { get; set; }

        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        private Scintilla auditLogEditor;

        

        public override void SetDatabaseObject(IActivateItems activator, ExtractableCohort databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            ExtractableCohort = databaseObject;
            AssociatedCollection = RDMPCollection.SavedCohorts;
        }

        private void tbOverrideReleaseIdentifierSQL_TextChanged(object sender, EventArgs e)
        {
            if (ExtractableCohort != null)
            {
                var syntax = ExtractableCohort.GetQuerySyntaxHelper();

                if (
                    !string.IsNullOrWhiteSpace(tbOverrideReleaseIdentifierSQL.Text)//if it has an override
                    &&
                    syntax.GetRuntimeName(tbOverrideReleaseIdentifierSQL.Text)
                        .Equals(syntax.GetRuntimeName(ExtractableCohort.GetPrivateIdentifier())))//and that ovoerride is the same as the private identifier they are trying to release identifiable data on the sly!
                {
                    //release identifier cannot be the same as private identififer (I AM THE LAW!)
                    tbOverrideReleaseIdentifierSQL.ForeColor = Color.Red;
                    return;
                }

                tbOverrideReleaseIdentifierSQL.ForeColor = Color.Black;
                ExtractableCohort.OverrideReleaseIdentifierSQL = tbOverrideReleaseIdentifierSQL.Text;
                ExtractableCohort.SaveToDatabase();
            }
        }

        private void btnImportCustomDataFile_Click(object sender, EventArgs e)
        {
            
        }
        
        private void btnImportPatientIndexTable_Click(object sender, EventArgs e)
        {
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractableCohortUI_Design, UserControl>))]
    public abstract class ExtractableCohortUI_Design : RDMPSingleDatabaseObjectControl<ExtractableCohort>
    {
    }
}
