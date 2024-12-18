// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace Rdmp.UI;

/// <summary>
/// ComboBox with support for autocomplete (based on substring)
/// </summary>
public class SuggestComboBox : ComboBox
{
    #region fields and properties

    private readonly ListBox _suggLb = new() { Visible = false, TabStop = false };
    private readonly BindingList<string> _suggBindingList = new();
    private Expression<Func<ObjectCollection, IEnumerable<string>>> _propertySelector;
    private Func<ObjectCollection, IEnumerable<string>> _propertySelectorCompiled;
    private Expression<Func<string, string, bool>> _filterRule;
    private Func<string, bool> _filterRuleCompiled;
    private Expression<Func<string, string>> _suggestListOrderRule;
    private Func<string, string> _suggestListOrderRuleCompiled;


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SuggestBoxHeight
    {
        get => _suggLb.Height;
        set
        {
            if (value > 0) _suggLb.Height = value;
        }
    }

    /// <summary>
    /// If the item-type of the ComboBox is not string,
    /// you can set here which property should be used
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Expression<Func<ObjectCollection, IEnumerable<string>>> PropertySelector
    {
        get => _propertySelector;
        set
        {
            if (value == null) return;
            _propertySelector = value;
            _propertySelectorCompiled = value.Compile();
        }
    }

    ///<summary>
    /// Lambda-Expression to determine the suggested items
    /// (as Expression here because simple lamda (func) is not serializable)
    /// <para>default: case-insensitive contains search</para>
    /// <para>1st string: list item</para>
    /// <para>2nd string: typed text</para>
    ///</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Expression<Func<string, string, bool>> FilterRule
    {
        get => _filterRule;
        set
        {
            if (value == null) return;

            _filterRule = value;
            _filterRuleCompiled = item => value.Compile()(item, Text);
        }
    }

    ///<summary>
    /// Lambda-Expression to order the suggested items
    /// (as Expression here because simple lamda (func) is not serializable)
    /// <para>default: alphabetic ordering</para>
    ///</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Expression<Func<string, string>> SuggestListOrderRule
    {
        get => _suggestListOrderRule;
        set
        {
            if (value == null) return;

            _suggestListOrderRule = value;
            _suggestListOrderRuleCompiled = value.Compile();
        }
    }

    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    public SuggestComboBox()
    {
        // set the standard rules:
        _filterRuleCompiled = DefaultFilterRule;
        _suggestListOrderRuleCompiled = s => s;
        _propertySelectorCompiled = collection => collection.Cast<string>();

        _suggLb.DataSource = _suggBindingList;
        _suggLb.Click += SuggLbOnClick;

        ParentChanged += OnParentChanged;

        _suggLb.VisibleChanged += _suggLb_VisibleChanged;
    }

    private bool DefaultFilterRule(string arg)
    {
        if (string.IsNullOrWhiteSpace(Text))
            return false;

        var keywords = Text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        return keywords.All(k => arg.ToLower().Contains(k.ToLower()));
    }

    private bool _changingVisibility;

    private void _suggLb_VisibleChanged(object sender, EventArgs e)
    {
        //don't fire event if its already being fired (needed because Controls.Remove will make visible false)
        if (_changingVisibility)
            return;

        _changingVisibility = true;

        try
        {
            var form = _suggLb.FindForm();

            if (form != null)
            {
                if (_suggLb.Parent != form)
                {
                    //move it to the parent form
                    _suggLb.Parent.Controls.Remove(_suggLb);
                    form.Controls.Add(_suggLb);
                }

                if (_suggLb.Visible)
                {
                    //but translate the coordinates to make sure it is in the same place on the screen
                    var locScreen = PointToScreen(Point.Empty);
                    locScreen.Y += 20;

                    var locOnForm = form.PointToClient(locScreen);
                    _suggLb.Location = locOnForm;

                    _suggLb.BringToFront();
                }
            }
        }
        finally
        {
            _changingVisibility = false;
        }
    }

    /// <summary>
    /// the magic happens here ;-)
    /// </summary>
    /// <param name="e"></param>
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        if (!Focused) return;

        _suggBindingList.Clear();
        _suggBindingList.RaiseListChangedEvents = false;
        _propertySelectorCompiled(Items)
            .Where(_filterRuleCompiled)
            .OrderBy(_suggestListOrderRuleCompiled)
            .ToList()
            .ForEach(_suggBindingList.Add);
        _suggBindingList.RaiseListChangedEvents = true;
        _suggBindingList.ResetBindings();

        _suggLb.Visible = _suggBindingList.Any();

        if (_suggBindingList.Count == 1 &&
            _suggBindingList.Single().Length == Text.Trim().Length)
        {
            Text = _suggBindingList.Single();
            Select(0, Text.Length);
            _suggLb.Visible = false;
        }
    }

    #region size and position of suggest box

    /// <summary>
    /// suggest-ListBox is added to parent control
    /// (in ctor parent isn't already assigned)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnParentChanged(object sender, EventArgs e)
    {
        Parent.Controls.Add(_suggLb);
        Parent.Controls.SetChildIndex(_suggLb, 0);
        _suggLb.Top = Top + Height - 3;
        _suggLb.Left = Left + 3;
        _suggLb.Width = Width - 20;
        _suggLb.Font = new Font("Segoe UI", 9);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        _suggLb.Top = Top + Height - 3;
        _suggLb.Left = Left + 3;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        _suggLb.Width = Width - 20;
    }

    #endregion

    #region visibility of suggest box

    protected override void OnLostFocus(EventArgs e)
    {
        // _suggLb can only getting focused by clicking (because TabStop is off)
        // --> click-eventhandler 'SuggLbOnClick' is called
        if (!_suggLb.Focused)
            HideSuggBox();
        base.OnLostFocus(e);
    }

    private void SuggLbOnClick(object sender, EventArgs eventArgs)
    {
        Text = _suggLb.Text;
        Focus();
    }

    private void HideSuggBox()
    {
        _suggLb.Visible = false;
    }

    protected override void OnDropDown(EventArgs e)
    {
        HideSuggBox();
        base.OnDropDown(e);
    }

    #endregion

    #region keystroke events

    /// <summary>
    /// if the suggest-ListBox is visible some keystrokes
    /// should behave in a custom way
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
    {
        if (!_suggLb.Visible)
        {
            base.OnPreviewKeyDown(e);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Down:
                if (_suggLb.SelectedIndex < _suggBindingList.Count - 1)
                    _suggLb.SelectedIndex++;
                return;
            case Keys.Up:
                if (_suggLb.SelectedIndex > 0)
                    _suggLb.SelectedIndex--;
                return;
            case Keys.Enter:
                Text = _suggLb.Text;
                Select(0, Text.Length);
                _suggLb.Visible = false;
                return;
            case Keys.Escape:
                HideSuggBox();
                return;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyData is Keys.Enter or Keys.Escape)
        {
            e.SuppressKeyPress = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private static readonly Keys[] KeysToHandle = { Keys.Down, Keys.Up, Keys.Enter, Keys.Escape };

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) =>
        // the keystrokes of interest should not be processed by base class:
        (_suggLb.Visible && KeysToHandle.Contains(keyData)) || base.ProcessCmdKey(ref msg, keyData);

    #endregion
}