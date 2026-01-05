// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.PipelineUIs.Pipelines;

/// <summary>
/// Allows you to name, describe and configure a Data Flow Pipeline (IPipeline).  This is a flow of objects (usually DataTables) from a Source through 0 or more Components to a Destination.
/// Depending on the context the source and/or/neither destination may be fixed.  There will also be zero or more initialization objects which components can consume.  For example if you
/// are trying to build a pipeline to import a FlatFileToLoad into your database then you might use a DelimitedFlatFileDataFlowSource component to read the file (assuming it wasn't fixed
/// width or a database file or anything weird) and a DataTableUploadDestination to put it into the endpoint.
/// </summary>
public partial class ConfigurePipelineUI : Form
{
    private readonly IPipeline _pipeline;
    private readonly IPipelineUseCase _useCase;

    private PipelineWorkAreaUI _workArea;

    public ConfigurePipelineUI(IActivateItems activator, IPipeline pipeline, IPipelineUseCase useCase,
        ICatalogueRepository repository)
    {
        _pipeline = pipeline;
        _useCase = useCase;
        InitializeComponent();

        _workArea = new PipelineWorkAreaUI(activator, pipeline, useCase, repository) { Dock = DockStyle.Fill };
        panelWorkArea.Controls.Add(_workArea);

        tbName.Text = pipeline.Name;
        tbDescription.Text = pipeline.Description;

        RefreshUIFromDatabase();

        KeyPreview = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.W))
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }


    private void RefreshUIFromDatabase()
    {
        _workArea.SetTo(_pipeline, _useCase);
    }


    private void tbName_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbName.Text))
            tbName.Text = "NoName";

        if (tbName.Text.StartsWith("TO DO:") || tbName.Text.Equals("NoName"))
            tbName.ForeColor = Color.Red;
        else
            tbName.ForeColor = Color.Black;

        try
        {
            _pipeline.Name = tbName.Text;
            _pipeline.SaveToDatabase();
        }
        catch (Exception)
        {
            tbName.ForeColor = Color.Red;
        }
    }

    private void tbDescription_TextChanged(object sender, EventArgs e)
    {
        _pipeline.Description = tbDescription.Text;

        try
        {
            _pipeline.SaveToDatabase();
            tbDescription.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            tbDescription.ForeColor = Color.Red;
        }
    }
}