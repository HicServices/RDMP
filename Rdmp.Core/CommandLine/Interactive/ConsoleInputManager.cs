using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;
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

        public DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            Console.WriteLine(taskDescription);
            var value = ReadLine(new PickDatabase());
            return value.Database;
        }

        public DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            Console.WriteLine(taskDescription);
            var value = ReadLine(new PickTable());
            return value.Table;
        }

        public void ShowException(string errorText, Exception exception)
        {
            throw exception ?? new Exception(errorText);
        }

        public void Wait(string title, Task task, CancellationTokenSource cts)
        {
            task.Wait(cts.Token);
        }

        public void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest)
        {
            //do nothing
        }

        public bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            string chosenStr = GetString(prompt, Enum.GetNames(enumType).ToList());
            chosen = (Enum)Enum.Parse(enumType, chosenStr);
            return true;
        }

        private void RefreshChildProvider()
        {
            //todo pass the plugin child providers
            if(RepositoryLocator.DataExportRepository != null)
                CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier());
            else
                CoreChildProvider = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository,null,new ThrowImmediatelyCheckNotifier());
        }


        public List<KeyValuePair<Type, Func<RequiredArgument, object>>> GetDelegates()
        {
            return new List<KeyValuePair<Type, Func<RequiredArgument, object>>>();
        }

        public IEnumerable<Type> GetIgnoredCommands()
        {
            return new Type[0];
        }

        public IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText)
        {
            Console.WriteLine(prompt);

            var value = ReadLine(new PickObjectBase[]
                {new PickObjectByID(RepositoryLocator), new PickObjectByName(RepositoryLocator)},
                availableObjects.Select(t=>t.GetType().Name).Distinct());
            
            var unavailable = value.DatabaseEntities.Except(availableObjects).ToArray();

            if(unavailable.Any())
                throw new Exception("The following objects were not among the listed available objects " + string.Join(",",unavailable.Select(o=>o.ToString())));

            return value.DatabaseEntities.ToArray();
        }

        public IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false)
        {
            Console.WriteLine(prompt);

            if (availableObjects.Length == 0)
                throw new Exception("No available objects found");

            //handle auto selection when there is one object
            if (availableObjects.Length == 1 && allowAutoSelect)
                return availableObjects[0];

            var value = ReadLine(new PickObjectBase[]
                {new PickObjectByID(RepositoryLocator), new PickObjectByName(RepositoryLocator)},
                availableObjects.Select(t=>t.GetType().Name).Distinct());

            var chosen = value.DatabaseEntities?.SingleOrDefault();

            if (chosen == null)
                return null;

            if(!availableObjects.Contains(chosen))
                throw new Exception("Picked object was not one of the listed available objects");

            return chosen;
        }

        private string ReadLine(IEnumerable<string> autoComplete = null)
        {
            return autoComplete != null ? GetString("", autoComplete.ToList()) : Console.ReadLine();
        }

        private CommandLineObjectPickerArgumentValue ReadLine(PickObjectBase picker)
        {
            Console.WriteLine($"Format:\r\n {picker.Format} \r\n Example(s): \r\n {string.Join(Environment.NewLine,picker.Examples)} \r\n Help: \r\n {picker.Help}");
            string line = ReadLine(picker.GetAutoCompleteIfAny());

            return picker.Parse(line, 0);
        }
        private CommandLineObjectPickerArgumentValue ReadLine(PickObjectBase[] pickers,IEnumerable<string> autoComplete)
        {
            Console.WriteLine("Enter value in one of the following formats:");

            foreach (PickObjectBase p in pickers)
                Console.WriteLine($"Format:\r\n {p.Format} \r\n Example(s): \r\n {string.Join(Environment.NewLine,p.Examples)} \r\n Help: \r\n {p.Help}");
            
            string line = ReadLine(autoComplete);
            
            var picker = new CommandLineObjectPicker(new[]{line},RepositoryLocator,pickers);
            return picker[0];
        }

        public DirectoryInfo SelectDirectory(string prompt)
        {
            Console.WriteLine(prompt);
            return new DirectoryInfo(Console.ReadLine());
        }
        public FileInfo SelectFile(string prompt)
        {
            Console.WriteLine(prompt);
            return new FileInfo(Console.ReadLine());
        }
        public IEnumerable<T> GetAll<T>()
        {
            //todo abstract base class!
            return CoreChildProvider.GetAllSearchables()
                .Keys.OfType<T>();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAll(Type t)
        {
            return CoreChildProvider.GetAllSearchables()
                .Keys.Where(t.IsInstanceOfType);
        }

        public object SelectValueType(string prompt, Type paramType)
        {
            Console.WriteLine("Enter value for " + prompt +":");

            if (paramType == typeof(string))
                return ReadLine();

            return UsefulStuff.ChangeType(ReadLine(), paramType);
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
                            return "exit";
                        else
                        {
                            var match = options.FirstOrDefault(c => c.ToLower() == lowerLine);

                            if (match != null)
                                return match;

                            return result.LineBeforeKeyPress.Line;
                        }

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
