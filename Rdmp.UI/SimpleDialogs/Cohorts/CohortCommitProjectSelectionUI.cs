using NPOI.SS.Formula.Functions;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs.Cohorts
{
    public partial class CohortCommitProjectSelectionUI : RDMPForm
    {
        private readonly IProject _currentProject;
        private readonly Project[] _projects;
        private readonly IActivateItems _activator;
        public CohortCommitProjectSelectionUI(IActivateItems activator, IProject currentProject, Project[] projects)
        {
            InitializeComponent();
            _activator = activator;
            _currentProject = currentProject;
            _projects = projects;
            if (_currentProject != null)
            {
                btnCurrentProject.Text = $"This Project ({_currentProject.Name.Substring(0,Math.Min(10,_currentProject.Name.Length))}{(_currentProject.Name.Length>0?"...":"")})";
            }
            else
            {
                btnCurrentProject.Enabled = false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProject Result { get; set; }

        private void btnCurrentProject_Click(object sender, EventArgs e)
        {
            Result = _currentProject;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            var p = new ProjectUI.ProjectUI();
            var dialog = new RDMPForm(_activator);

            p.SwitchToCutDownUIMode();
            p.SetItemActivator(_activator);
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

            var project = new Project(_activator.RepositoryLocator.DataExportRepository, "New Project");
            p.SetDatabaseObject(_activator, project);
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                project.SaveToDatabase();
                if (_currentProject != null)
                {
                    var projectSpecificCatalogues = _currentProject.GetAllProjectCatalogues().Where(p => p.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository));
                    foreach (var psc in projectSpecificCatalogues)
                    {
                        var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator, psc, project, true);
                        cmd.Execute();
                    }
                }

                _activator.Publish(project);
                DialogResult = DialogResult.OK;
                Result = project;
                Close();
            }
        }
        private void btnExistingProject_Click(object sender, EventArgs e)
        {
            var selected = _activator.SelectOne(new DialogArgs
            {
                TaskDescription =
                    "Choose a Project which this cohort will be associated with.  This will set the cohorts ProjectNumber.  A cohort can only be extracted from a Project whose ProjectNumber matches the cohort (multiple Projects are allowed to have the same ProjectNumber)"
            }, _projects);
            if (selected != null)
            {
                Result = selected as Project;
                if (_currentProject != null)
                {
                    var projectSpecificCatalogues = _currentProject.GetAllProjectCatalogues().Where(p => p.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository));
                    foreach (var psc in projectSpecificCatalogues)
                    {
                        var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator, psc, Result, true);
                        cmd.Execute();
                    }
                }
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
