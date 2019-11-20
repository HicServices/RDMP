using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.ImportExport;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows
{
    public class ConsoleGuiEdit
    {
        private readonly BasicActivateItems _activator;
        public IMapsDirectlyToDatabaseTable DatabaseObject { get; }

        public ConsoleGuiEdit(BasicActivateItems activator, IMapsDirectlyToDatabaseTable databaseObject)
        {
            _activator = activator;
            DatabaseObject = databaseObject;
        }

        public void ShowDialog()
        {
            var win = new Window("Edit " + DatabaseObject.GetType().Name)
            {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };


            var collection =

                TableRepository.GetPropertyInfos(DatabaseObject.GetType())
                    .Select(p => new PropertyInListView(p, DatabaseObject)).ToList();

            var list = new ListView(collection)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2)
            };

            var btnSet = new Button("Set")
            {
                X = 0,
                Y = Pos.Bottom(list),
                Width = 5,
                Height = 1,
                IsDefault = true
            };

            var btnClose = new Button("Close")
            {
                X = Pos.Right(btnSet) + 3,
                Y = Pos.Bottom(list),
                Width = 5,
                Height = 1
            };

            btnClose.Clicked = Application.RequestStop;
            btnSet.Clicked = () =>
            {
                if (list.SelectedItem != -1)
                {
                    try
                    {
                        var p = collection[list.SelectedItem];

                        var cmd = new ExecuteCommandSet(_activator, DatabaseObject, p.PropertyInfo);
                        cmd.Execute();

                        if (cmd.Success)
                        {
                            
                            //redraws the list and re selects the current item

                            p.UpdateValue(cmd.NewValue ?? string.Empty);
                            list.SetSource(collection = collection.ToList());
                            list.SelectedItem = list.SelectedItem;
                        }
                        
                    }
                    catch (Exception e)
                    {
                        _activator.ShowException("Failed to set Property",e);
                    }
                    
                }
            };

            win.Add(list);
            win.Add(btnSet);
            win.Add(btnClose);

            Application.Run(win);
        }

        /// <summary>
        /// A list view entry with the value of the field and 
        /// </summary>
        private class PropertyInListView
        {
            public PropertyInfo PropertyInfo;
            public string DisplayMember;

            public PropertyInListView(PropertyInfo p, IMapsDirectlyToDatabaseTable o)
            {
                PropertyInfo = p;
                UpdateValue(p.GetValue(o));

            }

            public override string ToString()
            {
                return DisplayMember;
            }

            /// <summary>
            /// Updates the <see cref="DisplayMember"/> to indicate the new value
            /// </summary>
            /// <param name="newValue"></param>
            public void UpdateValue(object newValue)
            {
                DisplayMember = PropertyInfo.Name + ":" + newValue;
            }
        }
    }
}