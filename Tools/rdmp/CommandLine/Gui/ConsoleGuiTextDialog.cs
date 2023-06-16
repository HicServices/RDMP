// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

class ConsoleGuiTextDialog
{
    private readonly DialogArgs _args;
    private readonly string _initialValue;

    public string ResultText;

    public ConsoleGuiTextDialog(DialogArgs args, string initialValue)
    {
        _args = args;
        _initialValue = initialValue;
    }

    public int? MaxLength { get; set; }

    public bool ShowDialog()
    {
        var okClicked = false;

        var win = new Window(_args.WindowTitle) {
            X = 0,
            Y = 0,

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(1),
            Height = Dim.Fill(1),
            Modal = true,
            ColorScheme = ConsoleMainWindow.ColorScheme
        };

        var description = new Label
        {
            Text = _args.TaskDescription ?? "",
            Y = 0
        };

        win.Add(description);

        var entryLabel = new Label
        {
            Text = _args.EntryLabel ?? "",
            Y = Pos.Bottom(description)
        };

        win.Add(entryLabel);

        var textField = new TextView
        {
            X = 1,
            Y = Pos.Bottom(entryLabel),
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            Text = _initialValue ?? "",
            AllowsTab = false,
            AllowsReturn = MaxLength is > BasicActivateItems.MultiLineLengthThreshold
        };

        win.Add(textField);

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
            Width = 13,
            Height = 1,
            IsDefault = false
        };
        btnCancel.Clicked += () =>
        {
            okClicked = false;
            Application.RequestStop();
        };

        var btnClear = new Button("C_lear", true)
        {
            X = Pos.Right(btnCancel),
            Y = Pos.Bottom(textField),
            Width = 13,
            Height = 1,
            IsDefault = false
        };
        btnClear.Clicked += () =>
        {
            textField.Text = "";
        };

        win.Add(btnOk);
        win.Add(btnCancel);
        win.Add(btnClear);

        Application.Run(win, ConsoleMainWindow.ExceptionPopup);

        return okClicked;
    }
}