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
                Height = Dim.Fill ()
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
                IsDefault = true,
                Clicked = () =>
                {
                    okClicked = true;
                    ResultText = textField.Text.ToString();
                    Application.RequestStop();
                }
            };

            var btnCancel = new Button("Cancel",true)
            {
                X = Pos.Right(btnOk),
                Y = Pos.Bottom(textField),
                Width = 10,
                Height = 1,
                Clicked = () =>
                {
                    okClicked = false;
                    Application.RequestStop();
                }
            };

            win.Add(textField);
            win.Add(btnOk);
            win.Add(btnCancel);

            Application.Run(win);

            return okClicked;
        }
    }
}
