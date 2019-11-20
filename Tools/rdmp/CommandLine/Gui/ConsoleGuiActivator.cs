// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiActivator : BasicActivateItems
    {
        public ConsoleGuiActivator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier) : base(repositoryLocator, globalErrorCheckNotifier)
        {
            CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,GlobalErrorCheckNotifier);
        }

        public override bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            deleteable.DeleteInDatabase();
            return true;
        }

        public override object SelectValueType(string prompt, Type paramType, object initialValue)
        {
            if (paramType.IsEnum)
                return SelectEnum(prompt, paramType, out Enum chosen) ? chosen : null;

            var dlg = new ConsoleGuiTextDialog(prompt,initialValue?.ToString());
            dlg.ShowDialog();

            return dlg.ResultText;
        }

        public override void Publish(DatabaseEntity databaseEntity)
        {
            
        }

        public override void Show(string message)
        {
            var dlg = new Dialog("Message", 100, 20,
                new Button("Ok", true){Clicked = Application.RequestStop});

            dlg.Add(new TextField(message)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            });

            Application.Run(dlg);
        }

        public override bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            var dlg = new ConsoleGuiTextDialog(prompt,initialText);
            bool okPressed = dlg.ShowDialog();

            text = dlg.ResultText;

            return okPressed;
        }

        public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            var dlg = new ConsoleGuiServerDatabaseTableSelector(this, taskDescription, "Ok",false);
            if (dlg.ShowDialog())
                return dlg.GetDiscoveredDatabase();

            return null;
        }

        public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            var dlg = new ConsoleGuiServerDatabaseTableSelector(this, taskDescription, "Ok",true);
            if (dlg.ShowDialog())
                return dlg.GetDiscoveredTable();

            return null;
        }

        public override IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null)
        {
            //todo make this handle multi selection
            var chosen = SelectOne(prompt, availableObjects, initialSearchText);
            return chosen == null?null : new []{chosen};
        }

        public override IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false)
        {
            var dlg = new ConsoleGuiSelectOne(CoreChildProvider,availableObjects);
            if (dlg.ShowDialog())
                return dlg.Selected;

            return null;
        }

        public override DirectoryInfo SelectDirectory(string prompt)
        {
            var openDir = new OpenDialog(prompt,"Directory"){AllowsMultipleSelection = false};
            
            Application.Run(openDir);

            var selected = openDir.DirectoryPath?.ToString();
            
            return selected == null ? null : new DirectoryInfo(selected);

        }

        public override FileInfo SelectFile(string prompt)
        {
            var openDir = new OpenDialog(prompt,"Directory"){AllowsMultipleSelection = false};
            
            Application.Run(openDir);

            var selected = openDir.FilePaths.FirstOrDefault();
            
            return selected == null ? null : new FileInfo(selected);
        }

        public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
        {
            var openDir = new OpenDialog(prompt,"Directory")
            {
                AllowsMultipleSelection = false,
                AllowedFileTypes = pattern == null ? null : new []{pattern.TrimStart('*')}
            };
            
            Application.Run(openDir);

            var selected = openDir.FilePaths.FirstOrDefault();
            
            return selected == null ? null : new FileInfo(selected);
        }

        public override bool YesNo(string text, string caption)
        {
            bool toReturn = false;

            var dlg = new Dialog(caption, 100, 20,
                new Button("Yes", true){Clicked = () =>
                    {
                        toReturn = true;
                        Application.RequestStop();
                    }
                },
                new Button("No"){Clicked = Application.RequestStop}
                );

            dlg.Add(new TextField(text )
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            });

            Application.Run(dlg);

            return toReturn;
        }

        public override bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            var dlg = new ConsoleGuiBigListBox<Enum>(prompt, "Ok", false, Enum.GetValues(enumType).Cast<Enum>().ToList(), null);

            if (dlg.ShowDialog())
            {
                chosen = dlg.Selected;
                return true;
            }

            chosen = null;
            return false;
        }

        public override void ShowException(string errorText, Exception exception)
        {
            var dlg = new Dialog("Error", 80, 20,
                new Button("Ok", true){Clicked = Application.RequestStop});

            dlg.Add(new Label(Wrap(errorText + Environment.NewLine + exception.Message,76))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            });

            Application.Run(dlg);
        }

        private string Wrap(string longString, int width)
        {
            return string.Join("\n",Regex.Matches( longString, ".{1,"+width+"}" ).Select( m => m.Value ).ToArray());
        }
    }
}