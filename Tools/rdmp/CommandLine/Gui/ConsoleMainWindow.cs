// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleMainWindow
    {
        private Window _win;
        private TreeView<object> _treeView;
        private IBasicActivateItems _activator;

        private MenuItem mi_default;
        private ColorScheme _defaultColorScheme;
        private MenuItem mi_green;
        private ColorScheme _greenColorScheme;

        const string Catalogues = "Catalogues";
        const string Projects = "Projects";
        const string Loads = "Data Loads";
        const string CohortConfigs = "Cohort Configurations";
        const string BuiltCohorts = "Built Cohorts";
        const string Other = "Other";

        public View CurrentWindow {get;set;}

        /// <summary>
        /// Global scheme to apply to all windows
        /// </summary>
        public static ColorScheme ColorScheme { get; private set; }

        public ConsoleMainWindow(ConsoleGuiActivator activator)
        {
            _activator = activator;
            activator.Published += Activator_Published;
            activator.Emphasise += (s,e)=>Show(e.Request.ObjectToEmphasise);
        }

        private void Activator_Published(IMapsDirectlyToDatabaseTable obj)
        {
            _treeView.RebuildTree();
        }

        private void Quit()
        {
            Application.RequestStop ();
        }

        internal void SetUp(Toplevel top)
        {
            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("_File (F9)", new MenuItem [] {
                    new MenuItem ("_New...", "", () => New()),
                    new MenuItem ("_Find...", "", () => Find()),
                    new MenuItem ("_Run...", "", () => Run()),
                    new MenuItem ("_Refresh...", "", () => Publish()),
                    new MenuItem ("_Quit", "", () => Quit()),
                }),
                new MenuBarItem ("_Diagnostics", new MenuItem [] {
                    mi_default = new MenuItem (){Title = "Query Catalogue", Action = ()=>Query(nameof(CataloguePatcher))},
                    mi_default = new MenuItem (){Title = "Query Data Export", Action = ()=>Query(nameof(DataExportPatcher))},
                }),
                new MenuBarItem ("_Color Scheme", new MenuItem [] {
                    mi_default = new MenuItem (){Title = "Default", Checked = true, CheckType = MenuItemCheckStyle.Radio, Action = ()=>SetColorScheme(mi_default)},
                    mi_green = new MenuItem (){Title = "Green", Checked = false, CheckType = MenuItemCheckStyle.Radio, Action = ()=>SetColorScheme(mi_green)},
                }),
            });
            top.Add (menu);
                
            _win = new Window(){
                X = 0,
                Y = 1, // menu
                Width =  Dim.Fill(1),
                Height = Dim.Fill(1) // status bar
            };

            _defaultColorScheme = ColorScheme = _win.ColorScheme;
            _greenColorScheme = new ColorScheme()
            {
                Disabled = Application.Driver.MakeAttribute(Color.Black, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Black, Color.Green),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.Green),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
                Normal = Application.Driver.MakeAttribute(Color.Green, Color.Black),
            };


            _treeView = new TreeView<object> () {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };


            // Determines how to compute children of any given branch
            _treeView.TreeBuilder = new DelegateTreeBuilder<object>(ChildGetter);
            _treeView.AddObjects(
                new string[]{ 
                    Catalogues,
                    Projects,
                    Loads,
                    CohortConfigs,
                    BuiltCohorts,
                    Other});

            _win.Add(_treeView);
            top.Add(_win);

            _treeView.ObjectActivated += _treeView_ObjectActivated;
            _treeView.KeyPress += treeView_KeyPress;
            _treeView.SelectionChanged += _treeView_SelectionChanged;
            _treeView.AspectGetter = AspectGetter;

            var statusBar = new StatusBar (new StatusItem [] {
                new StatusItem(Key.Q | Key.CtrlMask, "~^Q~ Quit", () => Quit()),
                new StatusItem(Key.R | Key.CtrlMask, "~^R~ Run", () => Run()),
                new StatusItem(Key.F | Key.CtrlMask, "~^F~ Find", () => Find()),
                new StatusItem(Key.N | Key.CtrlMask, "~^N~ New", () => New()),
                new StatusItem(Key.F5, "~F5~ Refresh", () => Publish()),
            });

            top.Add (statusBar);

            string scheme = UserSettings.ConsoleColorScheme;

            if (scheme == "green")
            {
                SetColorScheme(mi_green);
            }
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
                _activator.ShowException("Failed to build query",ex);
            }
            
        }
        private void SetColorScheme(MenuItem sender)
        {
            if(sender == mi_default)
            {
                _win.ColorScheme = ColorScheme = _defaultColorScheme;
                UserSettings.ConsoleColorScheme = "default";
            }

            if (sender == mi_green)
            {
                _win.ColorScheme = ColorScheme = _greenColorScheme;
                UserSettings.ConsoleColorScheme = "green";
            }

            foreach(var mi in new[] {mi_default , mi_green })
            {
                mi.Checked = mi == sender;
            }

            _win.SetNeedsDisplay();
        }

        private void _treeView_ObjectActivated(ObjectActivatedEventArgs<object> obj)
        {
            Menu();
        }

        private string AspectGetter(object model)
        {
            if (model is IContainer container)
            {
                return $"{container} ({container.Operation})"; 
            }

            if (model is CohortAggregateContainer setContainer)
            {
                return $"{setContainer} ({setContainer.Operation})";
            }

            if (model is ExtractionInformation ei)
            {
                return $"{ei} ({ei.ExtractionCategory})";
            }

            if ( model is CatalogueItemsNode cin)
            {
                return $"{cin} ({cin.CatalogueItems.Length})";
            }

            if (model is TableInfoServerNode server)
            {
                return $"{server.ServerName} ({server.DatabaseType})";
            }

            if (model is IDisableable d)
            {
                return d.IsDisabled ? d.ToString() + " (Disabled)" : d.ToString();
            }

            if (model is IArgument arg)
            {
                return $"{arg} ({(string.IsNullOrWhiteSpace(arg.Value) ? "Null" : arg.Value)})";
            }
            return model?.ToString() ?? "Null Object";
        }

        private void Publish()
        {
            var obj = GetObjectIfAnyBehind(_treeView.SelectedObject);

            if(obj != null)
                _activator.Publish(obj);
            else
            {
                // Selected node is not refreshable

                //refresh any object (to update core child provider)
                var anyObject = _activator.CoreChildProvider.GetAllSearchables().Keys.FirstOrDefault();

                if(anyObject != null)
                    _activator.Publish(anyObject);

                //and refresh the selected tree node
                _treeView.RefreshObject(_treeView.SelectedObject,true);
            }
                
        }

        private void Find()
        {
            try
            {
                var dlg = new ConsoleGuiSelectOne(_activator.CoreChildProvider);
                
                if (dlg.ShowDialog())
                {
                    Show(dlg.Selected);
                }
            }
            catch (Exception e)
            {
                _activator.ShowException("Unexpected error in open/edit tree",e);
            }
        }

        private void Show(IMapsDirectlyToDatabaseTable selected)
        {
            var desc = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(selected);
            
            // In main RDMP, Projects are root level items so have no descendancy.  But in the console
            // gui we have a root category so give it a descendancy now so that expansion works properly
            if(selected is IProject)
            {
                desc = new DescendancyList(Projects);
            }

            if(desc == null)
                return;

            // In the main RDMP, we have a specific node for these but in console gui we have a text 
            // category, fix the descendency for these objects
            if(desc.Parents.Length  > 0 && desc.Parents[0] is AllCohortsNode)
            {
                desc.Parents[0] = BuiltCohorts;
            }

            if(desc.Parents.Any())
            {
                var topLevelCategory = GetRootCategoryOf(desc.Parents[0]);

                if(topLevelCategory != null)
                    _treeView.Expand(topLevelCategory);
            }

            foreach(var p in desc.Parents)
                _treeView.Expand(p);

            _treeView.SelectedObject = selected;
            _treeView.ScrollOffsetVertical = _treeView.GetScrollOffsetOf(selected)-1;
            _treeView.SetNeedsDisplay();
        }

        private void _treeView_SelectionChanged(object sender, SelectionChangedEventArgs<object> e)
        {
            if(e.NewValue != null)
                _treeView.RefreshObject(e.NewValue);
        }

        private void Menu()
        {
            var commands = GetCommands().ToArray();

            foreach(var gotoCommands in commands.OfType<ExecuteCommandShow>())
            {
                gotoCommands.FetchDestinationObjects();
                gotoCommands.OverrideCommandName = "Go To:" + gotoCommands.OverrideCommandName;
            }
            
            // only show viable commands
            commands = commands.Where(c=>!c.IsImpossible).ToArray();

            if(!commands.Any())
                return;
            
            var maxWidth = commands.Max(c=>c.GetCommandName().Length + 4);
            var windowWidth = maxWidth + 8;
            var windowHeight = commands.Length + 5;

            var btnCancel = new Button("Cancel");
            btnCancel.Clicked += ()=>Application.RequestStop();

            var dlg = new Dialog("Menu",windowWidth,windowHeight,btnCancel);
                    
            for(int i=0 ; i < commands.Length;i++)
            {
                var cmd = commands[i];
                var btn = new Button(cmd.GetCommandName());
                
                btn.Clicked += ()=>{
                    Application.RequestStop();
                    try
                    {
                        if(cmd.IsImpossible)
                            _activator.Show("Cannot run command because:" + Environment.NewLine + cmd.ReasonCommandImpossible);
                        else
                            cmd.Execute();
                    }
                    catch (Exception ex)
                    {
                        _activator.ShowException("Command Failed",ex);
                    }
                    
                    };

                var buttonWidth = maxWidth + 4;

                btn.X = (windowWidth/2) - (buttonWidth/2) - 1 /*window border*/;
                btn.Y = i;
                btn.Width = buttonWidth;
                btn.TextAlignment = TextAlignment.Centered;
                
                dlg.Add(btn);
            }

            Application.Run(dlg);
        }

        private IEnumerable<IAtomicCommand> GetCommands()
        {
            var factory = new AtomicCommandFactory(_activator);

            var many = _treeView.GetAllSelectedObjects().ToArray();

            if(many.Length > 1)
            {
                return factory.CreateManyObjectCommands(many).ToArray();
            }

            var o = _treeView.SelectedObject;

            if(ReferenceEquals(o,  Catalogues))
            {
                return new IAtomicCommand[] {
                    new ExecuteCommandCreateNewCatalogueByImportingFile(_activator),
                    new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator),
                };
            }
            if (ReferenceEquals(o, Loads))
            {
                return new IAtomicCommand[] {
                    new ExecuteCommandCreateNewLoadMetadata(_activator),
                };
            }
            if (ReferenceEquals(o, Projects))
            {
                return new IAtomicCommand[] {
                    new ExecuteCommandNewObject(_activator,typeof(Project)){OverrideCommandName = "New Project" }
                };
            }
            if (ReferenceEquals(o, CohortConfigs))
            {
                return new IAtomicCommand[] {
                    new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
                };
            }

            if (o == null)
                return new IAtomicCommand[0];

            return
                GetExtraCommands(o)
                .Union(factory.CreateCommands(o))
                .Union(_activator.PluginUserInterfaces.SelectMany(p=>p.GetAdditionalRightClickMenuItems(o)))
                .OrderBy(c=>c.Weight);
        }

        private IEnumerable<IAtomicCommand> GetExtraCommands(object o)
        {
            if(CommandFactoryBase.Is(o, out LoadMetadata lmd))
            {
                yield return new ExecuteCommandRunConsoleGuiView(_activator, 
                    () => new RunDleWindow(_activator, lmd)){ OverrideCommandName = "Execute Load..." };
            }

            if (CommandFactoryBase.Is(o, out Project p))
            {
                yield return new ExecuteCommandRunConsoleGuiView(_activator,
                    () => new RunReleaseWindow(_activator,p))
                { OverrideCommandName = "Release..." };
            }
            if (CommandFactoryBase.Is(o, out ExtractionConfiguration ec))
            {
                yield return new ExecuteCommandRunConsoleGuiView(_activator,
                    () => new RunReleaseWindow(_activator, ec))
                { OverrideCommandName = "Release..." };

                yield return new ExecuteCommandRunConsoleGuiView(_activator,
                    () => new RunExtractionWindow(_activator, ec))
                { OverrideCommandName = "Extract..." };
            }

            if(CommandFactoryBase.Is(o, out CacheProgress cp))
            {
                yield return new ExecuteCommandRunConsoleGuiView(_activator,
                    () => new RunCacheWindow(_activator, cp))
                { OverrideCommandName = "Run Cache..." };
            }

            if(CommandFactoryBase.Is(o, out Catalogue c) && !c.IsApiCall())
            {
                yield return new ExecuteCommandRunConsoleGuiView(_activator,
                    () => new RunDataQualityEngineWindow(_activator, c))
                { OverrideCommandName = "Run DQE..." };
            }
        }

        private void treeView_KeyPress(View.KeyEventEventArgs obj)
        {
            if(!_treeView.CanFocus || !_treeView.HasFocus)
            {
                return;
            }

            try
            {
                switch(obj.KeyEvent.Key)
                {
                    case Key.DeleteChar :
                        var many = _treeView.GetAllSelectedObjects().ToArray();
                        obj.Handled = true;

                        //delete many at once?
                        if (many.Length > 1)
                        {
                            if (many.Cast<object>().All(d => d is IDeleteable))
                            {
                                var cmd = new ExecuteCommandDelete(_activator, many.Cast<IDeleteable>().ToArray());
                                if(!cmd.IsImpossible)
                                {
                                    cmd.Execute();
                                }
                                else
                                {
                                    _activator.Show("Cannot Delete", cmd.ReasonCommandImpossible);
                                }
                            }
                        }
                        else
                        if(_treeView.SelectedObject is IDeleteable d)
                        {
                            // it is a single object selection
                            _activator.DeleteWithConfirmation(d);
                        }							

                        break;
                }
            }
            catch (Exception ex)
            {
                _activator.ShowException("Error",ex);
            }
        }

        private IMapsDirectlyToDatabaseTable GetObjectIfAnyBehind(object o)
        {
            if(o is IMasqueradeAs masquerade)
                return masquerade.MasqueradingAs() as IMapsDirectlyToDatabaseTable;
            
            return o as IMapsDirectlyToDatabaseTable;
        }


        private IEnumerable<object> ChildGetter(object model)
        {
            return ChildGetterUnordered(model).OrderBy(o=>o,new OrderableComparer(null));
        }


        private IEnumerable<object> ChildGetterUnordered(object model)
        {
            
            var dx = _activator.CoreChildProvider as DataExportChildProvider;

            try
            {
                // Top level brackets for the tree view
                if (ReferenceEquals(model , Catalogues))
                    return new []{CatalogueFolder.Root };
                
                if (ReferenceEquals(model , Projects)  && dx != null)
                    return dx.Projects;
                
                if (ReferenceEquals(model , Loads))
                    return _activator.CoreChildProvider.AllLoadMetadatas;
                
                if (ReferenceEquals(model , CohortConfigs))
                    if(dx != null)
                    {
                        return new object[]{
                            dx.AllProjectCohortIdentificationConfigurationsNode,
                            dx.AllFreeCohortIdentificationConfigurationsNode 
                            };
                    }
                    else
                        return _activator.CoreChildProvider.AllCohortIdentificationConfigurations;
                
                if (ReferenceEquals(model , BuiltCohorts) && dx != null)
                    return dx.CohortSources;

                if(ReferenceEquals(model,Other))
                    return GetOtherCategoryChildren();

                //sub brackets
                return _activator.CoreChildProvider.GetChildren(model) ?? new object[0];
            }
            catch (Exception ex)
            {
                _activator.ShowException("Error getting node children",ex);
                return new object[0];
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

            if(type == typeof(CatalogueFolder))
                return Catalogues;
            if(type == typeof(Project))
                return Projects;
            if(type == typeof(LoadMetadata))	
                return Loads;
            if (type == typeof(AllLoadMetadatasNode))
                return Loads;

            if (type == typeof(AllFreeCohortIdentificationConfigurationsNode) || 
                type == typeof(AllProjectCohortIdentificationConfigurationsNode) ||
                type == typeof(CohortIdentificationConfiguration))
                return CohortConfigs;

            if(type == typeof(ExtractableCohort))
                return BuiltCohorts;
            if (GetOtherCategoryChildren().Any(a=>a.Equals(o)))
                return Other;

            return null;
        }
        private void Run()
        {
            var commandInvoker = new CommandInvoker(_activator);
            commandInvoker.CommandImpossible += (o, e) => { _activator.Show("Command Impossible because:" + e.Command.ReasonCommandImpossible);};
            
            var commands = commandInvoker.GetSupportedCommands();

            var dlg = new ConsoleGuiBigListBox<Type>("Choose Command","Run",true,commands.ToList(),(t)=>BasicCommandExecution.GetCommandName(t.Name),false);
            if (dlg.ShowDialog())
                try
                {
                    commandInvoker.ExecuteCommand(dlg.Selected,null);
                }
                catch (Exception exception)
                {
                    _activator.ShowException("Run Failed",exception);
                }
        }
        private void New()
        {
            var commandInvoker = new CommandInvoker(_activator);
            commandInvoker.CommandImpossible += (o, e) => { _activator.Show("Command Impossible because:" + e.Command.ReasonCommandImpossible);};
            
            try
            {
                commandInvoker.ExecuteCommand(typeof(ExecuteCommandNewObject),null);
            }
            catch (Exception exception)
            {
                _activator.ShowException("New Object Failed",exception);
            }
        }

    }
}
