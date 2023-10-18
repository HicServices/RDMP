// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NPOI.OpenXmlFormats.Dml.Diagram;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Point = System.Drawing.Point;

namespace Rdmp.UI.CohortUI.CreateHoldoutLookup;


/// <summary>
/// this is incorrect
/// Once you have created a cohort database, this dialog lets you upload a new cohort into it.  You will already have selected a file which contains the private patient identifiers of
/// those you wish to be in the cohort.  Next you must create or choose an existing Project for which the cohort belongs.
/// 
/// <para>Once you have chosen the project you can choose to either create a new cohort for use with the project (use this if you have multiple cohorts in the project e.g. 'Cases' and
/// 'Controls').  Or 'Revised version of existing cohort' for if you made a mistake with your first version of a cohort or if you are doing a refresh of the cohort (e.g. after 5 years
/// it is likely there will be different patients that match the research study criteria so a new version of the cohort is appropriate).</para>
/// </summary>
public partial class CreateHoldoutLookupUI : RDMPForm
{
    private readonly IExternalCohortTable _target;
    private IDataExportRepository _repository;
    private readonly CohortIdentificationConfiguration _cic;


    public string CohortDescription
    {
        get => tbDescription.Text;
        set => tbDescription.Text = value;
    }


    public CreateHoldoutLookupUI(IActivateItems activator, IExternalCohortTable target, IProject project = null, CohortIdentificationConfiguration cic = null) :
        base(activator)
    {
        _target = target;


        InitializeComponent();


        if (_target == null)
            return;


        _repository = (IDataExportRepository)_target.Repository;
        _cic = cic;
        SetProject(project);
        tbName.Text = $"holdout_{cic.Name}"; //todo this should be the existing  cohorts name


        taskDescriptionLabel1.SetupFor(new DialogArgs
        {
            TaskDescription =
                "Holdouts...do stuff todo"
        });
    }




    public CohortHoldoutLookupRequest Result { get; set; }
    public IProject Project { get; set; }


    private void btnOk_Click(object sender, EventArgs e)
    {
        string name = tbName.Text;
        string minDate = textBox2.Text;
        string maxDate = textBox3.Text;
        string dateColumnName = textBox4.Text;
        string whereQuery = textBox1.Text;
        Result = new CohortHoldoutLookupRequest(_cic, name, Decimal.ToInt32(numericUpDown1.Value), comboBox1.Text == "%", "", whereQuery, minDate, maxDate, dateColumnName);
        //see if it is passing checks
        var notifier = new ToMemoryCheckNotifier();
        //Result.Check(notifier);
        //if (notifier.GetWorst() <= CheckResult.Warning)
        //{
        DialogResult = DialogResult.OK;
        Close();
        //        }
        //        else
        //        {
        //            var bads = notifier.Messages.Where(c => c.Result == CheckResult.Fail);


        //            WideMessageBox.Show("Checks Failed",
        //                $@"Checks must pass before continuing:
        //- {string.Join($"{Environment.NewLine}- ", bads.Select(b => b.Message))}");


        //            //if it is not passing checks display the results of the failing checking
        //            ragSmiley1.Reset();
        //            Result.Check(ragSmiley1);
        //        }
    }


    private void btnCancel_Click(object sender, EventArgs e)
    {
        Result = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }




    private void CohortHoldoutCreationRequestUI_Load(object sender, EventArgs e)
    {
        _target.Check(ragSmiley1);
    }




    private void btnNewProject_Click(object sender, EventArgs e)
    {
        try
        {
            var p = new ProjectUI.ProjectUI();
            var dialog = new RDMPForm(Activator);


            p.SwitchToCutDownUIMode();


            var ok = new Button();
            ok.Click += (s, ev) =>
            {
                dialog.Close();
                dialog.DialogResult = DialogResult.OK;
            };
            ok.Location = new Point(0, p.Height + 10);
            ok.Width = p.Width / 2;
            ok.Height = 30;
            ok.Text = "Ok";


            var cancel = new Button();
            cancel.Click += (s, ev) =>
            {
                dialog.Close();
                dialog.DialogResult = DialogResult.Cancel;
            };
            cancel.Location = new Point(p.Width / 2, p.Height + 10);
            cancel.Width = p.Width / 2;
            cancel.Height = 30;
            cancel.Text = "Cancel";


            dialog.Controls.Add(ok);
            dialog.Controls.Add(cancel);


            dialog.Height = p.Height + 80;
            dialog.Width = p.Width + 10;
            dialog.Controls.Add(p);


            ok.Anchor = AnchorStyles.Bottom;
            cancel.Anchor = AnchorStyles.Bottom;


            var project = new Project(_repository, "New Project");
            p.SetDatabaseObject(Activator, project);
            var result = dialog.ShowDialog();
            result = DialogResult.OK; //temp
            if (result == DialogResult.OK)
            {
                //project.SaveToDatabase();
                //SetProject(project);
                Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(project));
            }
            else
            {
                //project.DeleteInDatabase();
            }
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }


    private void SetProject(IProject project)
    {
        Project = project;


        gbChooseCohortType.Enabled = true;
    }


    private void tbName_TextChanged(object sender, EventArgs e)
    {
    }


    private void btnExisting_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "Choose a Project which this cohort will be associated with.  This will set the cohorts ProjectNumber.  A cohort can only be extracted from a Project whose ProjectNumber matches the cohort (multiple Projects are allowed to have the same ProjectNumber)"
        }, Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>(), out var proj))
            SetProject(proj);
    }


    private void btnClear_Click(object sender, EventArgs e)
    {
        SetProject(null);
    }


    private void gbChooseCohortType_Enter(object sender, EventArgs e)
    {


    }


    private void label1_Click(object sender, EventArgs e)
    {


    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

    private void tbDescription_TextChanged(object sender, EventArgs e)
    {

    }
}