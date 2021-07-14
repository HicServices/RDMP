// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Changes a single property of an object and saves the new value to the database.  New value must be valid for the object and respect
    /// any Type / Database constraints.
    /// </summary>
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
        public ExecuteCommandSet(IBasicActivateItems activator,
            
            [DemandsInitialization("A single object on which you want to change a given property")]
            IMapsDirectlyToDatabaseTable setOn,
            [DemandsInitialization("Name of a property you want to change e.g. Description")]
            string property, 
            [DemandsInitialization("New value to assign, this will be parsed into a valid Type if property is not a string")]
            string value):base(activator)
        {
            _setOn = setOn;

            _property = _setOn.GetType().GetProperty(property);

            if(_property == null)
            {
                SetImpossible($"Unknown Property '{property}'");

                //suggest similar sounding properties
                var suggestions =
                    _setOn.GetType().GetProperties().Where(c => CultureInfo.CurrentCulture.CompareInfo.IndexOf(c.Name,property, CompareOptions.IgnoreCase) >= 0).ToArray();

                if (suggestions.Any())
                {
                    StringBuilder msg = new StringBuilder($"Unknown Property '{property}'");
                    msg.AppendLine();
                    msg.AppendLine("Did you mean:");
                    foreach (var s in suggestions)
                        msg.AppendLine(s.Name);
                    activator.Show(msg.ToString());
                }
            }
            else
            {
                var picker = new CommandLineObjectPicker(new string[]{value ?? "NULL"},activator.RepositoryLocator);

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

            if(string.Equals("ID",property?.Name))
            {
                SetImpossible("ID property cannot be changed");
            }

        }
        
        public override void Execute()
        {
            base.Execute();

            if(_property == null)
                return;
            
            if(_getValueAtExecuteTime)
            {
                bool populatedNewValueWithRelationship = false;

                // If the property we are getting a value for is a foreign key ID field then we should show the user the compatible objects
                var rel = _property.GetCustomAttribute(typeof(RelationshipAttribute)) as RelationshipAttribute;
                if(rel != null && (_property.PropertyType == typeof(int) || _property.PropertyType == typeof(int?)))
                {
                    if(typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(rel.Cref))
                    {
                        IMapsDirectlyToDatabaseTable[] available;

                        // is there a method that can be called to find compatible children for populating this property?
                        if (!string.IsNullOrWhiteSpace(rel.ValueGetter))
                        {
                            //get available from that method
                            var method = _setOn.GetType().GetMethod(rel.ValueGetter,new Type[0]);

                            if(method == null)
                            {
                                throw new Exception($"Could not find a method called '{rel.ValueGetter}' on Type '{_setOn.GetType()}'.  This was specified as a ValueGetter on Property {_property.Name}");
                            }

                            try
                            {
                                available = ((IEnumerable<IMapsDirectlyToDatabaseTable>)method.Invoke(_setOn, null)).ToArray();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error running method '{rel.ValueGetter}' on Type '{_setOn.GetType()}'.  This was specified as a ValueGetter on Property {_property.Name}",ex);
                            }
                        }
                        else
                        {
                            available = BasicActivator.GetAll(rel.Cref).ToArray();
                        }

                        NewValue = BasicActivator.SelectOne(_property.Name, available)?.ID;
                        populatedNewValueWithRelationship = true;
                    }
                }
                
                if(!populatedNewValueWithRelationship)
                {
                    if (BasicActivator.SelectValueType(_property.Name, _property.PropertyType, _property.GetValue(_setOn), out object chosen))
                    {
                        NewValue = chosen;
                    }
                    else
                    {
                        Success = false;
                        return;
                    }
                }
            }
            
            ShareManager.SetValue(_property,NewValue,_setOn);
            ((DatabaseEntity)_setOn).SaveToDatabase();

            Success = true;
            Publish((DatabaseEntity) _setOn);
        }

    }
}
