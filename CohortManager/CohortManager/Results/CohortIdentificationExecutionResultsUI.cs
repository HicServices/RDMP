using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using CohortManagerLibrary.Execution;
using ReusableUIComponents;
using UserControl = System.Windows.Forms.UserControl;

namespace CohortManager.Results
{
    /// <summary>
    /// Getting the cohort correct for research projects is one of the hardest and most important parts of being a data analyst.  The RDMP helps with this by break down the study criteria
    /// into manageable testable sets (See CohortCompilerUI).  Ensuring each set is 100% correct is very important, you can do this after the set has executed successfully by 
    /// selecting it in CohortCompilerUI and looking at the 'Dataset Sample' in this control.
    /// 
    /// The 'Dataset Sample' is a preview of the top 1000 records returned by the set.  The 'Identifiers' is a list of all the unique patients which fit the set criteria.  
    /// 
    /// If you select a Set Operation then you will see the Identifiers but no 'Dataset Sample', this is because Set Operations operate across datasets by applying operations (UNION,
    ///  INTERSECT, EXCEPT) to the patient identifiers not the row data and therefore there is no sample data.
    /// </summary>
    public partial class CohortIdentificationExecutionResultsUI : UserControl
    {
        private CohortIdentificationTaskExecution _taskExecution;

        public CohortIdentificationTaskExecution TaskExecution
        {
            get { return _taskExecution; }
            set
            {
                _taskExecution = value;


                //clear the data tables
                dataGridView2.DataSource = null;
                dataGridView2.Rows.Clear();

                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();

                //if we have results
                if (value != null)
                {
                    //set preview (might be null - e.g. for containers there is no preview possible as it is cross dataset with disperate columns)
                    dataGridView2.DataSource = value.Preview;

                    //if there are identifiers
                    if(value.Identifiers != null)
                    {
                        //tell user how many there aree
                        lblRowCountIdentifiers.Text = "Cohort Row Count:" + value.Identifiers.Rows.Count;

                        //if there are a LOT of identifiers then we don't want to crash the users computer trying to load 1,000,000,000 rows into a data grid
                        if (value.Identifiers.Rows.Count > 1000)
                        {
                            //load only 1000 identifiers
                            dataGridView1.DataSource = value.Identifiers.Rows.Cast<System.Data.DataRow>().Take(1000).CopyToDataTable();

                            //but tell them that's what we did
                            lblRowCountIdentifiers.Text += "(Only 1000 displayed)";
                        }
                        else
                            dataGridView1.DataSource = value.Identifiers;//there are less than 1000 identifiers - just add those.
                    }
                    else
                    {

                        dataGridView1.DataSource = null;
                        dataGridView1.Rows.Clear();
                    }
                }
            }
        }

        public CohortIdentificationExecutionResultsUI()
        {
            InitializeComponent();
        }

        private void btnSaveResults_Click(object sender, EventArgs e)
        {
            if(_taskExecution.Identifiers == null)
                return;

            if(!_taskExecution.IsResultsForRootContainer)
                if(MessageBox.Show("This list is not the Root Container for the Cohort, are you sure you want to save it?","Confirm saving subset of cohort only?",MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma Separated Values|*.csv";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(sfd.FileName);

                    sw.WriteLine(_taskExecution.Identifiers.Columns[0].ColumnName);
                    foreach (DataRow dr in _taskExecution.Identifiers.Rows)
                        sw.WriteLine(dr[0]);

                    sw.Flush();
                    sw.Close();
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
            }

            
        }
    }
}