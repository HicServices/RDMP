// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows
{
    class ConsoleGuiEdit : Window
    {
        private readonly IBasicActivateItems _activator;
        private List<PropertyInListView> collection;
        private ListView list;

        public IMapsDirectlyToDatabaseTable DatabaseObject { get; }

        public ConsoleGuiEdit(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable databaseObject)
        {
            _activator = activator;
            DatabaseObject = databaseObject;

            ColorScheme = ConsoleMainWindow.ColorScheme;
            collection =

                TableRepository.GetPropertyInfos(DatabaseObject.GetType())
                    .Select(p => new PropertyInListView(p, DatabaseObject)).ToList();

            list = new ListView(collection)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2)
            };
            list.KeyPress += List_KeyPress;

            var btnSet = new Button("Set")
            {
                X = 0,
                Y = Pos.Bottom(list),
                IsDefault = true
            };
            

            btnSet.Clicked += () =>
            {
                SetProperty(false);
            };

            var btnClose = new Button("Close")
            {
                X =  Pos.Right(btnSet),
                Y = Pos.Bottom(list)
            };
            btnClose.Clicked += ()=>Application.RequestStop();
            
            this.Add(list);
            this.Add(btnSet);
            this.Add(btnClose);
        }

        private void SetProperty(bool setNull)
        {
            if (list.SelectedItem != -1)
            {
                try
                {
                    var p = collection[list.SelectedItem];

                    var cmd = setNull ? new ExecuteCommandSet(_activator, DatabaseObject, p.PropertyInfo.Name, "null")
                                      : new ExecuteCommandSet(_activator, DatabaseObject, p.PropertyInfo);

                    if (cmd.IsImpossible)
                    {
                        _activator.Show("Error", cmd.ReasonCommandImpossible);
                        return;
                    }

                    cmd.Execute();

                    if (cmd.Success)
                    {

                        //redraws the list and re selects the current item

                        p.UpdateValue(cmd.NewValue ?? string.Empty);

                        var oldSelected = list.SelectedItem;
                        list.SetSource(collection = collection.ToList());
                        list.SelectedItem = oldSelected;
                        list.EnsureSelectedItemVisible();
                    }

                }
                catch (Exception e)
                {
                    _activator.ShowException("Failed to set Property", e);
                }
            }
        }

        private void List_KeyPress(KeyEventEventArgs obj)
        {
            if(obj.KeyEvent.Key == Key.DeleteChar)
            {
                int rly = MessageBox.Query("Clear", "Clear Property Value?", "Yes", "Cancel");
                if(rly == 0)
                {
                    obj.Handled = true;
                    SetProperty(true);
                }
            }
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

    public static class ListViewExtensions
    {
        public static void EnsureSelectedItemVisible(this ListView list)
        {
            if (list.SelectedItem < list.TopItem)
            {
                list.TopItem = list.SelectedItem;
            }
            else if (list.Frame.Height > 0 && list.SelectedItem >= list.TopItem + list.Frame.Height)
            {
                list.TopItem = Math.Max(list.SelectedItem - list.Frame.Height + 2, 0);
            }
        }
    }
}