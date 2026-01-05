// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.ReusableLibraryCode.Checks;


namespace Rdmp.UI.PipelineUIs.DataObjects;

/// <summary>
/// TECHNICAL: Base class for PipelineComponentVisualisation but can also include empty components (no type selected yet)
/// </summary>
[TechnicalUI]
public partial class DataFlowComponentVisualisation : UserControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object Value { get; set; }

    private readonly PipelineComponentRole _role;

    private ICheckable _checkable;
    private MandatoryPropertyChecker _mandatoryChecker;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsLocked
    {
        get => pbPadlock.Visible;
        set => pbPadlock.Visible = value;
    }

    private readonly Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> _shouldAllowDrop;

    public DataFlowComponentVisualisation()
        : this(PipelineComponentRole.Middle, null, null)
    {
        if (LicenseManager.UsageMode !=
            LicenseUsageMode
                .Designtime) //don't connect to database in design mode unless they passed in a fist full of nulls
            throw new NotSupportedException("Do not use this constructor, it is for use by Visual Studio Designer");
    }

    public PipelineComponentRole GetRole() => _role;

    public DataFlowComponentVisualisation(PipelineComponentRole role, object value,
        Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> shouldAllowDrop)
    {
        Value = value;
        _role = role;
        _shouldAllowDrop = shouldAllowDrop;
        InitializeComponent();

        _fullPen.Width = 2;
        _emptyPen.Width = 2;
        _emptyPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

        if (value == null)
        {
            IsLocked = false; //cannot be locked AND empty!
            _isEmpty = true;
            lblText.Text = "Empty";
            AllowDrop = _shouldAllowDrop != null;
        }
        else
        {
            _checkable = value as ICheckable;
            _mandatoryChecker = new MandatoryPropertyChecker(value);
            lblText.Text = value.ToString(); //.GetType().Name;
            GenerateToolTipBasedOnProperties(value);
        }

        switch (role)
        {
            case PipelineComponentRole.Source:
                prongLeft1.Visible = false;
                prongLeft2.Visible = false;
                break;
            case PipelineComponentRole.Middle:
                break;
            case PipelineComponentRole.Destination:
                prongRight1.Visible = false;
                prongRight2.Visible = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role));
        }


        Width = lblText.PreferredWidth + 80;
    }

    private ToolTip _toolTip = new();

    private void GenerateToolTipBasedOnProperties(object value)
    {
        var toolTip = new StringBuilder();

        //get all the properties that must be set on AnySeparatorFileAttacher (Those marked with the attribute DemandsInitialization
        var propertiesWeHaveToSet =
            value.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(DemandsInitializationAttribute), true).Any())
                .ToArray();
        foreach (var p in propertiesWeHaveToSet)
        {
            var propValue = p.GetValue(value);

            if (propValue == null || string.IsNullOrWhiteSpace(propValue.ToString()))
                propValue = "<NotSet>";

            toolTip.AppendLine($"{p.Name}:{propValue}");
        }

        var result = toolTip.ToString();

        if (!string.IsNullOrWhiteSpace(result))
            _toolTip.SetToolTip(lblText,
                $"{lblText.Text}{Environment.NewLine}Arguments:{Environment.NewLine}{result}");
    }

    protected bool _isEmpty;
    private Pen _emptyPen = new(new SolidBrush(Color.Black));
    protected Pen _fullPen = new(new SolidBrush(Color.Black));


    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);

        e.Graphics.DrawRectangle(_isEmpty ? _emptyPen : _fullPen, pComponent.Bounds);
    }

    private void DataFlowComponentVisualisation_DragEnter(object sender, DragEventArgs e)
    {
        if (_shouldAllowDrop == null)
            return;

        var shouldAllow = _shouldAllowDrop(e, this);

        if (shouldAllow != DragDropEffects.None)
            pbInsertHere.Visible = true;

        e.Effect = shouldAllow;
    }

    private void DataFlowComponentVisualisation_DragLeave(object sender, EventArgs e)
    {
        pbInsertHere.Visible = false;
    }

    public virtual void Check()
    {
        try
        {
            _checkable?.Check(ragSmiley1);
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }
    }

    public void CheckMandatoryProperties()
    {
        try
        {
            _mandatoryChecker?.Check(ragSmiley1);
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }
    }
}