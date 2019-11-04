// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
            _pickers.Add(new PickType(repositoryLocator));
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
            var pickers = _pickers.Where(p => p.IsMatch(arg, idx)).ToArray();

            if (pickers.Any())
                return pickers.First().Parse(arg, idx).Merge(pickers.Skip(1).Select(p=>p.Parse(arg,idx)));
            
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
