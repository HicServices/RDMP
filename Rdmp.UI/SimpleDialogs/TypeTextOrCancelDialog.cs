// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Prompts the user to type in some text.  There will be a title text telling you what the system expects you to type (e.g. some DQE annotation text).
/// </summary>
[TechnicalUI]
public partial class TypeTextOrCancelDialog : Form
{
    private readonly bool _allowBlankText;
    private readonly bool _multiline;
    private Scintilla _scintilla;

    public string ResultText => (_multiline ? _scintilla.Text : textBox1.Text)?.Trim();

    /// <summary>
    /// True to require that text typed be sane for usage as a column name, table name etc e.g. "bob" but not "bob::bbbbb".
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RequireSaneHeaderText { get; set; }

    //"Column Name","Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);

    public TypeTextOrCancelDialog(string title, string prompt, int maxCharacters, string startingTextForInputBox = null,
        bool allowBlankText = false, bool multiLine = false)
        : this(new DialogArgs
        {
            WindowTitle = title,
            EntryLabel = prompt
        }, maxCharacters, startingTextForInputBox, allowBlankText, multiLine)
    {
    }

    public TypeTextOrCancelDialog(DialogArgs args, int maxCharacters, string startingTextForInputBox = null,
        bool allowBlankText = false, bool multiLine = false)
    {
        _allowBlankText = allowBlankText;
        _multiline = multiLine;

        InitializeComponent();

        var header = args.WindowTitle;


        if (header is { Length: > WideMessageBox.MAX_LENGTH_TITLE })
            header = header[..WideMessageBox.MAX_LENGTH_TITLE];

        taskDescriptionLabel1.SetupFor(args);

        Text = header;
        textBox1.MaxLength = maxCharacters;

        if (_multiline)
        {
            var editor = new ScintillaTextEditorFactory();
            _scintilla = editor.Create(null, SyntaxLanguage.None, null, true, false);
            _scintilla.Dock = DockStyle.Fill;
            _scintilla.TextChanged += _scintilla_TextChanged;
            _scintilla.KeyDown += _scintilla_KeyDown;
            _scintilla.Text = startingTextForInputBox;
            _scintilla.WrapMode = WrapMode.Word;

            pTextEditor.Controls.Remove(textBox1);
            pTextEditor.Controls.Add(_scintilla);

            //Move cursor to the end of the textbox
            ActiveControl = _scintilla;
            _scintilla.SelectionStart = _scintilla.TextLength;

            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.ScrollBars = ScrollBars.Vertical;
            Width = 740;

            //Update the tooltip for the OK button
            toolTip.SetToolTip(btnOk, "Press to Save (SHIFT + ENTER)");
        }
        else
        {
            textBox1.Text = startingTextForInputBox;
            Width = Math.Max(540, Math.Min(740, taskDescriptionLabel1.PreferredWidth));

            ActiveControl = textBox1;
        }

        SetEnabledness();
    }

    private void _scintilla_KeyDown(object sender, KeyEventArgs e)
    {
        FinishedKeyCheck(e);
    }

    private void _scintilla_TextChanged(object sender, EventArgs e)
    {
        SetEnabledness();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void textBox1_KeyDown(object sender, KeyEventArgs e)
    {
        FinishedKeyCheck(e);
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        SetEnabledness();
    }

    private void SetEnabledness()
    {
        textBox1.ForeColor = Color.Black;

        //if there's some text typed and we want typed text to be sane
        if (RequireSaneHeaderText && !string.IsNullOrWhiteSpace(textBox1.Text))
            //if the sane name doesn't match the
            if (!textBox1.Text.Equals(QuerySyntaxHelper.MakeHeaderNameSensible(textBox1.Text),
                    StringComparison.CurrentCultureIgnoreCase))
            {
                btnOk.Enabled = false;
                textBox1.ForeColor = Color.Red;
                return;
            }

        btnOk.Enabled = !string.IsNullOrWhiteSpace(ResultText) || _allowBlankText;
    }

    private void FinishedKeyCheck(KeyEventArgs e)
    {
        //If they've pressed enter...
        if (e.KeyCode == Keys.Enter)
            //If the OK button is enabled AND... (we're not multiline OR we are multiline but they're holding shift)
            if (btnOk.Enabled && (!_multiline || (_multiline && e.Shift)))
            {
                //Suppress the enter key (so a new line isn't created) and press the OK button
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnOk_Click(null, null);
            }

        //Escape should work for all controls
        if (e.KeyCode == Keys.Escape)
            btnCancel_Click(null, null);
    }

    private void TypeTextOrCancelDialog_Resize(object sender, EventArgs e)
    {
        // Set the height by taking the designer height and adding on the height that the task description label wants to be
        if (_multiline)
            Height = taskDescriptionLabel1.PreferredHeight + 220;
        else
            Height = taskDescriptionLabel1.PreferredHeight + 100;
    }
}