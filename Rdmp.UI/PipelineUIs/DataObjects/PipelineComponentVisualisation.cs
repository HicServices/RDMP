// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.PipelineUIs.Pipelines;


namespace Rdmp.UI.PipelineUIs.DataObjects;

/// <summary>
/// Shows the state of a single pipeline component within a data flow pipeline (See 'Pipelines' in UserManual.md).  This includes the Type of the pipeline
/// component, whether the software was able to create an instance of the type and whether it passed checking.  Components can be either a Source, Middle (of which there can be any
/// number) or Destination.  Objects (usually DataTables) flow through the pipeline and are processed by each component in turn.
/// 
/// <para>But first the pipeline must be constructed, this component is used as part of <see cref="PipelineDiagramUI"/> to let you adjust the order / add new components etc into your pipeline.  The pipeline
/// itself is stored as a reusable resource in the Catalogue Database. </para>
/// </summary>
[TechnicalUI]
internal class PipelineComponentVisualisation : DataFlowComponentVisualisation
{
    private bool _isSelected;

    private Pen _origFullPen;
    private Exception _exInitialization;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowDrag { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowSelection { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IPipelineComponent PipelineComponent { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            _fullPen = value ? new Pen(new SolidBrush(Color.FromArgb(0, 99, 177)), 7.0f) : _origFullPen;
            Invalidate(true);
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Exception ExInitialization
    {
        get => _exInitialization;
        set
        {
            _exInitialization = value;

            if (value != null)
                ragSmiley1.OnCheckPerformed(new CheckEventArgs("Could not initialize object", CheckResult.Fail,
                    _exInitialization));
        }
    }


    public PipelineComponentVisualisation(IPipelineComponent component, PipelineComponentRole role,
        object valueOrNullIfBroken, Exception constructionExceptionIfAny,
        Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> shouldAllowDrop)
        : base(role, valueOrNullIfBroken, shouldAllowDrop)
    {
        PipelineComponent = component;

        if (constructionExceptionIfAny != null)
            ragSmiley1.OnCheckPerformed(new CheckEventArgs("Failed to construct component", CheckResult.Fail,
                constructionExceptionIfAny));

        _origFullPen = _fullPen;

        if (component == null)
            return;

        lblText.Text = component.GetClassNameLastPart();

        if (valueOrNullIfBroken == null)
            ragSmiley1.OnCheckPerformed(new CheckEventArgs("Could not construct object", CheckResult.Fail));

        Width = lblText.PreferredWidth + 85;

        _isEmpty = false;

        MouseDown += Anywhere_MouseDown;
        lblText.MouseDown += Anywhere_MouseDown;
        prongRight1.MouseDown += Anywhere_MouseDown;
        prongRight2.MouseDown += Anywhere_MouseDown;
        prongLeft1.MouseDown += Anywhere_MouseDown;
        prongLeft2.MouseDown += Anywhere_MouseDown;
        pComponent.MouseDown += Anywhere_MouseDown;
    }


    public event PipelineComponentSelectedHandler ComponentSelected;

    private void Anywhere_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        if (AllowSelection)
        {
            IsSelected = true;
            ComponentSelected(this, PipelineComponent);
        }

        if (AllowDrag) DoDragDrop(this, DragDropEffects.Move);
    }
}