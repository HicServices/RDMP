// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// Prompts the user to type in some text.  There will be a title text telling you what the system expects you to type (e.g. some DQE annotation text).
    /// </summary>
    [TechnicalUI]
    public partial class TypeTextOrCancelDialog : Form
    {
        private readonly bool _allowBlankText;
        private readonly bool _multiline;
        private Scintilla _scintilla;

        public string ResultText {get { return (_multiline ? _scintilla.Text : textBox1.Text)?.Trim(); }}

        /// <summary>
        /// True to require that text typed be sane for usage as a column name, table name etc e.g. "bob" but not "bob::bbbbb".
        /// </summary>
        public bool RequireSaneHeaderText{get;set; }

        //"Column Name","Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);

        public TypeTextOrCancelDialog(string title, string prompt, int maxCharacters, string startingTextForInputBox = null, bool allowBlankText = false, bool multiLine = false)
            : this(new DialogArgs
            {
                WindowTitle = title,
                EntryLabel = prompt,
            },maxCharacters,startingTextForInputBox,allowBlankText,multiLine)
        {
            
        }

        public TypeTextOrCancelDialog(DialogArgs args, int maxCharacters, string startingTextForInputBox = null, bool allowBlankText = false, bool multiLine = false)
        {
            _allowBlankText = allowBlankText;
            _multiline = multiLine;

            InitializeComponent();

            var header = args.WindowTitle;
           

            if (header != null && header.Length > WideMessageBox.MAX_LENGTH_TITLE)
                header = header.Substring(0, WideMessageBox.MAX_LENGTH_TITLE);

            taskDescriptionLabel1.SetupFor(args);

            this.Text = header;
            this.textBox1.MaxLength = maxCharacters;

            if (_multiline)
            {
                var editor = new ScintillaTextEditorFactory();
                _scintilla = editor.Create(null,null,null,true,false);
                _scintilla.Dock = DockStyle.Fill;
                _scintilla.TextChanged += _scintilla_TextChanged;
                _scintilla.Text = startingTextForInputBox;
                _scintilla.WrapMode = WrapMode.Word;

                pTextEditor.Controls.Remove(textBox1);
                pTextEditor.Controls.Add(_scintilla);

                this.textBox1.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
                this.textBox1.ScrollBars = ScrollBars.Vertical;
                this.Height = 290;
                this.Width = 740;
            }
            else
            {
                textBox1.Text = startingTextForInputBox;
                Width = Math.Max(540, Math.Min(740, taskDescriptionLabel1.PreferredWidth));
            }

            SetEnabledness();          
        }

        private void _scintilla_TextChanged(object sender, EventArgs e)
        {
            SetEnabledness();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //If they've pressed enter...
            if (e.KeyCode == Keys.Enter)
            {
                if(!(_multiline && e.Control))
                {
                    //Supress the enter key (so a new line isn't created) and press the OK button (if it's enabled) if we're NOT (in a multiline control while holding the control key)
                    //i.e. in a multiline control hold CTRL+Enter to create a new line, else we treat enter as normal and "OK" the window
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    if (btnOk.Enabled)
                        btnOk_Click(null, null);
                }
            }

            //Escape should work for all controls
            if (e.KeyCode == Keys.Escape)
                btnCancel_Click(null, null);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SetEnabledness();
        }

        private void SetEnabledness()
        {
            textBox1.ForeColor = Color.Black;

            //if there's some text typed and we want typed text to be sane
            if(RequireSaneHeaderText && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                //if the sane name doesn't match the 
                if(!textBox1.Text.Equals(QuerySyntaxHelper.MakeHeaderNameSensible(textBox1.Text),StringComparison.CurrentCultureIgnoreCase))
                {
                    btnOk.Enabled = false;
                    textBox1.ForeColor = Color.Red;
                    return;
                }
            }

            btnOk.Enabled = (!string.IsNullOrWhiteSpace(ResultText)) || _allowBlankText;
        }
    }
}
