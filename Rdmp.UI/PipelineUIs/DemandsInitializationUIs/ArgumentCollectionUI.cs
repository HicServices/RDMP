// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;
using HelpIcon = Rdmp.UI.SimpleControls.HelpIcon;
using RAGSmiley = Rdmp.UI.ChecksUI.RAGSmiley;
using ViewSourceCodeDialog = Rdmp.UI.SimpleDialogs.ViewSourceCodeDialog;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs;

/// <summary>
///     Allows you to specify values for any IArgumentHost class.  This control is used by the user at 'design time' (e.g.
///     when they are building a data load configuration) and the values
///     are then populated into instantiated runtime instances (not that this control cares about how that happens).  You
///     will see a list of all properties marked with [DemandsInitialization]
///     on the argument host class.  Selecting the Argument will display the help text associated with the argument (user
///     friendly message telling them what they are supposed to put in for that
///     property) and an appropriate user control for providing a value (for example an enum will show a dropdown while a
///     string property will show a text box - See ArgumentUI).
/// </summary>
public partial class ArgumentCollectionUI : UserControl
{
    public Dictionary<IArgument, RequiredPropertyInfo> DemandDictionary;
    private Type _argumentsAreFor;
    private IArgumentHost _parent;
    private ArgumentValueUIFactory _valueUisFactory;

    private IActivateItems _activator;

    public ArgumentCollectionUI()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Reconfigures this UI (can be called multiple times throughout controls lifetime) to facilitate the population of
    ///     DemandsInitialization
    ///     properties on an underlying type (e.g. if your collection is ProcessTask and your argument type is
    ///     ProcessTaskArgument then your underlying type could
    ///     be AnySeparatorFileAttacher or MDFAttacher).  Note that while T is IArgumentHost, it also should be tied to one or
    ///     more interfaces (e.g. IAttacher) and able to host
    ///     any child of that interface of which argumentsAreForUnderlyingType is the currently configured concrete class (e.g.
    ///     AnySeparatorFileAttacher).
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="parent"></param>
    /// <param name="argumentsAreForUnderlyingType"></param>
    /// <param name="catalogueRepository"></param>
    public void Setup(IActivateItems activator, IArgumentHost parent, Type argumentsAreForUnderlyingType,
        ICatalogueRepository catalogueRepository)
    {
        _parent = parent;
        _argumentsAreFor = argumentsAreForUnderlyingType;
        _activator = activator;

        var typeLoadable = !(_argumentsAreFor == null);

        lblTypeUnloadable.Visible = !typeLoadable;

        lblClassName.Visible = typeLoadable;
        helpIcon1.Visible = typeLoadable;
        lblArgumentsTitle.Visible = typeLoadable;
        pArguments.Visible = typeLoadable;
        lblComponentNotFound.Visible = !pArguments.Visible;

        _valueUisFactory = new ArgumentValueUIFactory();

        if (_argumentsAreFor != null)
            lblClassName.Text = UsefulStuff.PascalCaseStringToHumanReadable(_argumentsAreFor.Name);

        helpIcon1.Left = lblClassName.Right;

        if (_argumentsAreFor != null)
        {
            var summary = catalogueRepository.CommentStore.GetTypeDocumentationIfExists(argumentsAreForUnderlyingType);

            if (summary != null)
                helpIcon1.SetHelpText(_argumentsAreFor.Name, summary);
            else
                helpIcon1.ClearHelpText();

            RefreshArgumentList();
        }
    }

    private void RefreshArgumentList()
    {
        DemandDictionary = ArgumentFactory.GetDemandDictionary(_parent, _argumentsAreFor);
        lblNoArguments.Visible = !DemandDictionary.Any();
        pArguments.Visible = DemandDictionary.Any();

        if (!DemandDictionary.Any())
            return;

        pArguments.Controls.Clear();
        pArguments.SuspendLayout();

        float maxArgNameWidth = 0;

        if (DemandDictionary.Any())
        {
            var g = CreateGraphics();
            maxArgNameWidth = DemandDictionary.Select(a =>
                    g.MeasureString(UsefulStuff.PascalCaseStringToHumanReadable(a.Value.Name), DefaultFont).Width)
                .Max();
        }


        foreach (var kvp in DemandDictionary)
            CreateLine(_parent, kvp.Key, kvp.Value, maxArgNameWidth);

        //headerLabel.SendToBack();
        pArguments.ResumeLayout(true);
    }


    private static Label GetLabelHeader(string caption)
    {
        var label = new Label
        {
            Text = caption,
            BackColor = Color.DarkGray,
            Dock = DockStyle.Top,

            TextAlign = ContentAlignment.MiddleCenter
        };

        return label;
    }

    private void CreateLine(IArgumentHost parent, IArgument argument, RequiredPropertyInfo required,
        float maxArgNameWidth)
    {
        var name = new Label();

        var helpIcon = new HelpIcon();
        helpIcon.SetHelpText(GetSystemTypeName(argument.GetSystemType()) ?? $"Unrecognised Type:{argument.Type}",
            required.Demand.Description);
        helpIcon.Dock = DockStyle.Right;

        var spaceSeparatedArgumentName = UsefulStuff.PascalCaseStringToHumanReadable(argument.Name);
        name.Height = helpIcon.Height;
        name.Text = spaceSeparatedArgumentName;
        name.TextAlign = ContentAlignment.MiddleLeft;
        name.Dock = DockStyle.Left;
        name.Width = (int)maxArgNameWidth + 3 /*padding*/;

        var ragSmiley = new RAGSmiley();

        if (required.Demand.Mandatory && string.IsNullOrWhiteSpace(argument.Value))
            ragSmiley.Fatal(new Exception($"Property {argument.Name} is Mandatory"));

        var args = new ArgumentValueUIArgs
        {
            Parent = parent,
            Type = argument.GetSystemType(),
            ContextText = required.Demand.ContextText
        };

        try
        {
            args.InitialValue = argument.GetValueAsSystemType();
        }
        catch (Exception e)
        {
            //add the text value value and report the error
            args.InitialValue = ArgumentValueUIFactory.CanHandleInvalidStringData(args.Type) ? argument.Value : null;

            ragSmiley.Fatal(e);
        }


        args.Required = required;
        args.CatalogueRepository = (ICatalogueRepository)argument.Repository;
        args.Setter = v =>
        {
            ragSmiley.Reset();

            try
            {
                argument.SetValue(v);
                argument.SaveToDatabase();

                argument.GetValueAsSystemType();

                if (required.Demand.Mandatory && (v == null || string.IsNullOrWhiteSpace(v.ToString())))
                    ragSmiley.Fatal(new Exception($"Property {argument.Name} is Mandatory"));
            }
            catch (Exception ex)
            {
                ragSmiley.OnCheckPerformed(new CheckEventArgs("Failed to set property properly", CheckResult.Fail, ex));
            }
        };
        args.Fatal = ragSmiley.Fatal;

        var valueui = (Control)_valueUisFactory.Create(_activator, args);
        valueui.Dock = DockStyle.Fill;

        var p = new Panel
        {
            Height = Math.Max(Math.Max(lblClassName.Height, helpIcon.Height), valueui.Height),
            Dock = DockStyle.Top
        };

        name.Location = new Point(0, 0);
        p.Controls.Add(name);

        helpIcon.Left = name.Right;
        p.Controls.Add(helpIcon);

        ragSmiley.Dock = DockStyle.Right;
        p.Controls.Add(ragSmiley);
        p.Controls.Add(valueui);

        name.Height = p.Height;

        var hr = new Label
        {
            AutoSize = false,
            BorderStyle = BorderStyle.FixedSingle,
            Height = 1,
            Dock = DockStyle.Bottom
        };
        p.Controls.Add(hr);

        valueui.BringToFront();
        pArguments.Controls.Add(p);
        p.BringToFront();
    }

    private static string GetSystemTypeName(Type type)
    {
        return typeof(Enum).IsAssignableFrom(type) ? "Enum" : type?.Name;
    }

    private void btnViewSourceCode_Click(object sender, EventArgs e)
    {
        new ViewSourceCodeDialog($"{_argumentsAreFor.Name}.cs").Show();
    }
}