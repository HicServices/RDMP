// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using NLog;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Gui.Windows;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiRunner : IRunner
    {
        private ConsoleGuiActivator _activator;

        public ConsoleGuiRunner(ConsoleGuiOptions options)
        {
            
        }
        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            _activator = new ConsoleGuiActivator(repositoryLocator,checkNotifier);

            
            LogManager.DisableLogging();

            Application.Init ();
            var top = Application.Top;
            
            // Creates the top-level window to show
            var win = new Window ("RDMP v" + 
FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion) {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };
            top.Add (win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem("_Open","",Open),
                    new MenuItem("Open _Tree","",OpenTree),
                    new MenuItem("_Run","",Run),
                    new MenuItem("Re_fresh","",Refresh),
                    new MenuItem ("_Quit", "", () => { top.Running = false; })
                })
            });


            top.Add (menu);
            
            top.Add(new Label("Press F9 for menu")
            {
                X = Pos.Center(),
                Y = Pos.Center()
            });

            try
            {
                Application.Run();
            }
            catch (Exception e)
            {
                _activator.ShowException("Application Crashed",e);
                top.Running = false;
                return -1;
            }

            return 0;
        }

        private void Refresh()
        {
            //one day this might be a problem but not today
            _activator.Publish(null);
        }

        private void OpenTree()
        {
            try
            {
                var dlg = new ConsoleGuiSelectOne(_activator.CoreChildProvider);
                if (dlg.ShowDialog())
                {
                    var edit = new ConsoleGuiTree(_activator,dlg.Selected);
                    edit.ShowDialog();
                }
            }
            catch (Exception e)
            {
                _activator.ShowException("Unexpected error in open/edit tree",e);
            }
        }

        private void Run()
        {
            var commandInvoker = new CommandInvoker(_activator);

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

        public void Open()
        {
            try
            {
                var dlg = new ConsoleGuiSelectOne(_activator.CoreChildProvider);
                if (dlg.ShowDialog())
                {
                    var edit = new ConsoleGuiEdit(_activator,dlg.Selected);
                    edit.ShowDialog();
                }
            }
            catch (Exception e)
            {
                _activator.ShowException("Unexpected error in open/edit",e);
            }
        }
    }
}

