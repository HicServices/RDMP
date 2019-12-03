// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandSet:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable _setOn;
        private PropertyInfo _property;
        
        /// <summary>
        /// The new value chosen by the user during command execution
        /// </summary>
        public object NewValue { get; private set; }
        
        /// <summary>
        /// True if the command was successfully completed 
        /// </summary>
        public bool Success { get; private set; }

        public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn):base(activator)
        {
            _setOn = setOn;
        }

        public ExecuteCommandSet(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable setOn,PropertyInfo property) : this(activator,setOn)
        {
            _property = property;
        }

        public override void Execute()
        {
            base.Execute();

            if(_property == null)
                if (BasicActivator.TypeText("Property To Set", "Property Name", 1000, null, out string propName, false))
                {
                    _property = _setOn.GetType().GetProperty(propName);

                    if (_property == null)
                    {
                        Show($"No property found called '{propName}' on Type '{_setOn.GetType().Name}'");
                        return;
                    }
                }

            if(_property == null)
                return;
            
            var val = BasicActivator.SelectValueType(_property.Name, _property.PropertyType, _property.GetValue(_setOn));
               
            NewValue = val;
            ShareManager.SetValue(_property,val,_setOn);
            ((DatabaseEntity)_setOn).SaveToDatabase();

            Success = true;
            Publish((DatabaseEntity) _setOn);
        }

    }
}
