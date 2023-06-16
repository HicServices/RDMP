// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;



namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of Array types TableInfo[], Catalogue[] etc</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueDictionaryUI : UserControl, IArgumentValueUI
{
    private Type _kType;
    private Type _vType;
    private IDictionary _dictionary;
    private ArgumentValueUIFactory _factory;

    private ArgumentValueUIArgs _args;
    private IActivateItems _activator;

    public ArgumentValueDictionaryUI()
    {
        InitializeComponent();

        _factory = new ArgumentValueUIFactory();
    }

    public void SetUp(IActivateItems activator,ArgumentValueUIArgs args)
    {
        _activator = activator;
        _args = args;
        var concreteType = args.Type;

        //get an IDictionary either from the object or a new empty one (e.g. if Value is null)
        _dictionary = (IDictionary)(args.InitialValue??Activator.CreateInstance(concreteType));

        _kType = concreteType.GenericTypeArguments[0];
        _vType = concreteType.GenericTypeArguments[1];

        foreach (DictionaryEntry kvp in _dictionary)
            AddRow(kvp.Key,kvp.Value);

        btnSave.Enabled = false;
    }

    private List<object> keys = new();
    private List<object> values = new();

    private Stack<Tuple<Control,Control>> controls = new();
        

    private void btnAdd_Click(object sender, EventArgs e)
    {
        AddRow(null, null);
    }

    private void AddRow(object key, object val)
    {
        const int uiWidth = 350;
        var element = keys.Count;
        var y = element * 25;
        keys.Add(key);
        values.Add(val);

        var keyArgs = _args.Clone();

        keyArgs.Setter = k =>
        {
            keys[element] = k;
            btnSave.Enabled = true;
        };

        keyArgs.InitialValue = key;
        keyArgs.Type = _kType;

        var keyUI = (Control)_factory.Create(_activator, keyArgs);
        keyUI.Dock = DockStyle.None;
        keyUI.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        keyUI.Location = new Point(0, y);
        keyUI.Width = uiWidth;
        panel1.Controls.Add(keyUI);

        var valueArgs = _args.Clone();

        valueArgs.Setter = v =>
        {
            values[element] = v;
            btnSave.Enabled = true;
        };

        valueArgs.InitialValue = val;
        valueArgs.Type = _vType;

        var valueUI = (Control)_factory.Create(_activator, valueArgs);
        valueUI.Dock = DockStyle.None;
        valueUI.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        valueUI.Location = new Point(keyUI.Right, y);
        valueUI.Width = uiWidth;

        panel1.Controls.Add(valueUI);
        //they added a row so it's saveable
        btnSave.Enabled = true;
        btnRemove.Enabled = true;

        controls.Push(Tuple.Create(keyUI, valueUI));
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (keys.Any(k => k == null) || keys.Count != keys.Distinct().Count())
        {
            MessageBox.Show("You cannot have null or duplicate values for keys");
            return;
        }

        try
        {
            _dictionary.Clear();
            for (var i = 0; i < keys.Count; i++)
                _dictionary.Add(keys[i], values[i]);

            _args.Setter(_dictionary);
            btnSave.Enabled = false;
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
            btnSave.Enabled = true;
        }
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
        var popped = controls.Pop();
        panel1.Controls.Remove(popped.Item1);
        panel1.Controls.Remove(popped.Item2);

        keys.RemoveAt(keys.Count - 1);
        values.RemoveAt(values.Count - 1);

        btnRemove.Enabled = keys.Any();
        btnSave.Enabled = true;
    }

}