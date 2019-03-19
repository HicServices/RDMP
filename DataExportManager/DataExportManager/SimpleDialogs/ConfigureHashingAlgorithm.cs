// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Managers;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data;
using DataExportLibrary.Repositories.Managers;
using MapsDirectlyToDatabaseTable;
using CatalogueManager.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace DataExportManager.SimpleDialogs
{
    /// <summary>
    /// Any column in a data extraction which is marked with 'Hash On Data Release' (See ExtractionInformationUI) will be wrapped with this SQL string.  Use this to call a scalar valued
    /// function which generates hash strings based on the column value and the project number (salt).
    /// 
    /// <para>For example Work.dbo.HicHash({0},{1}) would wrap column names such that the column name appeared in the {0} and the project number appeared in {1}.  For this to work you must have
    /// a database Work and a scalar function called HicHash (this is just an example, you can call the function whatever you want and adjust it accordingly).  You don't have to use the
    /// salt if you don't want to either, if you don't add a {1} then you won't get a salt argument into your scalar function.</para>
    /// 
    /// <para>This is quite technical if you don't know what a Scalar Function is in SQL then you probably don't want to do hashing and instead you might want to just not extract these columns
    /// or configure them with the RDMP ANO system (See ANOTable).</para>
    /// </summary>
    public partial class ConfigureHashingAlgorithm : RDMPForm
    {
        private readonly IDataExportRepository _dataExportRepository;

        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        public Scintilla QueryPreview { get; set; }

        public ConfigureHashingAlgorithm(IDataExportRepository dataExportRepository)
        {
            _dataExportRepository = dataExportRepository;
            InitializeComponent();
            
            if(VisualStudioDesignMode)
                return;

            QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryPreview.ReadOnly = true;

            panel2.Controls.Add(QueryPreview);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            //get the current hashing algorithm
            string value = _dataExportRepository.DataExportPropertyManager.GetValue(DataExportProperty.HashingAlgorithmPattern);
            tbHashingAlgorithm.Text = value;
        }

        private void tbHashingAlgorithm_TextChanged(object sender, EventArgs e)
        {
            string pattern = tbHashingAlgorithm.Text;

            try
            {
                QueryPreview.ReadOnly = false;
                QueryPreview.Text = String.Format(pattern, "[TEST]..[ExampleColumn]", "123");
                _dataExportRepository.DataExportPropertyManager.SetValue(DataExportProperty.HashingAlgorithmPattern, pattern);
                
            }
            catch (Exception exception)
            {
                QueryPreview.Text = ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            }
            finally
            {
                QueryPreview.ReadOnly = true;
            }
        }

        private void btnReferenceColumn_Click(object sender, EventArgs e)
        {
            tbHashingAlgorithm.Text = tbHashingAlgorithm.Text + "{0}";
        }

        private void btnReferenceSalt_Click(object sender, EventArgs e)
        {
            tbHashingAlgorithm.Text = tbHashingAlgorithm.Text + "{1}";
        }
    }
}
