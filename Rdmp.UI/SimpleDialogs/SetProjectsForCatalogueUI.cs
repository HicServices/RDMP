using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using SynthEHR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class SetProjectsForCatalogueUI : Form
    {
        private readonly Catalogue _catalogue;
        private readonly IActivateItems _activator;
        private List<int> _linkedProjects;
        private List<int> _savedLinkedProjects;

        private readonly List<Project> _allProjects;
        public SetProjectsForCatalogueUI(IActivateItems activator, Catalogue catalogue)
        {
            InitializeComponent();
            _activator = activator;
            _catalogue = catalogue;
            _allProjects = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().ToList();
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);
            //_allProjects.AddRange(_allProjects);

            _linkedProjects = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Where(eds => eds.Project_ID != null).Select(eds => (int)eds.Project_ID).ToList();
            _savedLinkedProjects = _linkedProjects;
            Project.AspectGetter = obj => ((Project)obj).Name;
            ProjectID.AspectGetter = obj => ((Project)obj).ID;
            fastObjectListView1.CheckBoxes = true;
            fastObjectListView1.BooleanCheckStateGetter = delegate (Object rowObject) { 
                return _linkedProjects.Contains(((Project)rowObject).ID); 
            };
            fastObjectListView1.BooleanCheckStatePutter = delegate (Object rowObject, bool newValue) {
                if (_linkedProjects.Contains(((Project)rowObject).ID))
                {
                    //remove
                    _linkedProjects = _linkedProjects.Where(lp => lp != ((Project)rowObject).ID).ToList();
                }
                else
                {
                    //add
                    _linkedProjects.Add(((Project)rowObject).ID);
                }
                    return _linkedProjects.Contains(((Project)rowObject).ID); ; // return the value that you want the control to use
            };
            fastObjectListView1.BeginUpdate();
            fastObjectListView1.AddObjects(_allProjects);
            fastObjectListView1.EndUpdate();
        }

        private void OnItemCheck(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private bool Run(bool execute = false)
        {
            label2.Text = "";
            fastObjectListView1.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = false;
            var selectedProjects = _linkedProjects;
            var issues = new List<string>();
            foreach (var project in selectedProjects)
            {
                if (!_savedLinkedProjects.Contains(project))
                {
                    //new
                    var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator);
                    cmd.projectIdsToIgnore = selectedProjects.ToList();
                    cmd.SetTarget(_allProjects.First(p => p.ID == project));
                    cmd.SetTarget(_catalogue);
                    if (cmd.IsImpossible)
                    {
                        issues.Add(cmd.ReasonCommandImpossible);
                    }
                    else if (execute)
                    {
                        cmd.Execute();
                    }
                }
            }
            var selectedProjectIds = selectedProjects;
            var removedProjectIDs = _savedLinkedProjects.Where(lp => !selectedProjectIds.Contains(lp));
            foreach (var projectId in removedProjectIDs)
            {
                var project = _activator.RepositoryLocator.DataExportRepository.GetObjectByID<Project>(projectId);
                var cmd = new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, _catalogue, project);
                if (cmd.IsImpossible)
                {
                    issues.Add(cmd.ReasonCommandImpossible);
                }
                else if (execute)
                {
                    cmd.Execute();
                }
            }

            if (issues.Count == 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                label2.Text = string.Join("\r\n", issues);
            }
            fastObjectListView1.Enabled = true;
            button2.Enabled = true;
            return issues.Count == 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Run(true))
            {
                //checkedListBox1.Items.Clear();
                //_linkedProjects = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Where(eds => eds.Project_ID != null).Select(eds => (int)eds.Project_ID).ToList();
                //checkedListBox1.Items.AddRange(_allProjects.ToArray());
                //foreach (var selectedProjectId in _linkedProjects)
                //{
                //    checkedListBox1.SelectedItem = checkedListBox1.Items.OfType<Project>().ToList().FirstOrDefault(i => i.ID == selectedProjectId);
                //    checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, true);
                //    checkedListBox1.SelectedItem = null; // To clear selection if needed
                //}
                fastObjectListView1.Enabled = true;
                button2.Enabled = true;
                button1.Enabled = false;
            }
        }

        private void fastObjectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}