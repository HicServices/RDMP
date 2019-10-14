using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    /// <summary>
    /// Parses arguments given along with the "cmd" command to rdmp.exe (<see cref="Rdmp.Core.CommandLine.Options.ExecuteCommandOptions"/>).  This
    /// allows the user to launch certain commands (<see cref="Rdmp.Core.CommandExecution.BasicCommandExecution"/>) from the CLI.
    /// </summary>
    public class CommandLineObjectPicker
    {
        private CommandLineObjectPickerArgumentValue[] _arguments;
        
        public CommandLineObjectPickerArgumentValue this[int i] => _arguments[i];

        private readonly HashSet<PickObjectBase> _pickers = new HashSet<PickObjectBase>();

        public CommandLineObjectPicker(IEnumerable<string> args,
            IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _arguments = args.Select(ParseValue).ToArray();

            _pickers.Add(new PickObjectByID(repositoryLocator));
            _pickers.Add(new PickObjectByName(repositoryLocator));
            _pickers.Add(new PickDatabase());
        }

        private CommandLineObjectPickerArgumentValue ParseValue(string arg,int idx)
        {
            //find a picker that recognizes the format
            var picker = _pickers.FirstOrDefault(p => p.IsMatch(arg, idx));

            if (picker != null)
                return picker.Parse(arg, idx);

            //nobody recognized it, use the raw value (maybe it's just a regular string, int etc).  Delay hard typing it till we know
            //what constructor we are trying to match it to.
            return new CommandLineObjectPickerArgumentValue(arg,idx);
        }



        /// <summary>
        /// Returns true if the given <paramref name="idx"/> exists and is populated with a value of the expected <paramref name="paramType"/>
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        public bool HasArgumentOfType(int idx, Type paramType)
        {
            //if the index is greater than the number of arguments we have
            if (idx >= _arguments.Length)
                return false;

            return _arguments[idx].HasValueOfType(paramType);
        }
    }
}
