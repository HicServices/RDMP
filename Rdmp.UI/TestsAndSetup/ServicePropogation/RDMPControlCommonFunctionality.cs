// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using ScintillaNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;
using HelpIcon = Rdmp.UI.SimpleControls.HelpIcon;
using Point = System.Drawing.Point;

namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

public class RDMPControlCommonFunctionality
{
    /// <summary>
    /// This is the strip of buttons and labels for all controls commonly used for interacting with the content of the tab.  The
    /// bar should start with <see cref="_menuDropDown"/>.
    /// </summary>
    public ToolStrip ToolStrip { get; private set; }

    /// <summary>
    /// Occurs before checking the <see cref="ICheckable"/> (see  <see cref="StartChecking"/>
    /// </summary>
    public event EventHandler BeforeChecking;

    /// <summary>
    /// Occurs when a call to <see cref="Fatal"/> is made.  This will result in the form showing an error
    /// icon (but not closing itself).
    /// </summary>
    public event EventHandler<CheckEventArgs> OnFatal;

    /// <summary>
    /// Event occurs when the <see cref="ToolStrip"/> is added to the hosting Control
    /// </summary>
    public event EventHandler ToolStripAddedToHost;

    private readonly IRDMPControl _hostControl;

    /// <summary>
    /// This is the button with 3 horizontal lines which exposes all menu options which are seldom pressed or navigate you somewhere
    /// </summary>
    private readonly ToolStripMenuItem _menuDropDown;

    private AtomicCommandUIFactory atomicCommandUIFactory;

    private readonly RAGSmileyToolStrip _ragSmileyToolStrip;

    private readonly ToolStripMenuItem _refresh;

    private readonly ToolStripButton _runChecksToolStripButton =
        new("Run Checks", FamFamFamIcons.arrow_refresh.ImageToBitmap());

    private ICheckable _checkable;
    private IActivateItems _activator;

    private Dictionary<string, ToolStripDropDownButton> _dropDownButtons = new();
    private Dictionary<string, ToolStripMenuItem> _addToMenuSubmenus = new();


    public RDMPControlCommonFunctionality(IRDMPControl hostControl)
    {
        _hostControl = hostControl;
        ToolStrip = new ToolStrip
        {
            Location = new Point(0, 0),
            TabIndex = 1
        };

        //Add the three lines dropdown for seldom used options (See AddToMenu). This starts disabled.
        _menuDropDown = new ToolStripMenuItem
        {
            Image = CatalogueIcons.Menu.ImageToBitmap(),
            Visible = false
        };
        ToolStrip.Items.Add(_menuDropDown);

        _ragSmileyToolStrip = new RAGSmileyToolStrip();
        ToolStrip.Items.Add(_ragSmileyToolStrip);

        _runChecksToolStripButton.Click += (s, e) => StartChecking();
        ToolStrip.Items.Add(_runChecksToolStripButton);

        _ragSmileyToolStrip.Enabled = false;
        _ragSmileyToolStrip.Visible = false;
        _runChecksToolStripButton.Enabled = false;
        _runChecksToolStripButton.Visible = false;

        //todo this isn't appearing in all panels
        _refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right
        };
        _refresh.Click += Refresh;
        //ToolStrip.Items.Add(_refresh);
    }

    private void Refresh(object sender, EventArgs e)
    {
        //var cmd = new ExecuteCommandRefreshObject(_activator, DatabaseObject);
        //cmd.Execute();
        Console.WriteLine("yearp!");
        //todo figure this one out
        //_hostControl.RefreshBus_RefreshObject(sender, e);
    }

    public void SetItemActivator(IActivateItems activator)
    {
        _activator = activator;
        atomicCommandUIFactory = new AtomicCommandUIFactory(_activator);
        _activator.Theme.ApplyTo(ToolStrip);
    }


    /// <summary>
    /// Adds the given <paramref name="cmd"/> to the top bar at the top of the control.  This will be always
    /// visible at the top of the form
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="overrideCommandName"></param>
    /// <param name="overrideImage"></param>
    /// <param name="underMenu">If the command should appear under a submenu dropdown then this should be the name of that root button</param>
    public void Add(IAtomicCommand cmd, string overrideCommandName = null, Image<Rgba32> overrideImage = null,
        string underMenu = null)
    {
        var p = _hostControl.GetTopmostRDMPUserControl();
        if (p != _hostControl)
        {
            p.CommonFunctionality.Add(cmd, overrideCommandName, overrideImage);
            return;
        }

        InitializeToolStrip();

        var button = string.IsNullOrWhiteSpace(underMenu)
            ? atomicCommandUIFactory.CreateToolStripItem(cmd)
            : atomicCommandUIFactory.CreateMenuItem(cmd);
        if (!string.IsNullOrWhiteSpace(overrideCommandName))
            button.Text = overrideCommandName;

        if (overrideImage != null)
            button.Image = overrideImage.ImageToBitmap();

        Add(button, underMenu);
    }

    /// <summary>
    /// Adds the given <paramref name="item"/> to the top bar at the top of the control.  This will be always
    /// visible at the top of the form
    /// </summary>
    /// <param name="item"></param>
    /// <param name="underMenu">If the command should appear under a submenu dropdown then this should be the name of that root button</param>
    public void Add(ToolStripItem item, string underMenu = null)
    {
        var p = _hostControl.GetTopmostRDMPUserControl();
        if (p != _hostControl)
        {
            p.CommonFunctionality.Add(item);
            return;
        }

        InitializeToolStrip();

        if (!string.IsNullOrWhiteSpace(underMenu))
        {
            if (!_dropDownButtons.ContainsKey(underMenu))
                _dropDownButtons.Add(underMenu,
                    new ToolStripDropDownButton { Text = underMenu, DisplayStyle = ToolStripItemDisplayStyle.Text });

            _dropDownButtons[underMenu].DropDownItems.Add(item);
            ToolStrip.Items.Add(_dropDownButtons[underMenu]);
        }
        else
        {
            ToolStrip.Items.Add(item);
        }
    }

    /// <summary>
    /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target <paramref name="checkable"/>.
    /// </summary>
    /// <param name="checkable"></param>
    public void AddChecks(ICheckable checkable)
    {
        _ragSmileyToolStrip.Enabled = false;
        _runChecksToolStripButton.Enabled = true;
        _ragSmileyToolStrip.Visible = true;
        _runChecksToolStripButton.Visible = true;
        _checkable = checkable;
    }

    /// <summary>
    /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target the return value of <paramref name="checkableFunc"/>.  If the method throws the
    /// Exception will be exposed in the checking system.
    /// 
    /// <para>Only use this method if there is a reasonable chance the <paramref name="checkableFunc"/> will crash otherwise use the normal overload</para>
    /// </summary>
    /// <param name="checkableFunc"></param>
    public void AddChecks(Func<ICheckable> checkableFunc)
    {
        _runChecksToolStripButton.Enabled = true;

        try
        {
            _checkable = checkableFunc();
        }
        catch (Exception e)
        {
            //covers the case where you have previously passed checks then fail but _checks points to the old passing one
            _checkable = null;
            _ragSmileyToolStrip.Fatal(e);
        }
    }

    /// <summary>
    /// Runs checks on the last variable passed in <see cref="AddChecks(ICheckable)"/>.  Do not call this method unless you have first
    /// called <see cref="AddChecks(ICheckable)"/>.
    /// </summary>
    public void StartChecking()
    {
        if (_checkable == null)
            return;

        if (BeforeChecking != null)
        {
            _ragSmileyToolStrip.Enabled = true;

            var e = new BeforeCheckingEventArgs(_ragSmileyToolStrip, _checkable);
            BeforeChecking(this, e);

            if (e.Cancel)
                return;
        }

        _ragSmileyToolStrip.StartChecking(_checkable);
    }

    /// <summary>
    /// Reports the supplied exception in the RAG checks smiley on the top toolbar.  This will result in rag checks becomming
    /// visible if it was not visible before.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="exception"></param>
    public void Fatal(string s, Exception exception)
    {
        var args = new CheckEventArgs(s, CheckResult.Fail, exception);

        OnFatal?.Invoke(this, args);

        _ragSmileyToolStrip.OnCheckPerformed(args);
    }

    /// <summary>
    /// All keywords added via <see cref="AddHelp"/>
    /// </summary>
    private readonly HashSet<Control> _helpAdded = new();

    /// <summary>
    /// Adds a <see cref="HelpIcon"/> to the task bar at the top of the control
    /// </summary>
    /// <param name="title"></param>
    /// <param name="body"></param>
    public void AddHelpStringToToolStrip(string title, string body)
    {
        var help = new HelpIcon();
        help.SetHelpText(title, body);
        Add(new ToolStripControlHost(help));
    }


    /// <summary>
    /// Adds a <see cref="HelpIcon"/> on the right of the control with documentation for the listed property
    /// </summary>
    /// <param name="c">The control you want the help to appear beside</param>
    /// <param name="propertyName">The xml-doc property you want e.g. "ICatalogue.Name"</param>
    /// <param name="title"></param>
    /// <param name="anchor">Explicit anchor style to apply to help icon.  If you pass None (default) then anchor will
    ///  be chosen based on the control <paramref name="c"/></param>
    public void AddHelp(Control c, string propertyName, string title = null, AnchorStyles anchor = AnchorStyles.None)
    {
        if (_activator == null)
            throw new Exception(
                "Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

        if (c.Parent == null)
            throw new NotSupportedException(
                "Control is not in a container.  HelpIcon cannot be added to top level controls");

        title ??= propertyName;
        var body = _activator.CommentStore.GetDocumentationIfExists(propertyName, false, true);

        if (body == null)
            return;

        AddHelpString(c, title, body, anchor);
    }

    /// <summary>
    /// Adds a <see cref="HelpIcon"/> on the right of the control with the provided help text
    /// </summary>
    /// <param name="c">The control you want the help to appear beside</param>
    /// <param name="title">The textual header you want shown</param>
    /// <param name="body">The text you want displayed on hover (under the title)</param>
    /// <param name="anchor">Explicit anchor style to apply to help icon.  If you pass None (default) then anchor will
    ///  be chosen based on the control <paramref name="c"/></param>
    public void AddHelpString(Control c, string title, string body, AnchorStyles anchor = AnchorStyles.None)
    {
        //don't add help to the control more than once
        if (_helpAdded.Add(c))
            return;

        var help = new HelpIcon();
        help.SetHelpText(title, body);

        help.Location = new Point(c.Right + 3, c.Top);

        if (anchor == AnchorStyles.None)
        {
            if (c.Anchor.HasFlag(AnchorStyles.Right) && c.Anchor.HasFlag(AnchorStyles.Top))
                help.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            else if (c.Anchor.HasFlag(AnchorStyles.Right) && c.Anchor.HasFlag(AnchorStyles.Bottom))
                help.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        }
        else
        {
            help.Anchor = anchor;
        }

        c.Parent?.Controls.Add(help);
    }

    public void ClearToolStrip()
    {
        var p = _hostControl.GetTopmostRDMPUserControl();

        //never clear the toolbar if you are not the topmost RDMPUserControl otherwise the parent will setup some menu items and you will clear it
        if (p != _hostControl)
            return;

        ToolStrip.Items.Clear();

        _menuDropDown.DropDownItems.Clear();
        _menuDropDown.Visible = false;

        _addToMenuSubmenus.Clear();

        ToolStrip.Items.Add(_menuDropDown);
        ToolStrip.Items.Add(_ragSmileyToolStrip);
        ToolStrip.Items.Add(_runChecksToolStripButton);

        _dropDownButtons.Clear();
    }

    /// <summary>
    /// Adds the given <paramref name="cmd"/> to the drop down menu button bar at the top of the control.  This
    /// will be visible only when you click on the menu button.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="overrideCommandName"></param>
    /// <param name="overrideImage"></param>
    /// <param name="underMenu"></param>
    public void AddToMenu(IAtomicCommand cmd, string overrideCommandName = null, Image<Rgba32> overrideImage = null,
        string underMenu = null)
    {
        var p = _hostControl.GetTopmostRDMPUserControl();
        if (p != _hostControl)
        {
            p.CommonFunctionality.AddToMenu(cmd, overrideCommandName, overrideImage);
            return;
        }

        InitializeToolStrip();

        var menuItem = atomicCommandUIFactory.CreateMenuItem(cmd);
        if (!string.IsNullOrWhiteSpace(overrideCommandName))
            menuItem.Text = overrideCommandName;

        if (overrideImage != null)
            menuItem.Image = overrideImage.ImageToBitmap();

        AddToMenu(menuItem, underMenu);
    }

    /// <summary>
    /// Adds the given <paramref name="menuItem"/> to the drop down menu button bar at the top of the control.  This
    /// will be visible only when you click on the menu button.
    /// </summary>
    public void AddToMenu(ToolStripItem menuItem, string underMenu = null)
    {
        var p = _hostControl.GetTopmostRDMPUserControl();
        if (p != _hostControl)
        {
            p.CommonFunctionality.AddToMenu(menuItem);
            return;
        }

        InitializeToolStrip();

        if (!string.IsNullOrWhiteSpace(underMenu))
        {
            if (!_addToMenuSubmenus.TryGetValue(underMenu, out var stripMenuItem))
            {
                stripMenuItem = new ToolStripMenuItem(underMenu);
                _addToMenuSubmenus.Add(underMenu, stripMenuItem);

                // If it's the GoTo menu then when the user expands the menu we have to fetch the objects
                // and update the IsImpossible status etc.
                if (underMenu == AtomicCommandFactory.GoTo)
                    RDMPContextMenuStrip.RegisterFetchGoToObjecstCallback(_addToMenuSubmenus[underMenu]);
            }

            stripMenuItem.DropDownItems.Add(menuItem);
            _menuDropDown.DropDownItems.Add(stripMenuItem);
        }
        else
        {
            _menuDropDown.DropDownItems.Add(menuItem);
        }

        _menuDropDown.Visible = true;
    }


    /// <summary>
    /// Adds a new ToolStripLabel with the supplied <paramref name="label"/> text to the menu bar at the top of the control
    /// </summary>
    /// <param name="label"></param>
    /// <param name="showIcon">True to add the text icon next to the text</param>
    public void Add(string label, bool showIcon = true)
    {
        Add(new ToolStripLabel(label, showIcon ? FamFamFamIcons.text_align_left.ImageToBitmap() : null));
    }

    /// <summary>
    /// Adds the given <paramref name="cmd"/> to the menu bar at the top of the control
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="overrideCommandName"></param>
    /// <param name="overrideImage"></param>
    /// <param name="overlayKind"></param>
    protected void Add(IAtomicCommand cmd, string overrideCommandName, RDMPConcept overrideImage,
        OverlayKind overlayKind = OverlayKind.None)
    {
        Add(cmd, overrideCommandName, _activator.CoreIconProvider.GetImage(overrideImage, overlayKind));
    }

    protected virtual void InitializeToolStrip()
    {
        if (_activator == null)
            throw new Exception(
                "Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

        ((Control)_hostControl).Controls.Add(ToolStrip);

        ToolStripAddedToHost?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets the RAG checker (smiley) if there is one to empty
    /// </summary>
    public void ResetChecks()
    {
        _ragSmileyToolStrip.Reset();
        _ragSmileyToolStrip.Enabled = false;
    }

    /// <summary>
    /// Performs the given <paramref name="action"/>.  If an Exception is thrown then
    /// the <paramref name="tb"/> will be turned Red (otherwise it will be set to Black).
    /// </summary>
    /// <param name="tb"></param>
    /// <param name="action"></param>
    public static void DoActionAndRedIfThrows(TextBox tb, Action action)
    {
        tb.ForeColor = Color.Black;
        try
        {
            action();
        }
        catch (Exception)
        {
            tb.ForeColor = Color.Red;
        }
    }

    private Dictionary<Scintilla, Color> _oldColours = new();

    /// <summary>
    /// Sets the text color in the <paramref name="queryEditor"/> to red (or back to normal if <paramref name="red"/> is false).
    /// </summary>
    /// <param name="queryEditor"></param>
    /// <param name="red"></param>
    public void ScintillaGoRed(Scintilla queryEditor, bool red)
    {
        _oldColours.TryAdd(queryEditor, queryEditor.Styles[1].ForeColor);

        if (red)
        {
            queryEditor.Styles[1].ForeColor = Color.Red;

            queryEditor.StartStyling(0);
            queryEditor.SetStyling(queryEditor.TextLength, 1);
        }
        else
        {
            queryEditor.Styles[1].ForeColor = _oldColours[queryEditor];
            //resets styling
            queryEditor.Text = queryEditor.Text;
        }
    }

    public void ScintillaGoRed(Scintilla queryEditor, Exception exception)
    {
        queryEditor.ReadOnly = false;

        queryEditor.Text =
            $@"{ExceptionHelper.ExceptionToListOfInnerMessages(exception)}

Technical Detail:
Type:{exception.GetType()}
Stack Trace:{exception.StackTrace}";

        queryEditor.ReadOnly = true;

        //go red after you have set the text
        ScintillaGoRed(queryEditor, true);
    }

    /// <summary>
    /// Disables mouse wheel scrolling on the given <paramref name="cb"/>
    /// </summary>
    /// <param name="cb"></param>
    public static void DisableMouseWheel(ComboBox cb)
    {
        cb.MouseWheel += (s, e) => ((HandledMouseEventArgs)e).Handled = !((ComboBox)s).DroppedDown;
    }
}