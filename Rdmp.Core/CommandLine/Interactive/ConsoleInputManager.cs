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
    class ConsoleInputManager : IBasicActivateItems
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        
        /// <inheritdoc/>
        public ICoreChildProvider CoreChildProvider { get; private set; }

        public ICheckNotifier GlobalErrorCheckNotifier { get; set; }
        

        public ConsoleInputManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            _repositoryLocator = repositoryLocator;
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

        private void RefreshChildProvider()
        {
            //todo pass the plugin child providers
            if(_repositoryLocator.DataExportRepository != null)
                CoreChildProvider = new DataExportChildProvider(_repositoryLocator,null,new ThrowImmediatelyCheckNotifier());
            else
                CoreChildProvider = new CatalogueChildProvider(_repositoryLocator.CatalogueRepository,null,new ThrowImmediatelyCheckNotifier());
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
            Console.WriteLine("Available Objects:");
            foreach (var o in availableObjects)
            {
                Console.WriteLine(o.GetType().Name + "|" + o.ID + "|" + o.ToString());
            }
            Console.WriteLine(prompt);
            Console.WriteLine("Format \"{Type}:{ID}\" e.g. \"Catalogue:123\"");

            var args = Console.ReadLine().Split(':');

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
                                options, cyclingDirection);
                        ConsoleExt.SetLine(autoCompletedLine);
                        break;
                }
            }
        }
    }
}
