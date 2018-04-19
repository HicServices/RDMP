using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;
using HIC.Common.Validation.Constraints.Primary;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Shows you the SQL that will currently be executed as part of the ExtractionConfiguration of a data export project dataset.  This includes the join against the project cohort and any
    /// filters on the extraction dataset itself (e.g. extract only prescriptions from 2001 onwards)
    /// </summary>
    public partial class ViewExtractionConfigurationSQLUI : ViewExtractionConfigurationSQLUI_Design
    {
        private ScintillaNET.Scintilla QueryEditor;
        private ExtractionConfiguration _extractionConfiguration;
        private ExtractableDataSet _extractableDataSet;


        public ViewExtractionConfigurationSQLUI()
        {
            InitializeComponent();

            if(VisualStudioDesignMode)
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create();
            
            Controls.Add(QueryEditor);
            
            bool before = QueryEditor.ReadOnly;
            QueryEditor.ReadOnly = false;
            QueryEditor.Text = "";
            QueryEditor.ReadOnly = before;

            AssociatedCollection = RDMPCollection.DataExport;

        }

        private void RegenerateCodeInQueryEditor()
        {
            try
            {
                if(_extractionConfiguration.Cohort_ID == null)
                    throw new Exception("No cohort has been defined for this ExtractionConfiguration");

                //We are generating what the extraction SQL will be like, that only requires the dataset so empty bundle is fine
                ExtractDatasetCommand request = new ExtractDatasetCommand(RepositoryLocator,_extractionConfiguration,new ExtractableDatasetBundle(_extractableDataSet));
                request.GenerateQueryBuilder();

                QueryEditor.ReadOnly = false;

                //get the SQL from the query builder 
                QueryEditor.Text = request.QueryBuilder.SQL;
             
                QueryEditor.ReadOnly = true;
            }
            catch (Exception ex)
            {
                QueryEditor.ReadOnly = false;
                QueryEditor.Text = ex.ToString();
                QueryEditor.ReadOnly = true;
            }
        }

        public override void SetDatabaseObject(IActivateItems activator, SelectedDataSets databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _extractionConfiguration = databaseObject.ExtractionConfiguration;
            _extractableDataSet = databaseObject.ExtractableDataSet;
            RegenerateCodeInQueryEditor();
        }

        public override string GetTabName()
        {
            return base.GetTabName() + "Extraction SQL";

        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewExtractionConfigurationSQLUI_Design, UserControl>))]
    public abstract class ViewExtractionConfigurationSQLUI_Design:RDMPSingleDatabaseObjectControl<SelectedDataSets>
    {
    }
}
