// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleMainWindow
{
    private Window _win;
    private TreeView<object> _treeView;
    private readonly IBasicActivateItems _activator;

    /// <summary>
    /// The last <see cref="IBasicActivateItems"/> passed to this UI.
    /// Typically the same throughout the process lifetime.  This may
    /// be null if no main window/activator has been created yet
    /// </summary>
    public static ConsoleGuiActivator StaticActivator { get; set; }

    private MenuItem mi_default;
    private ColorScheme _defaultColorScheme;
    private MenuItem mi_green;
    private ColorScheme _greenColorScheme;
    private MouseFlags _rightClick = MouseFlags.Button3Clicked;

    // Last time the mouse moved and where it moved to
    private Point _lastMousePos = new(0, 0);
    private DateTime _lastMouseMove = DateTime.Now;

    public const string Catalogues = "Catalogues";
    public const string Projects = "Projects";
    public const string Loads = "Data Loads";
    public const string CohortConfigs = "Cohort Builder";
    public const string BuiltCohorts = "Built Cohorts";
    public const string Other = "Other";

    public View CurrentWindow { get; set; }

    /// <summary>
    /// Global scheme to apply to all windows
    /// </summary>
    public static ColorScheme ColorScheme { get; private set; }

    public ConsoleMainWindow(ConsoleGuiActivator activator)
    {
        _activator = activator;
        StaticActivator = activator;
        activator.Published += Activator_Published;
        activator.Emphasise += (s, e) => Show(e.Request.ObjectToEmphasise);
    }

    private void Activator_Published(IMapsDirectlyToDatabaseTable obj)
    {
        _treeView.RebuildTree();
    }

    private static void Quit()
    {
        Application.RequestStop();
    }

    internal void SetUp(Toplevel top)
    {
        var menu = new MenuBar(new MenuBarItem[]
        {
            new("_File (F9)", new MenuItem[]
            {
                new("_New...", "", () => New()),
                new("_Find...", "", () => Find()),
                new("_User Settings...", "", () => ShowUserSettings()),
                new("_Run...", "", () => Run()),
                new("_Refresh...", "", () => Publish()),
                new("_Quit", "", () => Quit())
            }),
            new("_Diagnostics", new MenuItem[]
            {
                mi_default = new MenuItem { Title = "Query Catalogue", Action = () => Query(nameof(CataloguePatcher)) },
                mi_default = new MenuItem
                    { Title = "Query Data Export", Action = () => Query(nameof(DataExportPatcher)) }
            }),
            new("_Color Scheme", new MenuItem[]
            {
                mi_default = new MenuItem
                {
                    Title = "Default", Checked = true, CheckType = MenuItemCheckStyle.Radio,
                    Action = () => SetColorScheme(mi_default)
                },
                mi_green = new MenuItem
                {
                    Title = "Green", Checked = false, CheckType = MenuItemCheckStyle.Radio,
                    Action = () => SetColorScheme(mi_green)
                }
            })
        });
        top.Add(menu);

        _win = new Window
        {
            X = 0,
            Y = 1, // menu
            Width = Dim.Fill(1),
            Height = Dim.Fill(1) // status bar
        };

        _defaultColorScheme = ColorScheme = _win.ColorScheme;
        _greenColorScheme = new ColorScheme
        {
            Disabled = Application.Driver.MakeAttribute(Color.Black, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.Black, Color.Green),
            HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.Green),
            HotNormal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black)
        };


        _treeView = new TreeView<object>
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            // Determines how to compute children of any given branch
            TreeBuilder = new DelegateTreeBuilder<object>(ChildGetter)
        };
        _treeView.AddObjects(
            new string[]
            {
                Catalogues,
                Projects,
                Loads,
                CohortConfigs,
                BuiltCohorts,
                Other
            });

        _win.Add(_treeView);
        top.Add(_win);

        Application.RootMouseEvent = OnRootMouseEvent;

        _treeView.ObjectActivationButton = _rightClick;
        _treeView.ObjectActivated += _treeView_ObjectActivated;
        _treeView.KeyPress += treeView_KeyPress;
        _treeView.SelectionChanged += _treeView_SelectionChanged;
        _treeView.AspectGetter = AspectGetter;

        var statusBar = new StatusBar(new StatusItem[]
        {
            new(Key.Q | Key.CtrlMask, "~^Q~ Quit", Quit),
            new(Key.R | Key.CtrlMask, "~^R~ Run", Run),
            new(Key.F | Key.CtrlMask, "~^F~ Find", Find),
            new(Key.N | Key.CtrlMask, "~^N~ New", New),
            new(Key.F5, "~F5~ Refresh", Publish)
        });

        top.Add(statusBar);

        var scheme = UserSettings.ConsoleColorScheme;

        if (scheme == "green") SetColorScheme(mi_green);
    }

    private void ShowUserSettings()
    {
        try
        {
            var dlg = new ConsoleGuiUserSettings(_activator);

            Application.Run(dlg, ExceptionPopup);
        }
        catch (Exception e)
        {
            _activator.ShowException("Unexpected error in open/edit tree", e);
        }
    }

    public static bool ExceptionPopup(Exception ex)
    {
        StaticActivator.ShowException("Error", ex);
        return true;
    }

    private void OnRootMouseEvent(MouseEvent obj)
    {
        _lastMousePos = new Point(obj.X, obj.Y);
        _lastMouseMove = DateTime.Now;
    }

    private void Query(string toQuery)
    {
        try
        {
            var cmd = new ExecuteCommandQueryPlatformDatabase(_activator, toQuery);
            cmd.Execute();
        }
        catch (Exception ex)
        {
            _activator.ShowException("Failed to build query", ex);
        }
    }

    private void SetColorScheme(MenuItem sender)
    {
        if (sender == mi_default)
        {
            _win.ColorScheme = ColorScheme = _defaultColorScheme;
            UserSettings.ConsoleColorScheme = "default";
        }

        if (sender == mi_green)
        {
            _win.ColorScheme = ColorScheme = _greenColorScheme;
            UserSettings.ConsoleColorScheme = "green";
        }

        foreach (var mi in new[] { mi_default, mi_green }) mi.Checked = mi == sender;

        _win.SetNeedsDisplay();
    }

    private void _treeView_ObjectActivated(ObjectActivatedEventArgs<object> obj)
    {
        try
        {
            Menu();
        }
        catch (Exception ex)
        {
            _activator.ShowException("Failed to build menu", ex);
        }
    }

    private string AspectGetter(object model)
    {
        return model switch
        {
            IContainer container => $"{container} ({container.Operation})",
            CohortAggregateContainer setContainer => $"{setContainer} ({setContainer.Operation})",
            ExtractionInformation ei => $"{ei} ({ei.ExtractionCategory})",
            CatalogueItemsNode cin => $"{cin} ({cin.CatalogueItems.Length})",
            TableInfoServerNode server => $"{server.ServerName} ({server.DatabaseType})",
            IDisableable d => d.IsDisabled ? $"{d} (Disabled)" : d.ToString(),
            _ => model is IArgument arg
                ? $"{arg} ({(string.IsNullOrWhiteSpace(arg.Value) ? "Null" : arg.Value)})"
                : model?.ToString() ?? "Null Object"
        };
    }

    private void Publish()
    {
        var obj = GetObjectIfAnyBehind(_treeView.SelectedObject);

        if (obj != null)
        {
            _activator.Publish(obj);
        }
        else
        {
            // Selected node is not refreshable

            //refresh any object (to update core child provider)
            var anyObject = _activator.CoreChildProvider.GetAllSearchables().Keys.FirstOrDefault();

            if (anyObject != null)
                _activator.Publish(anyObject);

            //and refresh the selected tree node
            _treeView.RefreshObject(_treeView.SelectedObject, true);
        }
    }

    private void Find()
    {
        try
        {
            var dlg = new ConsoleGuiSelectOne(_activator, null);

            if (dlg.ShowDialog()) Show(dlg.Selected);
        }
        catch (Exception e)
        {
            _activator.ShowException("Unexpected error in open/edit tree", e);
        }
    }

    private void Show(object selected)
    {
        var desc = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(selected);

        // In main RDMP, Projects are root level items so have no descendancy.  But in the console
        // gui we have a root category so give it a descendancy now so that expansion works properly
        if (selected is IProject) desc = new DescendancyList(Projects);

        if (desc == null)
            return;

        // In the main RDMP, we have a specific node for these but in console gui we have a text
        // category, fix the descendency for these objects
        if (desc.Parents.Length > 0 && desc.Parents[0] is AllCohortsNode) desc.Parents[0] = BuiltCohorts;

        if (desc.Parents.Any())
        {
            var topLevelCategory = GetRootCategoryOf(desc.Parents[0]);

            if (topLevelCategory != null)
                _treeView.Expand(topLevelCategory);
        }

        foreach (var p in desc.Parents)
            _treeView.Expand(p);

        _treeView.SelectedObject = selected;
        _treeView.ScrollOffsetVertical = _treeView.GetScrollOffsetOf(selected) - 1;
        _treeView.SetNeedsDisplay();
    }

    private void _treeView_SelectionChanged(object sender, [NotNull] SelectionChangedEventArgs<object> e)
    {
        if (e.NewValue != null)
            _treeView.RefreshObject(e.NewValue);
    }

    private void Menu()
    {
        var factory = new ConsoleGuiContextMenuFactory(_activator);
        var position = DateTime.Now.Subtract(_lastMouseMove).TotalSeconds < 1 ? _lastMousePos : new Point(10, 5);
        var menu = factory.Create(position.X,position.Y,_treeView.GetAllSelectedObjects().ToArray(), _treeView.SelectedObject);
        menu?.Show();
    }


    private void treeView_KeyPress(View.KeyEventEventArgs obj)
    {
        if (!_treeView.CanFocus || !_treeView.HasFocus) return;

        try
        {
            switch (obj.KeyEvent.Key)
            {
                case Key.DeleteChar:
                    var many = _treeView.GetAllSelectedObjects().ToArray();
                    obj.Handled = true;

                    //delete many at once?
                    if (many.Length > 1)
                    {
                        if (many.Cast<object>().All(d => d is IDeleteable))
                        {
                            var cmd = new ExecuteCommandDelete(_activator, many.Cast<IDeleteable>().ToArray());
                            if (!cmd.IsImpossible)
                                cmd.Execute();
                            else
                                _activator.Show("Cannot Delete", cmd.ReasonCommandImpossible);
                        }
                    }
                    else if (_treeView.SelectedObject is IDeleteable d)
                    {
                        // it is a single object selection
                        _activator.DeleteWithConfirmation(d);
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            _activator.ShowException("Error", ex);
        }
    }

    private static IMapsDirectlyToDatabaseTable GetObjectIfAnyBehind(object o) =>
        o is IMasqueradeAs masquerade
            ? masquerade.MasqueradingAs() as IMapsDirectlyToDatabaseTable
            : o as IMapsDirectlyToDatabaseTable;


    private IEnumerable<object> ChildGetter(object model)
    {
        return ChildGetterUnordered(model).OrderBy(o => o, new OrderableComparer(null));
    }


    private IEnumerable<object> ChildGetterUnordered(object model)
    {
        var dx = _activator.CoreChildProvider as DataExportChildProvider;

        try
        {
            // Top level brackets for the tree view
            if (ReferenceEquals(model, Catalogues))
                return new[] { _activator.CoreChildProvider.CatalogueRootFolder };

            if (ReferenceEquals(model, Projects) && dx != null)
                return new[] { dx.ProjectRootFolder };

            if (ReferenceEquals(model, Loads))
                return new[] { _activator.CoreChildProvider.LoadMetadataRootFolder };

            if (ReferenceEquals(model, CohortConfigs))
                return new[] { _activator.CoreChildProvider.CohortIdentificationConfigurationRootFolder };

            if (ReferenceEquals(model, BuiltCohorts) && dx != null)
                return dx.CohortSources;

            if (ReferenceEquals(model, Other))
                return GetOtherCategoryChildren();

            // don't show cic children (this is consistent with 'AxeChildren' in main collection RDMP client for Cohort Builder)
            if (model is CohortIdentificationConfiguration)
                return Array.Empty<object>();

            //sub brackets
            return _activator.CoreChildProvider.GetChildren(model) ?? Array.Empty<object>();
        }
        catch (Exception ex)
        {
            _activator.ShowException("Error getting node children", ex);
            return Array.Empty<object>();
        }
    }

    private IEnumerable<object> GetOtherCategoryChildren()
    {
        yield return _activator.CoreChildProvider.AllDashboardsNode;
        yield return _activator.CoreChildProvider.AllGovernanceNode;
        yield return _activator.CoreChildProvider.AllRDMPRemotesNode;
        yield return _activator.CoreChildProvider.AllObjectSharingNode;
        yield return _activator.CoreChildProvider.AllPipelinesNode;
        yield return _activator.CoreChildProvider.AllExternalServersNode;
        yield return _activator.CoreChildProvider.AllDataAccessCredentialsNode;
        yield return _activator.CoreChildProvider.AllANOTablesNode;
        yield return _activator.CoreChildProvider.AllServersNode;
        yield return _activator.CoreChildProvider.AllConnectionStringKeywordsNode;
        yield return _activator.CoreChildProvider.AllStandardRegexesNode;
        yield return _activator.CoreChildProvider.AllPluginsNode;
    }

    /// <summary>
    /// Returns the root category e.g. <see cref="BuiltCohorts"/> for the next level down Type <paramref name="t"/>
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private string GetRootCategoryOf(object o)
    {
        var type = o.GetType();

        if (type == typeof(FolderNode<Catalogue>) || type == typeof(FolderHelper))
            return Catalogues;

        if (type == typeof(Project))
            return Projects;
        if (type == typeof(LoadMetadata))
            return Loads;
        if (type == typeof(FolderNode<LoadMetadata>))
            return Loads;

        if (type == typeof(FolderNode<LoadMetadata>))
            return CohortConfigs;

        if (type == typeof(ExtractableCohort))
            return BuiltCohorts;
        return GetOtherCategoryChildren().Any(a => a.Equals(o)) ? Other : null;
    }

    private void Run()
    {
        var commandInvoker = new CommandInvoker(_activator);
        commandInvoker.CommandImpossible += (o, e) =>
        {
            _activator.Show(
                $"Command Impossible because:{e.Command.ReasonCommandImpossible}");
        };

        var commands = commandInvoker.GetSupportedCommands();

        var dlg = new ConsoleGuiBigListBox<Type>("Choose Command", "Run", true, commands.ToList(),
            t => BasicCommandExecution.GetCommandName(t.Name), false);
        if (dlg.ShowDialog())
            try
            {
                commandInvoker.ExecuteCommand(dlg.Selected, null);
            }
            catch (Exception exception)
            {
                _activator.ShowException("Run Failed", exception);
            }
    }

    private void New()
    {
        var commandInvoker = new CommandInvoker(_activator);
        commandInvoker.CommandImpossible += (o, e) =>
        {
            _activator.Show(
                $"Command Impossible because:{e.Command.ReasonCommandImpossible}");
        };

        try
        {
            commandInvoker.ExecuteCommand(typeof(ExecuteCommandNewObject), null);
        }
        catch (Exception exception)
        {
            _activator.ShowException("New Object Failed", exception);
        }
    }
}