using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using System;
using System.Collections.Generic;
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
		const string Loads = "Loads";
		const string CohortConfigs = "Cohort Configurations";
		const string BuiltCohorts = "Built Cohorts";

		const int TreeViewWidthPercent = 50;

		public View CurrentWindow {get;set;}

        public ConsoleMainWindow(IBasicActivateItems activator)
        {
			_activator = activator;
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
				new MenuBarItem ("_File", new MenuItem [] {
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
					BuiltCohorts });

			_win.Add(_treeView);
			top.Add(_win);

			_treeView.SelectionChanged += SelectionChanged;

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.ControlQ, "~^Q~ Quit", () => Quit()),
			});

			top.Add (statusBar);
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
			if(_treeView.SelectedObject is IMapsDirectlyToDatabaseTable o)
				SetSubWindow(new ConsoleGuiEdit(_activator,o));
			else
				ClearSubWindow();
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

				//sub brackets
			    return _activator.CoreChildProvider.GetChildren(model) ?? new object[0];
            }
            catch (Exception ex)
            {
				_activator.ShowException("Error getting node children",ex);
				return new object[0];
            }
        }
    }
}
