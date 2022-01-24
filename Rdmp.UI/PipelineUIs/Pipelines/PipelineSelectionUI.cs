// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Annotations;

namespace Rdmp.UI.PipelineUIs.Pipelines
{
    /// <inheritdoc cref="IPipelineSelectionUI" />
    public partial class PipelineSelectionUI : UserControl, IPipelineSelectionUI
    {
        private readonly IActivateItems _activator;
        private IPipelineUseCase _useCase;
        private readonly ICatalogueRepository _repository;
        
        private IPipeline _pipeline;
        public event Action PipelineDeleted = delegate { };
        
        public event EventHandler PipelineChanged;
        IPipeline _previousSelection = null;

        ToolTip tt = new ToolTip();

        const string ShowAll = "Show All Pipelines";
        public bool showAll = false;
        public IPipeline Pipeline
        {
            get { return _pipeline; }
            set
            {
                _pipeline = value;

                if (ddPipelines != null)
                    ddPipelines.SelectedItem = value;
            }
        }

        public override string Text
        {
            get { return gbPrompt.Text; }
            set { gbPrompt.Text = value; }
        }

        private void RefreshPipelineList()
        {

            var before = ddPipelines.SelectedItem as Pipeline;

            ddPipelines.Items.Clear();
            
            var context = _useCase.GetContext();
            
            //add pipelines sorted alphabetically
            var allPipelines = _repository.GetAllObjects<Pipeline>().OrderBy(p=>p.Name).ToArray();

            ddPipelines.Items.Add("<<None>>");

            ddPipelines.Items.AddRange(context == null || showAll 
                ? allPipelines.ToArray() //no context/show incompatible enabled so add all pipelines
                : allPipelines.Where(_useCase.IsAllowable).ToArray()); //only compatible components
            
            //reselect if it is still there
            if (before != null)
            {
                var toReselect = ddPipelines.Items.OfType<Pipeline>().SingleOrDefault(p => p.ID == before.ID);

                //if we can reselect the users previously selected one
                if (toReselect != null)
                {
                    ddPipelines.SelectedItem = toReselect;
                    return;
                }
            }

            //if there is only one pipeline select it
            ddPipelines.SelectedItem = ddPipelines.Items.OfType<Pipeline>().Count() == 1
                ? (object) ddPipelines.Items.OfType<Pipeline>().Single()
                : "<<None>>";

            // add horizontal line
            var width = ddPipelines.Items.Cast<object>().Max(o => o.ToString().Length);

            // add show all
            ddPipelines.Items.Add(ShowAll);
        }
        
        public PipelineSelectionUI(IActivateItems activator,IPipelineUseCase useCase, ICatalogueRepository repository)
        {
            _activator = activator;
            _useCase = useCase;
            _repository = repository;
            InitializeComponent();
            
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) //dont connect to database in design mode
                return;

            RefreshPipelineList();

            tt.SetToolTip(btnClonePipeline,"Create a new copy of the selected pipeline");
            tt.SetToolTip(btnEditPipeline, "Change which components are run in the Pipeline and with what settings");

            ddPipelines.DrawMode = DrawMode.OwnerDrawFixed;
            ddPipelines.DrawItem += new DrawItemEventHandler(cmb_Type_DrawItem);

        }

        void cmb_Type_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if(e.Index == -1)
            {
                return;
            }

            var render = ddPipelines.Items[e.Index].ToString();

            if (Equals(ddPipelines.Items[e.Index],ShowAll))
            {
                // draw a line along the top
                e.Graphics.DrawLine(Pens.CornflowerBlue, new Point(e.Bounds.Left, e.Bounds.Top + 1), new Point(e.Bounds.Right, e.Bounds.Top + 1));

                var italic = new Font(ddPipelines.Font, FontStyle.Italic);

                if(showAll)
                {
                    render = "✓ " + render;
                }   

                TextRenderer.DrawText(e.Graphics, render, italic , new Rectangle(new Point(e.Bounds.Left, e.Bounds.Top + 1), e.Bounds.Size), Color.CornflowerBlue, TextFormatFlags.Left);
            }
            else
            {
                TextRenderer.DrawText(e.Graphics, render, ddPipelines.Font, e.Bounds, ddPipelines.ForeColor, TextFormatFlags.Left);
            }

            e.DrawFocusRectangle();
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        

        private void ddPipelines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Equals(ddPipelines.SelectedItem , ShowAll))
            {
                // toggle show all
                showAll = !showAll;
                RefreshPipelineList();
                return;
            }

            Pipeline = ddPipelines.SelectedItem as Pipeline;

            if (Pipeline == null)
                tbDescription.Text = "";
            else
                tbDescription.Text = Pipeline.Description;
            
            btnEditPipeline.Enabled = Pipeline != null;
            btnDeletePipeline.Enabled = Pipeline != null;
            btnClonePipeline.Enabled = Pipeline != null;

            if(!Equals(_previousSelection,Pipeline))
            {
                PipelineChanged?.Invoke(this,new EventArgs());
                _previousSelection = Pipeline;
            }
                
        }

        private void btnEditPipeline_Click(object sender, EventArgs e)
        {
            ShowEditPipelineDialog();
        }

        private void btnCreateNewPipeline_Click(object sender, EventArgs e)
        {
            Pipeline = new Pipeline(_repository, "TO DO:Name this pipeline!" + Guid.NewGuid());
            ddPipelines.Items.Add(Pipeline);
            ddPipelines.SelectedItem = Pipeline;

            ShowEditPipelineDialog();
        }

        private void ShowEditPipelineDialog()
        {
            //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
            var dialog = new ConfigurePipelineUI(_activator,Pipeline,_useCase, _repository);
            dialog.ShowDialog();

            ddPipelines.Items.Remove(Pipeline);

            Pipeline = _repository.GetObjectByID<Pipeline>(Pipeline.ID);
            ddPipelines.Items.Add(Pipeline);
            ddPipelines.SelectedItem = Pipeline;
        }

        private void btnDeletePipeline_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to delete " + Pipeline.Name + "? ","Confirm deleting pipeline?",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                Pipeline.DeleteInDatabase();
                Pipeline = null;
                PipelineDeleted();
                RefreshPipelineList();
            }
        }

        private void btnClonePipeline_Click(object sender, EventArgs e)
        {
            var p = ddPipelines.SelectedItem as Pipeline;

            if (p != null)
            {
                var clone = p.Clone();
                RefreshPipelineList();
                
                //select the clone
                ddPipelines.SelectedItem = clone;
            }
        }

        
        /// <summary>
        /// Turns the control into a single line ui control
        /// </summary>
        [UsedImplicitly]
        public void CollapseToSingleLineMode()
        {
            this.Height = 28;

            this.Controls.Remove(gbPrompt);

            this.Controls.Add(ddPipelines);
            ddPipelines.Location = new Point(0, 3);
            ddPipelines.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            foreach (var button in new Control[] { btnEditPipeline, btnCreateNewPipeline, btnClonePipeline, btnDeletePipeline })
            {
                this.Controls.Add(button);
                button.Location = new Point(2, 2);
                button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }

            btnDeletePipeline.Left = Width - btnDeletePipeline.Width;
            btnClonePipeline.Left = btnDeletePipeline.Left - btnClonePipeline.Width;
            btnCreateNewPipeline.Left = btnClonePipeline.Left - btnCreateNewPipeline.Width;
            btnEditPipeline.Left = btnCreateNewPipeline.Left - btnEditPipeline.Width;

            ddPipelines.Width = btnEditPipeline.Left - 2;

        }
    }
}
