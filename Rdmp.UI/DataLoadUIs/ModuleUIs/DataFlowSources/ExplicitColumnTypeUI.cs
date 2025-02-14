// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;

namespace Rdmp.UI.DataLoadUIs.ModuleUIs.DataFlowSources;

/// <summary>
/// Allows you to specify an explicit C# datatype for an RDMP data flow component to use for a given named column.  For example if you are trying to load a CSV file with values
/// like "291","195" but they know that some codes have leading zeros "012" and wish to preserve this leading 0s so they can explicitly define the column as being a string.
/// </summary>
public partial class ExplicitColumnTypeUI : UserControl
{
    public string ColumnName => textBox1.Text;

    public Type Type => (Type)ddType.SelectedItem;

    public ExplicitColumnTypeUI(string name, Type t)
    {
        InitializeComponent();

        ddType.Items.AddRange(typeof(string), typeof(double), typeof(bool), typeof(DateTime));

        textBox1.Text = name;
        ddType.SelectedItem = t;
    }

    public event EventHandler DeletePressed;

    private void btnDelete_Click(object sender, EventArgs e)
    {
        var h = DeletePressed;
        if (h != null)
            DeletePressed(this, EventArgs.Empty);
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
    }
}