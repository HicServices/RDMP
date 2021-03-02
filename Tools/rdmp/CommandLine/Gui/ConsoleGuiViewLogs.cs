// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiViewLogs : Window, ITreeBuilder<object>
    {
        private ILoggedActivityRootObject rootObject;
        private IBasicActivateItems _activator;
        private TreeView<object> _treeView;

        public ConsoleGuiViewLogs(IBasicActivateItems activator, ILoggedActivityRootObject rootObject, ArchivalDataLoadInfo[] archivalDataLoadInfos)
        {
            this.rootObject = rootObject;
            this._activator = activator;
            Modal = true;

            _treeView = new TreeView<object>()
            {
                X= 0,
                Y=0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };
            _treeView.TreeBuilder = this;
            _treeView.AddObjects(archivalDataLoadInfos);
            _treeView.KeyPress += TreeView_KeyPress;
            Add(_treeView);

			var close = new Button("Quit"){
                Y = Pos.Bottom(_treeView),
                X = 0
            };
            close.Clicked += Quit;

			Add(close);
        }

        private void TreeView_KeyPress(KeyEventEventArgs obj)
        {
            if(obj.KeyEvent.Key == Key.Enter && _treeView.SelectedObject != null)
            {
                obj.Handled = true;
				_activator.Show(_treeView.AspectGetter(_treeView.SelectedObject));
            }
        }

        public bool SupportsCanExpand => true;

        public bool CanExpand(object model)
        {
            return model is ArchivalDataLoadInfo || model is Category || model is ArchivalTableLoadInfo;
        }

        public IEnumerable<object> GetChildren(object model)
        {
            if(model is ArchivalDataLoadInfo dli)
            {
                yield return new Category(dli,LoggingTables.TableLoadRun);
                yield return new Category(dli,LoggingTables.FatalError);
                yield return new Category(dli,LoggingTables.ProgressLog);
            }

            if(model is Category c)
            {
                foreach(var child in c.GetChildren())
                    yield return child;
            }

            if(model is ArchivalTableLoadInfo ti)
                foreach(var source in ti.DataSources)
                    yield return source;
        }

        private void Quit()
        {
            Application.RequestStop();
        }

        private class Category
        {
            private LoggingTables _type;
            private ArchivalDataLoadInfo _dli;

            public Category(ArchivalDataLoadInfo dli, LoggingTables type)
            {
                _dli = dli;
                _type = type;
            }
            public override string ToString()
            {
                switch(_type)
                {
                    case LoggingTables.FatalError : return $"Errors ({_dli.Errors.Count})";
                    case LoggingTables.TableLoadRun : return $"Tables Loaded ({_dli.TableLoadInfos.Count})";
                    case LoggingTables.ProgressLog : return "Progress Log";
                }

                return base.ToString();
            }

            internal IEnumerable<object> GetChildren()
            {  
                switch(_type)
                {
                    case LoggingTables.FatalError : return _dli.Errors;
                    case LoggingTables.TableLoadRun : return _dli.TableLoadInfos;
                    case LoggingTables.ProgressLog : return _dli.Progress;
                }

                return Enumerable.Empty<object>();
            }
        }
    }
}