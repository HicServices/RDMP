// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleDialogs;

using ScintillaNET;

namespace Rdmp.UI.PluginManagement.CodeGeneration
{
    /// <summary>
    /// TECHNICAL: Allows you as a C# programmer to generate RDMP code automatically to help you build plugins and particularly plugin ITableRepository databases more efficiently.
    /// </summary>
    public partial class GenerateClassCodeFromTableUI : Form
    {
        private Scintilla _codeEditor;

        public GenerateClassCodeFromTableUI()
        {
            InitializeComponent();

            var factory = new ScintillaTextEditorFactory();
            _codeEditor = factory.Create(null, SyntaxLanguage.CSharp);
            _codeEditor.ReadOnly = false;
            panel1.Controls.Add(_codeEditor);
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {

            try
            {
                var table = serverDatabaseTableSelector1.GetDiscoveredTable();
                var generator = new MapsDirectlyToDatabaseTableClassCodeGenerator(table);
                _codeEditor.Text = generator.GetCode();

            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}
