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
using Terminal.Gui.Trees;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiViewLogs : Window, ITreeBuilder<object>
{
    private IBasicActivateItems _activator;
    private ArchivalDataLoadInfo[] _archivalDataLoadInfos = Array.Empty<ArchivalDataLoadInfo>();
    private TreeView<object> _treeView;
    private TextField _tbToFetch;
    private ILoggedActivityRootObject _rootObject;
    private TextField _tbcontains;

    public ConsoleGuiViewLogs(IBasicActivateItems activator, ILoggedActivityRootObject rootObject)
    {
        _activator = activator;
        Modal = true;
        _rootObject = rootObject;

        ColorScheme = ConsoleMainWindow.ColorScheme;

        var lbl = new Label($"Logs for '{rootObject}'");
        Add(lbl);

        var lblToFetch = new Label("Max:")
        {
            X = Pos.Right(lbl) + 1
        };

        Add(lblToFetch);

        _tbToFetch = new TextField
        {
            X = Pos.Right(lblToFetch),
            Text = "1000",
            Width = 10
        };

        Add(_tbToFetch);

        var btnFetch = new Button
        {
            X = Pos.Right(_tbToFetch),
            Text = "Go"
        };

        btnFetch.Clicked += FetchLogs;

        Add(btnFetch);

        var lblFilter = new Label("Filter:")
        {
            Y = Pos.Bottom(lbl)
        };

        Add(lblFilter);

        var btnAll = new Button("All")
        {
            Y = Pos.Bottom(lbl),
            X = Pos.Right(lblFilter)
        };
        btnAll.Clicked += BtnAll_Clicked;
        Add(btnAll);

        var btnFailing = new Button("Failing")
        {
            Y = Pos.Bottom(lbl),
            X = Pos.Right(btnAll)
        };
        btnFailing.Clicked += BtnFailing_Clicked;
        Add(btnFailing);

        var btnPassing = new Button("Passing")
        {
            Y = Pos.Bottom(lbl),
            X = Pos.Right(btnFailing)
        };
        btnPassing.Clicked += BtnPassing_Clicked;
        Add(btnPassing);


        var lblcontains = new Label("Contains:")
        {
            Y = Pos.Bottom(lbl),
            X = Pos.Right(btnPassing) + 1
        };

        Add(lblcontains);

        _tbcontains = new TextField
        {
            Y = Pos.Bottom(lbl),
            X = Pos.Right(lblcontains),
            Width = 10
        };
        _tbcontains.TextChanged += Tbcontains_TextChanged;
        Add(_tbcontains);

        _treeView = new TreeView<object>
        {
            X = 0,
            Y = Pos.Bottom(lblFilter),
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            TreeBuilder = this
        };
        _treeView.ObjectActivated += _treeView_ObjectActivated;
        Add(_treeView);

        var close = new Button("Quit")
        {
            Y = Pos.Bottom(_treeView),
            X = 0
        };
        close.Clicked += Quit;

        Add(close);

        FetchLogs();
    }

    private void Tbcontains_TextChanged(NStack.ustring obj)
    {
        _treeView.ClearObjects();

        if (string.IsNullOrWhiteSpace(_tbcontains.Text?.ToString()))
        {
            _treeView.AddObjects(_archivalDataLoadInfos);
        }
        else
        {
            _treeView.AddObjects(_archivalDataLoadInfos.Where(a => a.Description?.Contains(_tbcontains.Text.ToString()) ?? false));
        }
            
        _treeView.RebuildTree();
        _treeView.SetNeedsDisplay();
    }

    private void FetchLogs()
    {
        if (!int.TryParse(_tbToFetch.Text.ToString(), out var fetch))
        {
            fetch = 1000;
        }

        // no negative sized batches!
        fetch = Math.Max(0, fetch);

        try
        {

            var db = _rootObject.GetDistinctLoggingDatabase();
            var task = _rootObject.GetDistinctLoggingTask();

            var lm = new LogManager(db);
            _archivalDataLoadInfos = _rootObject.FilterRuns(lm.GetArchivalDataLoadInfos(task, null, null, fetch)).ToArray();

            _treeView.ClearObjects();
            _treeView.AddObjects(_archivalDataLoadInfos);
        }
        catch (Exception ex)
        {
            _activator.ShowException("Failed to fetch logs",ex);
        }
    }

    private void _treeView_ObjectActivated(ObjectActivatedEventArgs<object> obj)
    {
        _activator.Show(_treeView.AspectGetter(_treeView.SelectedObject));
    }

    private void BtnFailing_Clicked()
    {
        _treeView.ClearObjects();
        _treeView.AddObjects(_archivalDataLoadInfos.Where(a => a.HasErrors || !a.EndTime.HasValue));
    }

    private void BtnPassing_Clicked()
    {
        _treeView.ClearObjects();
        _treeView.AddObjects(_archivalDataLoadInfos.Where(a => !a.HasErrors && a.EndTime.HasValue));
    }

    private void BtnAll_Clicked()
    {
        _treeView.ClearObjects();
        _treeView.AddObjects(_archivalDataLoadInfos);
    }

    public bool SupportsCanExpand => true;

    public bool CanExpand(object model)
    {
        return model is ArchivalDataLoadInfo || (model is Category c && c.GetChildren().Any()) || model is ArchivalTableLoadInfo;
    }

    public IEnumerable<object> GetChildren(object model)
    {
        if (model is ArchivalDataLoadInfo dli)
        {
            yield return new Category(dli, LoggingTables.TableLoadRun);
            yield return new Category(dli, LoggingTables.FatalError);
            yield return new Category(dli, LoggingTables.ProgressLog);
        }

        if (model is Category c)
        {
            foreach (var child in c.GetChildren())
                yield return child;
        }

        if (model is ArchivalTableLoadInfo ti)
            foreach (var source in ti.DataSources)
                yield return source;
    }

    private void Quit()
    {
        Application.RequestStop();
    }

    private class Category
    {
        private readonly LoggingTables _type;
        private readonly ArchivalDataLoadInfo _dli;

        public Category(ArchivalDataLoadInfo dli, LoggingTables type)
        {
            _dli = dli;
            _type = type;
        }
        public override string ToString()
        {
            return _type switch
            {
                LoggingTables.FatalError => $"Errors ({_dli.Errors.Count:N0})",
                LoggingTables.TableLoadRun => $"Tables Loaded ({_dli.TableLoadInfos.Count:N0})",
                LoggingTables.ProgressLog => $"Progress Log ({_dli.Progress.Count:N0})",
                _ => base.ToString()
            };
        }

        internal IEnumerable<object> GetChildren()
        {
            return _type switch
            {
                LoggingTables.FatalError => _dli.Errors,
                LoggingTables.TableLoadRun => _dli.TableLoadInfos,
                LoggingTables.ProgressLog => _dli.Progress,
                _ => Enumerable.Empty<object>()
            };
        }
    }
}