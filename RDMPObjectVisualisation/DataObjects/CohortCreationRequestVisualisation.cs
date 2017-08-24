using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Shown during cohort creation, indicates that a request to create the indicated cohort will be given to  the destination component in the pipeline.
    /// </summary>
    public partial class CohortCreationRequestVisualisation : UserControl
    {
        private CohortCreationRequest _cohortCreationRequest;

        public CohortCreationRequestVisualisation(CohortCreationRequest value)
        {
            _cohortCreationRequest = value;
            InitializeComponent();

            if (value != null)
                lblDescription.Text =  value.ToString();
        }

        private void ExternalCohortTableVisualisation_Load(object sender, EventArgs e)
        {
            checksUIIconOnly1.Check(_cohortCreationRequest);
        }
    }
}
