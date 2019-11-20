using System;
using System.IO;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

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
            throw new NotImplementedException();
        }

        public override object SelectValueType(string prompt, Type paramType)
        {
            var dlg = new ConsoleGuiTextDialog(prompt,null);
            dlg.ShowDialog();

            return dlg.ResultText;
        }

        public override void Publish(DatabaseEntity databaseEntity)
        {
            throw new NotImplementedException();
        }

        public override void Show(string message)
        {
            throw new NotImplementedException();
        }

        public override bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override FileInfo SelectFile(string prompt)
        {
            throw new NotImplementedException();
        }

        public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
        {
            throw new NotImplementedException();
        }

        public override bool YesNo(string text, string caption)
        {
            throw new NotImplementedException();
        }

        public override bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            throw new NotImplementedException();
        }

        public override void ShowException(string errorText, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}