// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandSet:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable _setOn;
        private readonly PropertyInfo _property;
        private readonly bool _getValueAtExecuteTime;

        /// <summary>
        /// The new value chosen by the user during command execution
        /// </summary>
        public object NewValue { get; private set; }
        
        /// <summary>
        /// True if the command was successfully completed 
        /// </summary>
        public bool Success { get; private set; }

        [UseWithObjectConstructor]
        public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn,string property, string value):base(activator)
        {
            _setOn = setOn;

            _property = _setOn.GetType().GetProperty(property);

            if(_property == null)
                SetImpossible($"Unknown Property '{property}'");
            else
            {
                var invoker = new CommandInvoker(activator);
                
                var picker = new CommandLineObjectPicker(new string[]{value},activator.RepositoryLocator);

                if(!picker.HasArgumentOfType(0,_property.PropertyType))
                {
                    SetImpossible($"Provided value could not be converted to '{_property.PropertyType}'");
                }
                else
                    NewValue = picker[0].GetValueForParameterOfType(_property.PropertyType);
            }
        }
        public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn,PropertyInfo property):base(activator)
        {
            _setOn = setOn;
            _property = property;
            _getValueAtExecuteTime = true;
        }
        
        public override void Execute()
        {
            base.Execute();

            if(_property == null)
                return;
            
            if(_getValueAtExecuteTime)
            {
                var val = BasicActivator.SelectValueType(_property.Name, _property.PropertyType, _property.GetValue(_setOn));
                NewValue = val;
            }
            
            ShareManager.SetValue(_property,NewValue,_setOn);
            ((DatabaseEntity)_setOn).SaveToDatabase();

            Success = true;
            Publish((DatabaseEntity) _setOn);
        }

    }
}
