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
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.PipelineUIs.Pipelines;

/// <inheritdoc cref="IPipelineSelectionUI" />
public partial class PipelineSelectionUI : UserControl, IPipelineSelectionUI
{
    private readonly IActivateItems _activator;
    private IPipelineUseCase _useCase;
    private readonly ICatalogueRepository _repository;

    private IPipeline _pipeline;
    public event Action PipelineDeleted = delegate { };

    public event EventHandler PipelineChanged;
    private IPipeline _previousSelection;

    private readonly IExtractionConfiguration _extractionConfiguration;

    private ToolTip tt = new();

    private const string ShowAll = "Show All/Incompatible Pipelines";
    public bool showAll = false;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IPipeline Pipeline
    {
        get => _pipeline;
        set
        {
            _pipeline = value;
            if (ddPipelines == null) return;

            if (_extractionConfiguration is not null && value is not null)
            {
                _extractionConfiguration.DefaultPipeline_ID = value.ID;
                _extractionConfiguration.SaveToDatabase();
            }

            ddPipelines.SelectedItem = value;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get => gbPrompt.Text;
        set => gbPrompt.Text = value;
    }

    /// <summary>
    /// Refresh the list of pipeline components
    /// </summary>
    private void RefreshPipelineList()
    {
        ddPipelines.Items.Clear();

        //add pipelines sorted alphabetically
        var allPipelines = _repository.GetAllObjects<Pipeline>().OrderBy(p => p.Name).ToArray();

        ddPipelines.Items.Add("<<None>>");

        ddPipelines.Items.AddRange(allPipelines.Where(_useCase.IsAllowable).ToArray());
        ddPipelines.Items.Add(ShowAll);

        if (showAll)
            ddPipelines.Items.AddRange(allPipelines.Where(o => !_useCase.IsAllowable(o)).ToArray());

        if(_extractionConfiguration is not null)
        {
            var toReselect = ddPipelines.Items.OfType<Pipeline>().SingleOrDefault(p => p.ID == _extractionConfiguration.DefaultPipeline_ID);

            //if we can reselect the users previously selected one
            if (toReselect != null)
            {
                ddPipelines.SelectedItem = toReselect;
                return;
            }
        }

        //reselect if it is still there
        else if (ddPipelines.SelectedItem is Pipeline before)
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
            ? ddPipelines.Items.OfType<Pipeline>().Single()
            : "<<None>>";
    }

    public PipelineSelectionUI(IActivateItems activator, IPipelineUseCase useCase, ICatalogueRepository repository, IExtractionConfiguration extractionConfiguration=null)
    {
        _activator = activator;
        _useCase = useCase;
        _repository = repository;
        _extractionConfiguration=extractionConfiguration;
        InitializeComponent();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) //don't connect to database in design mode
            return;

        RefreshPipelineList();

        tt.SetToolTip(btnClonePipeline, "Create a new copy of the selected pipeline");
        tt.SetToolTip(btnEditPipeline, "Change which components are run in the Pipeline and with what settings");

        ddPipelines.DrawMode = DrawMode.OwnerDrawFixed;
        ddPipelines.DrawItem += cmb_Type_DrawItem;
    }

    private void cmb_Type_DrawItem(object sender, DrawItemEventArgs e)
    {
        e.DrawBackground();

        var italic = new Font(ddPipelines.Font, FontStyle.Italic);

        if (e.Index == -1)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Pink), e.Bounds);
            TextRenderer.DrawText(e.Graphics, "Select Pipeline", italic,
                new Rectangle(new Point(e.Bounds.Left, e.Bounds.Top + 1), e.Bounds.Size), Color.Black,
                TextFormatFlags.Left);
            return;
        }

        var render = ddPipelines.Items[e.Index].ToString();
        var isIncompatible = e.Index > ddPipelines.Items.IndexOf(ShowAll);


        if (Equals(ddPipelines.Items[e.Index], ShowAll))
        {
            // draw a line along the top
            e.Graphics.DrawLine(Pens.CornflowerBlue, new Point(e.Bounds.Left, e.Bounds.Top + 1),
                new Point(e.Bounds.Right, e.Bounds.Top + 1));

            if (showAll) render = $"\u2713 {render}";

            TextRenderer.DrawText(e.Graphics, render, italic,
                new Rectangle(new Point(e.Bounds.Left, e.Bounds.Top + 1), e.Bounds.Size), Color.CornflowerBlue,
                TextFormatFlags.Left);
        }
        else
        {
            TextRenderer.DrawText(e.Graphics, render, isIncompatible ? italic : ddPipelines.Font, e.Bounds,
                ddPipelines.ForeColor, TextFormatFlags.Left);
        }

        e.DrawFocusRectangle();
    }

    private void groupBox1_Enter(object sender, EventArgs e)
    {
    }

    private void ddPipelines_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Equals(ddPipelines.SelectedItem, ShowAll))
        {
            // toggle show all
            showAll = !showAll;
            RefreshPipelineList();
            // Force dropdown to show again with newly refreshed list
            ddPipelines.DroppedDown = true;
            return;
        }

        Pipeline = ddPipelines.SelectedItem as Pipeline;

        tbDescription.Text = Pipeline == null ? "" : Pipeline.Description;

        btnEditPipeline.Enabled = Pipeline != null;
        btnDeletePipeline.Enabled = Pipeline != null;
        btnClonePipeline.Enabled = Pipeline != null;

        if (!Equals(_previousSelection, Pipeline))
        {
            PipelineChanged?.Invoke(this, EventArgs.Empty);
            _previousSelection = Pipeline;
        }
    }

    private void btnEditPipeline_Click(object sender, EventArgs e)
    {
        ShowEditPipelineDialog();
    }

    private void btnCreateNewPipeline_Click(object sender, EventArgs e)
    {
        Pipeline = new Pipeline(_repository, $"TO DO:Name this pipeline!{Guid.NewGuid()}");
        ddPipelines.Items.Add(Pipeline);
        ddPipelines.SelectedItem = Pipeline;

        ShowEditPipelineDialog();
    }

    private void ShowEditPipelineDialog()
    {
        //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
        var dialog = new ConfigurePipelineUI(_activator, Pipeline, _useCase, _repository);
        dialog.ShowDialog();

        ddPipelines.Items.Remove(Pipeline);

        Pipeline = _repository.GetObjectByID<Pipeline>(Pipeline.ID);
        ddPipelines.Items.Add(Pipeline);
        ddPipelines.SelectedItem = Pipeline;

        // user may have edited it so raise the changed event
        PipelineChanged?.Invoke(this, EventArgs.Empty);
    }

    private void btnDeletePipeline_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show($"Are you sure you want to delete {Pipeline.Name}? ", "Confirm deleting pipeline?",
                MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
        {
            Pipeline.DeleteInDatabase();
            Pipeline = null;
            PipelineDeleted();
            RefreshPipelineList();
        }
    }

    private void btnClonePipeline_Click(object sender, EventArgs e)
    {
        if (ddPipelines.SelectedItem is Pipeline p)
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
        Height = 28;

        Controls.Remove(gbPrompt);

        Controls.Add(ddPipelines);
        ddPipelines.Location = new Point(0, 3);
        ddPipelines.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        foreach (var button in new Control[]
                     { btnEditPipeline, btnCreateNewPipeline, btnClonePipeline, btnDeletePipeline })
        {
            Controls.Add(button);
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