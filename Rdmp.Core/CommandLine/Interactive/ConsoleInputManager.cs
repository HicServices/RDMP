using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Implementation of <see cref="IBasicActivateItems"/> that handles object selection and message notification via the console
    /// </summary>
    public class ConsoleInputManager : IBasicActivateItems
    {
        /// <inheritdoc/>
        public ICoreChildProvider CoreChildProvider { get; private set; }

        public ICheckNotifier GlobalErrorCheckNotifier { get; set; }
        

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        public ConsoleInputManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            RepositoryLocator = repositoryLocator;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;

            RefreshChildProvider();

        }
        public void Publish(DatabaseEntity databaseEntity)
        {
            RefreshChildProvider();
        }

        public void Show(string message)
        {
            Console.WriteLine(message);
        }

        public bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            Console.WriteLine(header);
            Console.Write(prompt +":");
            text = ReadLine();
            return !string.IsNullOrWhiteSpace(text);
        }

        private void RefreshChildProvider()
        {
            //todo pass the plugin child providers
            if(RepositoryLocator.DataExportRepository != null)
                CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier());
            else
                CoreChildProvider = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository,null,new ThrowImmediatelyCheckNotifier());
        }


        public Dictionary<Type, Func<object>> GetDelegates()
        {
            return new Dictionary<Type, Func<object>>();
        }

        public IEnumerable<Type> GetIgnoredCommands()
        {
            return new Type[0];
        }

        public object PickMany(ParameterInfo parameterInfo, Type arrayElementType, IMapsDirectlyToDatabaseTable[] availableObjects)
        {
            throw new NotImplementedException();
        }

        public object SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null,bool allowAutoSelect = false)
        {
            Console.WriteLine("Format \"{Type}:{ID}\" e.g. \"Catalogue:123\"");

            var args = ReadLine().Split(':');

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid format");
                return null;
            }

            var id = int.Parse(args[1]);

            return availableObjects.Single(o =>
                o.ID == id &&
                o.GetType().Name.Equals(args[0].Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        private string ReadLine()
        {
            return Console.ReadLine();
        }

        public DirectoryInfo PickDirectory(ParameterInfo parameterInfo, Type paramType)
        {
            throw new NotImplementedException();
        }

        public void OnCommandImpossible(IAtomicCommand instance)
        {
            throw new NotImplementedException();
        }

        public void OnCommandFinished(IAtomicCommand instance)
        {
            throw new NotImplementedException();
        }

        public void OnCommandExecutionException(IAtomicCommand instance, Exception exception)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAll<T>()
        {
            //todo abstract base class!
            return CoreChildProvider.GetAllSearchables()
                .Keys.OfType<T>()
                .Cast<IMapsDirectlyToDatabaseTable>();
        }

        public object PickValueType(ParameterInfo parameterInfo, Type paramType)
        {
            if (paramType == typeof(string))
                return ReadLine();

            throw new NotImplementedException();
        }

        public bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            deleteable.DeleteInDatabase();

            return true;
        }

        public bool YesNo(string text, string caption)
        {
            Console.WriteLine(text + "(y/n)");
            return string.Equals(Console.ReadLine(), "y", StringComparison.CurrentCultureIgnoreCase);
        }

        public string GetString(string prompt, List<string> options)
        {
            Console.WriteLine(prompt +":");
            
            var cyclingAutoComplete = new CyclingAutoComplete();
            while (true)
            {
                var result = ConsoleExt.ReadKey();
                switch (result.Key)
                {
                    case ConsoleKey.Enter:
                        var lowerLine = result.LineBeforeKeyPress.Line.ToLower();
                        if (lowerLine == "exit")
                            return null;
                        else
                        {
                            var match = options.FirstOrDefault(c => c.ToLower() == lowerLine);

                            if (match != null)
                                return match;
                        }
                        break;
                    case ConsoleKey.Tab:
                        var shiftPressed = (result.Modifiers & ConsoleModifiers.Shift) != 0;
                        var cyclingDirection = shiftPressed ? CyclingDirections.Backward : CyclingDirections.Forward;
                        var autoCompletedLine =
                            cyclingAutoComplete.AutoComplete(result.LineBeforeKeyPress.LineBeforeCursor,
                                options, cyclingDirection,true);
                        ConsoleExt.SetLine(autoCompletedLine);
                        break;
                }
            }
        }
    }
}
