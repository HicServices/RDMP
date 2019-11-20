using System;
using System.IO;
using System.Linq;
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
            throw new NotImplementedException();
        }

        public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            throw new NotImplementedException();
        }

        public override IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null)
        {
            throw new NotImplementedException();
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

            var selected = openDir.FilePaths.FirstOrDefault();
            
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
            var dlg = new Dialog("Error", 100, 20,
                new Button("Ok", true){Clicked = Application.RequestStop});

            dlg.Add(new TextField(errorText + Environment.NewLine + exception.Message)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            });

            Application.Run(dlg);
        }
    }
}