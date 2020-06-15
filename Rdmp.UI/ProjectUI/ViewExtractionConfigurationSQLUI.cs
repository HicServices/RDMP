// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.DataViewing;
using Rdmp.UI.DataViewing.Collections.Arbitrary;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.DataAccess;


namespace Rdmp.UI.ProjectUI
{
    /// <summary>
    /// Shows you the SQL that will currently be executed as part of the ExtractionConfiguration of a data export project dataset.  This includes the join against the project cohort and any
    /// filters on the extraction dataset itself (e.g. extract only prescriptions from 2001 onwards)
    /// </summary>
    public partial class ViewExtractionConfigurationSQLUI : ViewExtractionConfigurationSQLUI_Design
    {
        private ScintillaNET.Scintilla QueryEditor;
        private IExtractionConfiguration _extractionConfiguration;
        private IExtractableDataSet _extractableDataSet;
        
        ToolStripButton btnRun = new ToolStripButton("Run",CatalogueIcons.ExecuteArrow);
        private ExtractDatasetCommand _request;

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
            btnRun.Click += BtnRun_Click;
        }

        
        private void RegenerateCodeInQueryEditor()
        {
            try
            {
                if(_extractionConfiguration.Cohort_ID == null)
                    throw new Exception("No cohort has been defined for this ExtractionConfiguration");

                //We are generating what the extraction SQL will be like, that only requires the dataset so empty bundle is fine
                _request = new ExtractDatasetCommand(_extractionConfiguration,new ExtractableDatasetBundle(_extractableDataSet));
                _request.GenerateQueryBuilder();

                QueryEditor.ReadOnly = false;

                //get the SQL from the query builder 
                QueryEditor.Text = _request.QueryBuilder.SQL;
                QueryEditor.ReadOnly = true;
                CommonFunctionality.ScintillaGoRed(QueryEditor,false);
            }
            catch (Exception ex)
            {
                CommonFunctionality.ScintillaGoRed(QueryEditor,ex);
            }
        }

        public override void SetDatabaseObject(IActivateItems activator, SelectedDataSets databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _extractionConfiguration = databaseObject.ExtractionConfiguration;
            _extractableDataSet = databaseObject.ExtractableDataSet;
            RegenerateCodeInQueryEditor();


            CommonFunctionality.Add(btnRun);
        }
        private void BtnRun_Click(object sender, EventArgs e)
        {
            var t = _request?.QueryBuilder?.TablesUsedInQuery?.FirstOrDefault();

            if(t == null)
                Activator.Show("Could not determine what table underlies the ExtractionConfiguration");
            else
            {
                Activator.Activate<ViewSQLAndResultsWithDataGridUI>(
                    new ArbitraryTableExtractionUICollection(t.Discover(DataAccessContext.InternalDataProcessing))
                    {
                        OverrideSql = QueryEditor.Text
                    });
            }
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
