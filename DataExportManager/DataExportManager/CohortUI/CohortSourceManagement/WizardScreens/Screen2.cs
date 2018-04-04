using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.CohortDatabaseWizard;

namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    /// <summary>
    /// Allows you to specify the private and release identifier column name/datatypes for the cohort database you are creating.  It is anticipated that you will have some datasets already
    /// configured in the Data Catalogue Database and have marked your patient identifier columns as IsExtractionIdentifier (See ExtractionInformationUI, ImportSQLTable and 
    /// ForwardEngineerCatalogue).
    /// 
    /// <para>All your private patient identifier fields should have the same name and datatype (e.g. 'NHSNumber' char(10)).  If you have a large legacy architecture and you cannot standardise
    /// the names of your private identifier columns then you can just alias them '[MyTable].[MyCol] as MyColRenamed' in ExtractionInformationUI (but this is not recommended).  Press
    /// the button at the top of the screen to list all the identifier columns in your datasets.  Select the correct name/datatype.</para>
    /// 
    /// <para>After you have chosen the correct private identifier you should choose a strategy for allocating release identifiers.  Because each agency handles governance and identifier assignment
    /// differently you can select 'Leave Blank' and provide your own implementation by editing the resulting cohort database manually (advanced topic).  Or you can select from one of the 
    /// two 'out of the box' solutions:</para>
    /// 
    /// <para>1. Auto Incrementing Int - generates an autonum column for the release identifier (Also known as magic numbers).
    /// 2. Guid - generates a new Globally Unique Identifier String for each release identifier (e.g. 29e4506c-c8ad-48e5-b8ad-4fa999dcd3d3)</para>
    /// 
    /// <para>Between the two, Guid is recommended since it prevents you making mistakes if you ever have to deanonymise data since you cannot cross map against the wrong cohort database (like you
    /// could if you used autonums).</para>
    /// </summary>
    public partial class Screen2 : RDMPUserControl
    {
        public Screen2()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Wizard = new CreateNewCohortDatabaseWizard(RepositoryLocator.CatalogueRepository, RepositoryLocator.DataExportRepository);
        }

        public ReleaseIdentifierAssignmentStrategy Strategy { get; private set; }
        public CreateNewCohortDatabaseWizard Wizard { get; private set; }
        public PrivateIdentifierPrototype PrivateIdentifierPrototype { get; private set; }

        private void btnDiscoverExtractionIdentifiers_Click(object sender, EventArgs e)
        {
            listView1.ClearObjects();
            listView1.AddObjects(Wizard.GetPrivateIdentifierCandidates());

            if (listView1.GetItemCount() == 0)
                MessageBox.Show("It looks like none of the ExtractionInformations in your Catalogue database are marked as IsExtractionIdentifier");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prototype = listView1.SelectedObject as PrivateIdentifierPrototype;
            PrivateIdentifierPrototype = prototype;

            if (prototype == null)
            {
                cohortSourceDiagram1.SetPrivateIdentifierText("");
                groupBox1.Enabled = false;
            }
            else
            {
                cohortSourceDiagram1.SetPrivateIdentifierText(prototype.RuntimeName + " " + prototype.DataType);
                groupBox1.Enabled = true;
            }
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = true;

            if(rbAutoIncrementing.Checked)
                Strategy = ReleaseIdentifierAssignmentStrategy.Autonum;
            else
            if (rbGuid.Checked)
                Strategy = ReleaseIdentifierAssignmentStrategy.Guid;
            else if (rbIWantToHackTheReleaseIdentifierMyself.Checked)
                Strategy = ReleaseIdentifierAssignmentStrategy.LeaveBlank;
            else
                btnNext.Enabled = false;

            cohortSourceDiagram1.SetPublicIdentifierText(Wizard.GetReleaseIdentifierNameAndTypeAsSqlString(Strategy));
        }
    }
}
