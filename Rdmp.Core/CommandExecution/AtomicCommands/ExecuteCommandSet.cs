// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandSet:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable _setOn;


        public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn):base(activator)
        {
            _setOn = setOn;
        }

        public override void Execute()
        {
            base.Execute();

            if (BasicActivator.TypeText("Property To Set", "Property Name", 1000, null, out string propName, false))
            {
                var prop = _setOn.GetType().GetProperty(propName);

                if (prop == null)
                {
                    Show($"No property found called '{propName}' on Type '{_setOn.GetType().Name}'");
                    return;
                }

                var invoker = new CommandInvoker(BasicActivator);

                var val = invoker.GetValueForParameterOfType(prop);
                
                prop.SetValue(_setOn,val);
                ((DatabaseEntity)_setOn).SaveToDatabase();
                
                Publish((DatabaseEntity) _setOn);
            }
        }
    }
}
