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
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private CommandLineObjectPickerArgumentValue[] _arguments;
        
        public CommandLineObjectPickerArgumentValue this[int i] => _arguments[i];

        private readonly HashSet<PickObjectBase> _pickers = new HashSet<PickObjectBase>();
        
        /// <summary>
        /// Constructs a picker with all possible formats and immediately parse the provided <paramref name="args"/>
        /// </summary>
        /// <param name="args"></param>
        /// <param name="repositoryLocator"></param>
        public CommandLineObjectPicker(IEnumerable<string> args,
            IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            
            _pickers.Add(new PickObjectByID(repositoryLocator));
            _pickers.Add(new PickObjectByName(repositoryLocator));
            _pickers.Add(new PickDatabase());
            _pickers.Add(new PickTable());

            _arguments = args.Select(ParseValue).ToArray();


        }

        /// <summary>
        /// Constructs a picker with only the passed format(s) (<paramref name="pickers"/>) and immediately parse the provided <paramref name="args"/>
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pickers"></param>
        public CommandLineObjectPicker(string[] args,IRDMPPlatformRepositoryServiceLocator repositoryLocator, IEnumerable<PickObjectBase> pickers)
        {
            _repositoryLocator = repositoryLocator;

            foreach(PickObjectBase p in pickers)
                _pickers.Add(p);

            _arguments = args.Select(ParseValue).ToArray();
        }

        private CommandLineObjectPickerArgumentValue ParseValue(string arg,int idx)
        {
            //find a picker that recognizes the format
            var picker = _pickers.FirstOrDefault(p => p.IsMatch(arg, idx));

            if (picker != null)
                return picker.Parse(arg, idx);

            //perhaps it's a Type?
            var type =_repositoryLocator.CatalogueRepository.MEF.GetType(arg);

            if(type != null)
                return new CommandLineObjectPickerArgumentValue(arg,idx,type);

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
