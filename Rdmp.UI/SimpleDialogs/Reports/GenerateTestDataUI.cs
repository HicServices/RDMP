// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BadMedicine;
using BadMedicine.Datasets;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.TransparentHelpSystem;
using Rdmp.UI.Tutorials;

namespace Rdmp.UI.SimpleDialogs.Reports;

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
public partial class GenerateTestDataUI : RDMPForm
{
    public HelpWorkflow HelpWorkflow { get; private set; }
    private int? _seed;

    public GenerateTestDataUI(IActivateItems activator, ICommandExecution command) : base(activator)
    {
        InitializeComponent();

        var yLoc = 0;

        foreach (var t in DataGeneratorFactory.GetAvailableGenerators())
        {
            var ui = new DataGeneratorUI
            {
                Generator = DataGeneratorFactory.Create(t.Type, new Random()),
                Location = new Point(0, yLoc)
            };
            yLoc += ui.Height;
            pDatasets.Controls.Add(ui);
        }

        lblDirectory.Visible = false;

        EnableOrDisableGoButton();

        if (command != null)
            BuildHelpWorkflow(command);

        helpIcon1.SetHelpText("Tutorial", "Click for tutorial", HelpWorkflow);
    }

    private void BuildHelpWorkflow(ICommandExecution command)
    {
        HelpWorkflow = new HelpWorkflow(this, command, new TutorialTracker(Activator));

        var ds =
            new HelpStage(pDatasets,
                "This control will allow you to create flat comma separated files with fictional data for a shared pool of patient identifiers.  Start by choosing the number of rows you want in each dataset e.g. 1,000,000");
        var pop =
            new HelpStage(pPopulationSize,
                "Now choose how many unique identifiers you want generated.  If your population pool is smaller than the number of records per dataset there will be a large overlap of patients between datasets while if it is larger the crossover will be sparser.");

        var location = new HelpStage(pOutputDirectory, @"Click browse to select a directory to create the 3 files in");

        var execute = new HelpStage(btnGenerate, "Click to start generating the flat files");

        ds.SetOption(">>", pop);
        pop.SetOption(">>", location);
        location.SetOption(">>", execute);

        HelpWorkflow.RootStage = ds;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ragSmileyPopulation.Visible = false;
        ragSmileyDirectory.Visible = false;
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
            if (populationSize < 1)
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
        else if (string.IsNullOrWhiteSpace(tbPopulationSize.Text) || !lblDirectory.Visible)
            btnGenerate.Enabled = false;
        else
            btnGenerate.Enabled = true;
    }

    private bool started;

    private List<DataGeneratorUI> Executing = new();
    private DirectoryInfo _extractDirectory;

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        var uis = pDatasets.Controls.OfType<DataGeneratorUI>().Where(static ui => ui.Generate).ToArray();

        if (uis.Length == 0)
        {
            MessageBox.Show("At least one dataset must be selected");
            return;
        }

        try
        {
            if (started)
            {
                MessageBox.Show("Generation already in progress");
                return;
            }

            started = true;


            var r = _seed.HasValue ? new Random(_seed.Value) : new Random();


            var identifiers = new PersonCollection();
            identifiers.GeneratePeople(populationSize, r);

            if (cbLookups.Checked)
                DataGenerator.WriteLookups(_extractDirectory);

            //run them at the same time
            if (!_seed.HasValue)
            {
                foreach (var ui in uis)
                {
                    Executing.Add(ui);
                    ui.BeginGeneration(identifiers, _extractDirectory);
                    var ui1 = ui;
                    ui.Completed += () =>
                    {
                        Executing.Remove(ui1);
                        AnnounceIfComplete();
                    };
                }
            }
            else
            {
                var queue = new Queue<DataGeneratorUI>(uis);
                Execute(identifiers, queue, queue.Dequeue(), r);
            }
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void Execute(PersonCollection identifiers, Queue<DataGeneratorUI> queue, DataGeneratorUI current, Random r)
    {
        if (current == null)
            return;

        //tell form it is running
        Executing.Add(current);

        //reset the current generator to use the seed provided
        current.Generator = DataGeneratorFactory.Create(current.Generator.GetType(), r);

        current.BeginGeneration(identifiers, _extractDirectory);

        //when it is complete
        current.Completed += () =>
        {
            if (queue.Count != 0)
                Execute(identifiers, queue, queue.Dequeue(), r);

            Executing.Remove(current);
            AnnounceIfComplete();
        };
    }

    private void AnnounceIfComplete()
    {
        if (Executing.Count == 0)
        {
            MessageBox.Show("Finished generating test data");
            Close();
        }
    }

    private void UserExercisesUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        //if it hasn't started let them close
        if (!started)
            return;

        //warn them and cancel if any of them are still generating data
        var stillRunning = Executing.FirstOrDefault();

        if (stillRunning != null)
        {
            MessageBox.Show($"{stillRunning.Generator.GetType().Name} is still generating data");
            e.Cancel = true;
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


    private void TbSeed_TextChanged(object sender, EventArgs e)
    {
        try
        {
            _seed = int.Parse(tbSeed.Text);
            tbSeed.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            _seed = null;
            tbSeed.ForeColor = Color.Red;
        }
    }
}