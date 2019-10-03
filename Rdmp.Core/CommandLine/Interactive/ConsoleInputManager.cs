using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace Rdmp.Core.CommandLine.Interactive
{
    class ConsoleInputManager : ICommandInvokerArgProvider
    {
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public object PickValueType(ParameterInfo parameterInfo, Type paramType)
        {
            throw new NotImplementedException();
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
