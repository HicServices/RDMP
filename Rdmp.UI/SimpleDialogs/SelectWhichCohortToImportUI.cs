// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// Allows you to select a cohort from your Cohort Database which is not yet been imported into the RDMP.  The RDMP lets you import cohorts directly into the cohort database through
    /// its user interface in which case a reference is created to the cohort in the cohort database once it has been successfully committed (so you won't need this dialog) (See 
    /// CohortCreationRequestUI).
    /// 
    /// <para>Only use this dialog if you have manually added a cohort into your cohort database yourself and RDMP does not show it in SavedCohortsCollectionUI.</para>
    /// </summary>
    public partial class SelectWhichCohortToImportUI : RDMPForm
    {
        private readonly ExternalCohortTable _source;
        public int IDToImport { get; private set; }

        private readonly string _projectNumberMemberName;
        private readonly string _versionMemberName;
        private readonly string _displayMember;
        private readonly string _valueMember;

        public SelectWhichCohortToImportUI(IActivateItems activator,ExternalCohortTable source):base(activator)
        {
            _source = source;
            InitializeComponent();
            
            if(source == null)
                return;

            
            DataTable dt = ExtractableCohort.GetImportableCohortDefinitionsTable(source,out _displayMember,out _valueMember,out _versionMemberName, out _projectNumberMemberName);

            dataGridView1.DataSource = dt;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                MessageBox.Show("Select a cohort to import or click Cancel");
                return;
            }

            var idUserPlansToImport = (int) dataGridView1.SelectedRows[0].Cells[_valueMember].Value;

            var existing = Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableCohort>("ExternalCohortTable_ID" , _source.ID ,ExpressionType.AndAlso,  "OriginID" , idUserPlansToImport);

            if (existing.Any())
            {
                MessageBox.Show("That cohort has already been imported");
                return;
            }

            IDToImport = idUserPlansToImport;

            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(tbFilter.Text))
                {
                    lblFilteringBy.Text = "";
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = null;
                    return;
                }
                
                int? number = null;

                try
                {
                    number = int.Parse(tbFilter.Text);
                }
                catch (Exception)
                {
                    //it's not a number
                    number = null;
                }

                string filter = null;


                //filter is numerical
                if (number != null)
                {
                    filter = string.Format("{0} LIKE '%{3}%' OR {1} = {3} OR {2} = {3}",
                        _displayMember,
                        _projectNumberMemberName,
                        _valueMember,
                        number);

                    lblFilteringBy.Text = "Filtering by Number";
                }
                    else
                {
                    filter = string.Format("{0} LIKE '%{1}%'",
                        _displayMember,
                        tbFilter.Text);

                    lblFilteringBy.Text = "Filtering by Text";
                }
                
                ((DataTable) dataGridView1.DataSource).DefaultView.RowFilter = filter;
                
                tbFilter.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbFilter.ForeColor = Color.Red;
                lblFilteringBy.Text = "Filter Error";
            }
        }


    }
}
