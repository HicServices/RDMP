using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    /// <summary>
    /// Describes what a cohort is in terms of the RDMP (a list of patient identifiers for a project with accompanying release identifiers).  It is important that you understand
    /// what a cohort is and how the RDMP will use the cohort database you are creating so that you can make the correct decisions in Screen2.
    /// </summary>
    public partial class Screen1 : UserControl
    {
        public Screen1()
        {
            InitializeComponent();
        }
    }
}
