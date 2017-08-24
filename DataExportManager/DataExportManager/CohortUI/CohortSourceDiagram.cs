using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExportManager.CohortUI
{
    /// <summary>
    /// Preview of the cohort table schema that will be used when creating a new cohort database using CreateNewCohortDatabaseWizardUI.  For example if you select a private identifier
    /// of 'varchar(10)' and a release identifier of GUIDs then the datatypes will appear in this control.  If you change to autonums for your release identifiers the release ID field
    /// will change to indicate the new datatype that will be created.
    /// </summary>
    public partial class CohortSourceDiagram : UserControl
    {
        public CohortSourceDiagram()
        {
            InitializeComponent();
            lblPrivateId.Text = "";
            lblReleaseId.Text = "";
        }

        public void SetPrivateIdentifierText(string privateIdDatatype)
        {
            lblPrivateId.Text = privateIdDatatype;
        }

        public void SetPublicIdentifierText(string releaseIdDatatype)
        {
            lblReleaseId.Text = releaseIdDatatype;
        }


    }
}
