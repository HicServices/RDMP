using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.Curation.Data;
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
					new MenuItem ("_Run...", "", () => Run()),
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
            _treeView.KeyPress += treeView_KeyPress;

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.ControlQ, "~^Q~ Quit", () => Quit()),
				new StatusItem(Key.ControlM, "~^M~ Menu", () => Menu()),
				new StatusItem(Key.ControlR, "~^R~ Run", () => Run()),
			});

			top.Add (statusBar);
        }

        private void Menu()
        {
			var commands = GetCommands().ToArray();

			if(!commands.Any())
				return;
			
			var maxWidth = commands.Max(c=>c.GetCommandName().Length);
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
					cmd.Execute();
					};

				var buttonWidth = maxWidth + 4;

				btn.X = (windowWidth/2) - (buttonWidth/2) - 1 /*window border*/;
				btn.Y = i;
				btn.Width = buttonWidth;
				
				dlg.Add(btn);
            }

			Application.Run(dlg);
        }

        private IEnumerable<IAtomicCommand> GetCommands()
        {
			var o = _treeView.SelectedObject;
            if(o == null)
				yield break;

			if(o is IDeleteable d)
				yield return new ExecuteCommandDelete(_activator,d);
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
			var imaps = e.NewValue as IMapsDirectlyToDatabaseTable;

			if(e.NewValue is IMasqueradeAs masquerade)
					imaps = masquerade.MasqueradingAs() as IMapsDirectlyToDatabaseTable;

			if(imaps != null)
				SetSubWindow(new ConsoleGuiEdit(_activator,imaps));
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
