using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.CohortSourceManagement.WizardScreens;
using DataExportLibrary.CohortDatabaseWizard;

namespace DataExportManager.CohortUI.CohortSourceManagement
{
    /// <summary>
    /// Wizard with three screens allowing you to create a Cohort database for use with Data Export Manager.  A cohort database is a database which stores all the patient identifier lists 
    /// for all the projects you have released data for.  A Cohort is a list of patient identifiers.  All identifiers must have the same datatype (If you handle two distinct patient 
    /// identifiers e.g. CHI number and NHS number then you can have 2 cohort databases... or you could just standardise with a mapping table and save yourself a lot of confusion) 
    /// 
    /// <para>This wizard offers 2 methods of allocating Release Identifiers (See Screen2) but because there can be company specific methods for allocating release identifiers (e.g. upload
    /// the private identifiers to an anonymisation web service and wait a week before downloading the corresponding release identifiers) the RDMP does not manage the cohort database 
    /// directly.  If you have such an obscure release identifier allocation policy tell Screen 3 to leave the release identifiers blank and write a process/plugin for updating the cohort
    /// table.</para>
    /// 
    /// <para>Data Export Manager will always link datasets against the private identifier and substitute it for the release identifier when extracting data.</para>
    /// 
    /// </summary>
    public partial class CreateNewCohortDatabaseWizardUI : RDMPUserControl
    {
        Screen1 screen1;
        Screen2 screen2;
        
        public ExternalCohortTable ExternalCohortTableCreatedIfAny { get; set; }

        public CreateNewCohortDatabaseWizardUI()
        {
            InitializeComponent();

            screen1 = new Screen1();
            screen2 = new Screen2();

            pStage.Controls.Clear();
            pStage.Controls.Add(screen1);

            screen1.btnOk.Click += btnOk_Click;
            screen2.btnBack.Click += btnBackScreen2_Click;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            screen2.RepositoryLocator = RepositoryLocator;

        }
        
        void btnBackScreen2_Click(object sender, EventArgs e)
        {
            pStage.Controls.Clear();
            pStage.Controls.Add(screen1);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            pStage.Controls.Clear();
            pStage.Controls.Add(screen2);
        }

    }
}
