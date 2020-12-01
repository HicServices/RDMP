// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using NStack;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiActivator : BasicActivateItems
    {
        public ConsoleGuiActivator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier) : base(repositoryLocator, globalErrorCheckNotifier)
        {
            InteractiveDeletes = true;
        }


        protected override bool SelectValueTypeImpl(string prompt, Type paramType, object initialValue, out object chosen)
        {
            var dlg = new ConsoleGuiTextDialog(prompt,initialValue?.ToString());
            if (dlg.ShowDialog())
            {
                chosen = dlg.ResultText;
                return true;
            }
            
            chosen = null;
            return false;
        }

        public override void Show(string message)
        {
            GetDialogDimensions(out var w, out var h);
            MessageBox.Query(w,h,"Message",message,"ok");
        }
        public override bool YesNo(string text, string caption, out bool chosen)
        {
            GetDialogDimensions(out var w, out var h);
            int result = MessageBox.Query(w,h,caption,text,"yes","no","cancel");
            chosen = result == 0;

            return result != 2;
        }

        private void GetDialogDimensions(out int w, out int h)
        {
            w = Math.Min(80,Application.Top.Frame.Width -4);
            h = Math.Min(20,Application.Top.Frame.Height -2);
        }

        public override bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            var dlg = new ConsoleGuiTextDialog(prompt,initialText);
            if (dlg.ShowDialog())
            {
                text = dlg.ResultText;
                return true;
            }

            text = null;
            return false;
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
        public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
        {
             var openDir = new OpenDialog(prompt,"Directory")
            {
                AllowsMultipleSelection = true,
                AllowedFileTypes = pattern == null ? null : new []{pattern.TrimStart('*')}
            };
            
            Application.Run(openDir);

            return openDir.FilePaths?.Select(f=>new FileInfo(f))?.ToArray();
        }

        public override bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            var dlg = new ConsoleGuiBigListBox<Enum>(prompt, "Ok", false, Enum.GetValues(enumType).Cast<Enum>().ToList(), null,false);

            if (dlg.ShowDialog())
            {
                chosen = dlg.Selected;
                return true;
            }

            chosen = null;
            return false;
        }

        public override bool SelectType(string prompt, Type[] available,out Type chosen)
        {
            var dlg = new ConsoleGuiBigListBox<Type>(prompt, "Ok", true, available.ToList(), null,true);

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
            var msg = GetExceptionText(errorText,exception,false);

            var textView = new TextView()
            {
                Text = msg,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2,
                ReadOnly = true
            };

            bool toggleStack = true;

            var btnOk = new Button("Ok", true);
            btnOk.Clicked += Application.RequestStop;
            var btnStack = new Button("Stack");
            btnStack.Clicked += () =>
            {
                //flip between stack / no stack
                textView.Text = GetExceptionText(errorText, exception, toggleStack);
                toggleStack = !toggleStack;
            };

            GetDialogDimensions(out var w, out var h);

            var dlg = new Dialog("Error",w,h,btnOk,btnStack);            
            dlg.Add(textView);
            
            Application.Run(dlg);
        }

        private ustring GetExceptionText(string errorText, Exception exception, bool includeStackTrace)
        {
            return Wrap(errorText + "\n" + ExceptionHelper.ExceptionToListOfInnerMessages(exception,includeStackTrace), 76);
        }

        private string Wrap(string longString, int width)
        {
            return string.Join("\n",Regex.Matches( longString, ".{1,"+width+"}" ).Select( m => m.Value ).ToArray());
        }
    }
}