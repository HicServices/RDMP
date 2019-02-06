// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Tutorials;
using Diagnostics.TestData.Exercises;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.TransparentHelpSystem;
using ReusableUIComponents.TransparentHelpSystem.ProgressTracking;


namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Lets you generate interesting test data in which to practice tasks such as importing data, generating cohorts and performing project extractions.  Note that ALL the data 
    /// generated is completely fictional.  Test data is generated randomly usually around a distribution (e.g. there are more prescriptions for Paracetamol/Aspirin than Morphine) but
    /// complex relationships are not modelled (e.g. there's no concept of someone being diabetic so just because someone is on INSULIN doesn't mean they will have diabetic blood tests 
    /// in biochemistry).  Likewise don't be surprised if people change address after they have died.
    ///
    /// <para>Identifiers are created from a central random pool and will be unique.  This means if you generate test data and then generate more tomorrow you are likely to only 
    /// have very minimal intersection of patient identifiers.  For this reason it is important not to generate and load Prescribing one day and then generate and load Biochemistry the 
    /// next day (instead you should generate all the data at once and use that as a reusable asset).</para>
    /// 
    /// <para>Make sure to put a PopulationSize that is lower than the number of records you want to create in each dataset so that there are multiple records per person (will make analysis more
    /// interesting/realistic).</para>
    /// </summary>
    public partial class GenerateTestDataUI : Form, IHelpWorkflowUser
    {
        private readonly IActivateItems _activator;
        public HelpWorkflow HelpWorkflow { get; private set; }

        public GenerateTestDataUI(IActivateItems activator, ICommandExecution command)
        {
            _activator = activator;
            InitializeComponent();

            PrescribingExerciseTestData prescribing = new PrescribingExerciseTestData();
            sizePrescribing.Generator = prescribing;

            BiochemistryExerciseTestData biochemistry = new BiochemistryExerciseTestData();
            sizeBiochemistry.Generator = biochemistry;

            DemographyExerciseTestData demography = new DemographyExerciseTestData();
            sizeDemography.Generator = demography;

            lblDirectory.Visible = false;

            EnableOrDisableGoButton();

            if(command != null)
                BuildHelpWorkflow(command);
        }

        private void BuildHelpWorkflow(ICommandExecution command)
        {
            
            HelpWorkflow = new HelpWorkflow(this, command, new TutorialTracker(_activator));

            var _bio =
                new HelpStage(gbBiochemistry,
                    "This control will allow you to create 3 flat comma separated files with fictional data for a shared pool of patient identifiers.  Start by choosing the number of rows you want in the biochemistry dataset e.g. 1,000,000");
            var _pres =
                new HelpStage(gbPrescribing, "Now do the same for Prescriptions");

            var _demog =
                new HelpStage(gbDemography,"Now do the same for Demography.  This dataset is 'known addresses' for patients.  So will have multiple records per person");

            var _pop =
                new HelpStage(pPopulationSize, "Now choose how many unique identifiers you want generated.  If your population pool is smaller than the number of records per dataset there will be a large overlap of patients between datasets while if it is larger the crossover will be sparser.");

            var _location = new HelpStage(pOutputDirectory,@"Click browse to select a directory to create the 3 files in");

            var _execute = new HelpStage(btnGenerate, "Click to start generating the flat files");

            HelpWorkflow.RootStage = _bio;

            sizeBiochemistry.TrackBarMouseUp += ()=>HelpWorkflow.ShowStage(_pres);
            sizePrescribing.TrackBarMouseUp += () => HelpWorkflow.ShowStage(_demog);
            sizeDemography.TrackBarMouseUp += () => tbPopulationSize.Focus();

            tbPopulationSize.GotFocus += (s, e) => HelpWorkflow.ShowStage(_pop);
            tbPopulationSize.LostFocus += (s, e) =>
            {
                if (!ragSmileyPopulation.IsFatal())
                    btnBrowse.Focus();
            };

            btnBrowse.GotFocus += (s, e) => HelpWorkflow.ShowStage(_location);
            btnBrowse.LostFocus += (s, e) =>
            {
                if (ragSmileyDirectory.Visible)
                {
                    HelpWorkflow.ShowStage(_execute);
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ragSmileyPopulation.Visible = false;
            ragSmileyDirectory.Visible = false;

            if (HelpWorkflow != null)
                HelpWorkflow.Start();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private int populationSize;

        private void tbPopulationSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                populationSize = int.Parse(tbPopulationSize.Text);
                if(populationSize < 1)
                    throw new IndexOutOfRangeException();

                tbPopulationSize.ForeColor = Color.Black;

                ragSmileyPopulation.Visible = true;
                ragSmileyPopulation.Reset();

            }
            catch (Exception ex)
            {
                tbPopulationSize.ForeColor = Color.Red;
                ragSmileyPopulation.Visible = true;
                ragSmileyPopulation.Fatal(ex);
            }
            EnableOrDisableGoButton();
        }

        private void EnableOrDisableGoButton()
        {
            if (tbPopulationSize.ForeColor == Color.Red)
                btnGenerate.Enabled = false;
            else
            if (string.IsNullOrWhiteSpace(tbPopulationSize.Text) || !lblDirectory.Visible)
                btnGenerate.Enabled = false;
            else
                btnGenerate.Enabled = true;

        }

        private bool started = false;
        
        private List<TestDataGenerator> Executing = new List<TestDataGenerator>();
        private DirectoryInfo _extractDirectory;

        private void btnGenerate_Click(object sender, EventArgs e)
        {

            //we are done
            HelpWorkflow.Abandon();

            try
            {
                if (started)
                {
                    MessageBox.Show("Generation already in progress");
                    return;
                }

                started = true;

                ExerciseTestIdentifiers identifiers = new ExerciseTestIdentifiers();
                identifiers.GeneratePeople(populationSize);

                string biochem = Path.Combine(_extractDirectory.FullName, "biochemistry.csv");
                string prescribing = Path.Combine(_extractDirectory.FullName, "prescribing.csv");
                string demography = Path.Combine(_extractDirectory.FullName, "demography.csv");

                Executing.Add(sizeBiochemistry);
                Executing.Add(sizePrescribing);
                Executing.Add(sizeDemography);

                sizeBiochemistry.BeginGeneration(identifiers, new FileInfo(biochem));
                sizePrescribing.BeginGeneration(identifiers, new FileInfo(prescribing));
                sizeDemography.BeginGeneration(identifiers, new FileInfo(demography));

                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(_extractDirectory);

                sizeBiochemistry.Completed += () => { Executing.Remove(sizeBiochemistry); AnnounceIfComplete(); };
                sizePrescribing.Completed += () => { Executing.Remove(sizePrescribing); AnnounceIfComplete(); };
                sizeDemography.Completed += () => { Executing.Remove(sizeDemography); AnnounceIfComplete(); };

            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void AnnounceIfComplete()
        {
            if(!Executing.Any())
            {
                MessageBox.Show("Finished generating test data");
                Close();
            }
        }
        
        private void UserExercisesUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if it hasn't started let them close
            if(!started)
                return;

            //warn them and cancel if any of them are still generating data
            if (Executing.Contains(sizeBiochemistry))
            {
                MessageBox.Show("Biochemistry is still generating data");
                e.Cancel = true;
                return;
            }

            if (Executing.Contains(sizePrescribing))
            {
                MessageBox.Show("Prescribing is still generating data");
                e.Cancel = true;
                return;
            }

            if (Executing.Contains(sizeDemography))
            {
                MessageBox.Show("Demography is still generating data");
                e.Cancel = true;
                return;
            }

        }
        
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var browserDialog = new FolderBrowserDialog();
            var result = browserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var directoryInfo = new DirectoryInfo(browserDialog.SelectedPath);
                _extractDirectory = directoryInfo;

                lblDirectory.Visible = true;
                lblDirectory.Text = directoryInfo.FullName;

                btnBrowse.Left = lblDirectory.Right + 5;
                ragSmileyDirectory.Visible = true;
                ragSmileyDirectory.Left = btnBrowse.Right + 5;

                EnableOrDisableGoButton();
            }
        }
    }
}
