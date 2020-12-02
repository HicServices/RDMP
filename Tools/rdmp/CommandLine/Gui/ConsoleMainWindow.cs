// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleMainWindow
    {
        private Window _win;
        private TreeView _treeView;
        private IBasicActivateItems _activator;

		const string Catalogues = "Catalogues";
		const string Projects = "Projects";
		const string Loads = "Data Loads";
		const string CohortConfigs = "Cohort Configurations";
		const string BuiltCohorts = "Built Cohorts";
		const string Other = "Other";

		const int TreeViewWidthPercent = 50;

		public View CurrentWindow {get;set;}

        public ConsoleMainWindow(ConsoleGuiActivator activator)
        {
			_activator = activator;
            activator.Published += Activator_Published;
		}

        private void Activator_Published(IMapsDirectlyToDatabaseTable obj)
        {
            _treeView.RefreshObject(obj,true);
        }

        private void Quit()
        {
			Application.RequestStop ();
        }

		/// <summary>
		/// Lays out the new view so that it fits in the right pane and fills the screen
		/// </summary>
		/// <param name="v"></param>
		public void SetSubWindow(View v)
        {
			ClearSubWindow();

			v.X = Pos.Percent(TreeViewWidthPercent);
			v.Y = 1;
			v.Width = Dim.Fill();
			v.Height = Dim.Fill();

			_win.Add(CurrentWindow = v);
        }

        internal void SetUp(Toplevel top)
        {
			var menu = new MenuBar (new MenuBarItem [] {
				new MenuBarItem ("_File (F9)", new MenuItem [] {
					new MenuItem ("_Find...", "", () => Find()),
					new MenuItem ("_Run...", "", () => Run()),
					new MenuItem ("_Refresh...", "", () => Publish()),
					new MenuItem ("_Quit", "", () => Quit()),
				}),
			});
			top.Add (menu);
				
			_win = new Window(){
				X = 0,
				Y = 1, // menu
				Width =  Dim.Fill(1),
				Height = Dim.Fill(1) // status bar
			};

			_treeView = new TreeView () {
				X = 0,
				Y = 0,
				Width = Dim.Percent(TreeViewWidthPercent),
				Height = Dim.Fill()
			};


			// Determines how to compute children of any given branch
			_treeView.ChildrenGetter = ChildGetter;
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

			_treeView.SelectionChanged += SelectionChanged;
            _treeView.KeyPress += treeView_KeyPress;

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.ControlQ, "~^Q~ Quit", () => Quit()),
				new StatusItem(Key.ControlN, "~^N~ Menu", () => Menu()),
				new StatusItem(Key.ControlR, "~^R~ Run", () => Run()),
				new StatusItem(Key.ControlF, "~^F~ Find", () => Find()),
				new StatusItem(Key.F5, "~F5~ Refresh", () => Publish()),
			});

			top.Add (statusBar);
        }

        private void Publish()
        {
			var obj = GetObjectIfAnyBehind(_treeView.SelectedObject);

			if(obj != null)
	            _activator.Publish(obj);
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

			if(desc == null)
				return;

			if(desc.Parents.Any())
            {
				var topLevelCategory = GetRootCategoryOf(desc.Parents[0].GetType());

				if(topLevelCategory != null)
					_treeView.Expand(topLevelCategory);
            }

			foreach(var p in desc.Parents)
				_treeView.Expand(p);

			_treeView.SelectedObject = selected;
			_treeView.ScrollOffset = _treeView.GetScrollOffsetOf(selected);
			_treeView.SetNeedsDisplay();
        }

        private void Menu()
        {
			var commands = GetCommands().ToArray();

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
			var o = _treeView.SelectedObject;
            if(o == null)
				return new IAtomicCommand[0];

			var factory = new AtomicCommandFactory(_activator);
			return factory.CreateCommands(o);
        }

        private void treeView_KeyPress(View.KeyEventEventArgs obj)
        {
            try
            {
				switch(obj.KeyEvent.Key)
				{
					case Key.DeleteChar : 

						if(_treeView.SelectedObject is IDeleteable d)
							_activator.DeleteWithConfirmation(d);

						obj.Handled = true;
						break;
				}
            }
            catch (Exception ex)
            {
				_activator.ShowException("Error",ex);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			var imaps = GetObjectIfAnyBehind(e.NewValue);

			if(imaps != null)
				SetSubWindow(new ConsoleGuiEdit(_activator,imaps));
			else
				ClearSubWindow();
        }

        private IMapsDirectlyToDatabaseTable GetObjectIfAnyBehind(object o)
        {
			if(o is IMasqueradeAs masquerade)
				return masquerade.MasqueradingAs() as IMapsDirectlyToDatabaseTable;
			
			return o as IMapsDirectlyToDatabaseTable;
        }

        private void ClearSubWindow()
        {
			if(CurrentWindow != null)
            {
				_win.Remove(CurrentWindow);
            }
        }

        private IEnumerable<object> ChildGetter(object model)
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
					return _activator.CoreChildProvider.AllCohortIdentificationConfigurations;
				
				if (ReferenceEquals(model , BuiltCohorts) && dx != null)
					return dx.Cohorts;

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
			if(type == typeof(CohortIdentificationConfiguration))	
				return CohortConfigs;
			if(type == typeof(ExtractableCohort))
				return BuiltCohorts;
			if(GetOtherCategoryChildren().Any(a=>a.Equals(o)))
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
    }
}
