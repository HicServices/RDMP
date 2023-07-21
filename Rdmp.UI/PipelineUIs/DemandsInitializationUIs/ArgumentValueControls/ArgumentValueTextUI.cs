// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
///     Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization]
///     decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
///     <para>
///         This Control is for setting Properties that can be represented as textual strings (this includes parsed types
///         like int, Regex etc).
///     </para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueTextUI : UserControl, IArgumentValueUI
{
    private ArgumentValueUIArgs _args;
    private bool _bLoading = true;
    private readonly bool _isPassword;

    public ArgumentValueTextUI(bool isPassword)
    {
        _isPassword = isPassword;
        InitializeComponent();
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _bLoading = true;
        _args = args;

        tbText.Text = args.InitialValue == null ? "" : args.InitialValue.ToString();

        if (args.Type == typeof(DirectoryInfo))
        {
            tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbText.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
        }

        if (args.Type == typeof(CultureInfo))
        {
            tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbText.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var collection = new AutoCompleteStringCollection();
            collection.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.Name).ToArray());

            tbText.AutoCompleteCustomSource = collection;
        }

        if (args.Type == typeof(FileInfo))
        {
            tbText.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbText.AutoCompleteSource = AutoCompleteSource.FileSystem;
        }

        if (_isPassword)
        {
            tbText.UseSystemPasswordChar = true;
            var val = args.InitialValue;
            tbText.Text = val != null ? ((IEncryptedString)val).GetDecryptedValue() : "";
        }

        _bLoading = false;
    }

    private void tbText_TextChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _args.Setter(tbText.Text);
    }
}