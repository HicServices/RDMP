// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows
{
    internal class ConsoleGuiTree
    {
        private readonly BasicActivateItems _activator;
        public IMapsDirectlyToDatabaseTable DatabaseObject { get; }

        public ConsoleGuiTree(ConsoleGuiActivator activator, IMapsDirectlyToDatabaseTable databaseObject)
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

            List<TreeNode> nodes = new List<TreeNode>();

            AddRecursively(DatabaseObject, 0, nodes);
            
            var list = new ListView(nodes)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2)
            };

            var btnSet = new Button("Edit")
            {
                X = 0,
                Y = Pos.Bottom(list),
                Width = 5,
                Height = 1,
                IsDefault = true,
                Clicked = ()=>
                {
                    if (list.SelectedItem != -1)
                        nodes[list.SelectedItem].Edit();
                }

            };

            var btnClose = new Button("Close")
            {
                X = Pos.Right(btnSet) + 3,
                Y = Pos.Bottom(list),
                Width = 5,
                Height = 1,
                Clicked = Application.RequestStop
            };
            
            win.Add(list);
            win.Add(btnSet);
            win.Add(btnClose);

            Application.Run(win);
        }

        private void AddRecursively(object current, int depth, List<TreeNode> nodes)
        {
            nodes.Add(new TreeNode(_activator,current,depth));
           
            foreach (var child in _activator.CoreChildProvider.GetChildren(current).OrderBy(o=>o is IOrderable ord ? ord.Order : int.MinValue))
                AddRecursively(child,depth+1,nodes);
        }

        /// <summary>
        /// A list view entry with the value of the field and 
        /// </summary>
        private class TreeNode
        {
            private readonly BasicActivateItems _activator;
            private readonly object _o;
            private readonly int _depth;

            public TreeNode(BasicActivateItems activator,object o,int depth)
            {
                _activator = activator;
                _o = o;
                _depth = depth;
            }

            public override string ToString()
            {
                return new string(' ',_depth) + _o;
            }

            public bool Edit()
            {
                if (_o is DatabaseEntity de)
                {
                    try
                    {
                        var edit = new ConsoleGuiEdit(_activator, de);
                        edit.ShowDialog();
                    }
                    catch (Exception e)
                    {
                        _activator.ShowException("Eror editing",e);
                    }
                    return true;
                }

                return false;
            }
        }
    }
}