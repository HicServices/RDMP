using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    public class ConsoleGuiEdit
    {
        public IMapsDirectlyToDatabaseTable DatabaseObject { get; }

        public ConsoleGuiEdit(IMapsDirectlyToDatabaseTable databaseObject)
        {
            DatabaseObject = databaseObject;
        }

        public void ShowDialog()
        {
            var win = new Window ("Edit") {
                X = 0,
                Y = 0, 

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };

            var btnSave = new Button("Save")
            {
                X = Pos.Center()-20,
                Y = Pos.Percent(100)-1,
                Width = 5,
                Height = 1
            };
            btnSave.Clicked = ((ISaveable)DatabaseObject).SaveToDatabase;

            var btnClose = new Button("Close")
            {
                X = Pos.Center() + 20,
                Y = Pos.Percent(100) - 1,
                Width = 5,
                Height = 1
            };

            btnClose.Clicked = Application.RequestStop;

            var infos = TableRepository.GetPropertyInfos (DatabaseObject.GetType());
            int requiredHeight = 0;
            List<View> toAdd = new List<View>();

            for (var index = 0; index < infos.Length; index++)
            {
                requiredHeight++;

                var prop = infos[index];
                var lbl = new Label(prop.Name +":")
                {
                    Y = index+1
                };

                toAdd.Add(lbl);
                
                View editControl = null;


                if(prop.Name == "ID")
                    editControl = new Label(prop.GetValue(DatabaseObject).ToString());
                else
                {
                    var text = new TextField((prop.GetValue(DatabaseObject)??string.Empty).ToString());
                    editControl = text;

                    TextField error = new TextField("!")
                    {
                        X = Pos.Right(text),
                        Y = text.Y,
                        Width = 1,
                        Height = 1
                    };
                    

                    text.Changed += (s, e) =>
                    {
                        try
                        {
                            
                            prop.SetValue(DatabaseObject, text.Text.ToString());
                           // text.SuperView?.Remove(error);
                        }
                        catch (Exception)
                        {
                         //   text.SuperView?.Add(error);
                        }
                    };
                }
                
                editControl.Y = index + 1;
                editControl.X = Pos.Right(lbl);
                editControl.Width = 160 - prop.Name.Length;
                editControl.Height = 1;
                toAdd.Add(editControl);
            }


            var scroll = new ScrollView(
                new Rect(0,0,Application.Top.Frame.Width - 1, Application.Top.Frame.Height - 1))
            {
                ShowVerticalScrollIndicator = true,

                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ContentSize = new Size(160, 400),
                ContentOffset = new Point(0, 0),
            };
            

            foreach (var a in toAdd) 
                scroll.Add(a);

            win.Add(scroll);
            scroll.Add(btnSave);
            scroll.Add(btnClose);

            Application.Run(win);

        }
    }
}