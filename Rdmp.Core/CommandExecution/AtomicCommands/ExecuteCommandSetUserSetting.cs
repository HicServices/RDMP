using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    public class ExecuteCommandSetUserSetting : BasicCommandExecution
    {
        
        private readonly PropertyInfo _property;
        
        /// <summary>
        /// The new value chosen by the user during command execution
        /// </summary>
        public object NewValue { get; private set; }
        
        /// <summary>
        /// True if the command was successfully completed 
        /// </summary>
        public bool Success { get; private set; }

        [UseWithObjectConstructor]
        public ExecuteCommandSetUserSetting(IBasicActivateItems activator,
            
            [DemandsInitialization("Name of a property you want to change e.g. AllowIdentifiableExtractions")]
            string property, 
            [DemandsInitialization("New value to assign, this will be parsed into a valid Type if property is not a string")]
            string value) : base(activator)
        {
            _property = typeof(UserSettings).GetProperty(property, BindingFlags.Public | BindingFlags.Static);

            if(_property == null)
            {
                SetImpossible($"Unknown Property '{property}'");

                //suggest similar sounding properties
                var suggestions =
                    typeof(UserSettings).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(c => CultureInfo.CurrentCulture.CompareInfo.IndexOf(c.Name,property, CompareOptions.IgnoreCase) >= 0).ToArray();

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

        
        public override void Execute()
        {
            base.Execute();

            if(_property == null)
                return;
                        
            ShareManager.SetValue(_property,NewValue,null);
            Success = true;
        }
    }
}
