// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiTextDialog
    {
        private readonly string _prompt;
        private readonly string _initialText;

        public string ResultText;

        public ConsoleGuiTextDialog(string prompt, string initialText)
        {
            _prompt = prompt;
            _initialText = initialText;
        }
        public bool ShowDialog()
        {
            bool okClicked = false;

            var win = new Window (_prompt) {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill (),
                Modal = true,
                ColorScheme = ConsoleMainWindow.ColorScheme
            };

            var textField = new TextField(_initialText ?? "")
            {
                X = 1,
                Y = 1,
                Height = Dim.Fill(2),
                Width = Dim.Fill(2)
            };
            
            var btnOk = new Button("Ok",true)
            {
                X = 0,
                Y = Pos.Bottom(textField),
                Width = 10,
                Height = 1,
                IsDefault = true
            };
            btnOk.Clicked += () =>
            {
                okClicked = true;
                ResultText = textField.Text.ToString();
                Application.RequestStop();
            };

            var btnCancel = new Button("Cancel",true)
            {
                X = Pos.Right(btnOk),
                Y = Pos.Bottom(textField),
                Width = 10,
                Height = 1
            };
            btnCancel.Clicked += () =>
            {
                okClicked = false;
                Application.RequestStop();
            };

            win.Add(textField);
            win.Add(btnOk);
            win.Add(btnCancel);

            Application.Run(win);

            return okClicked;
        }
    }
}
