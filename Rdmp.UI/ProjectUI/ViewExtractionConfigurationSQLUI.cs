// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.ExtractionTime.Commands;
using Rdmp.Core.DataExport.ExtractionTime.UserPicks;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;

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
                ExtractDatasetCommand request = new ExtractDatasetCommand(_extractionConfiguration,new ExtractableDatasetBundle(_extractableDataSet));
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
