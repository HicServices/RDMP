// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Creates or Updates an <see cref="ExtendedProperty"/> declaration on any RDMP object.
    /// </summary>
    public class ExecuteCommandSetExtendedProperty : BasicCommandExecution, IAtomicCommand
    {
        public IMapsDirectlyToDatabaseTable SetOn { get; }
        public string PropertyName { get; }
        public object Value { get; }
        public bool Strict { get; }

        [UseWithObjectConstructor]
        public ExecuteCommandSetExtendedProperty(IBasicActivateItems activator,
            [DemandsInitialization("The object that you want to set the property on")]
            IMapsDirectlyToDatabaseTable setOn,
            [DemandsInitialization("The property you want to set")]
            string propertyName,
            [DemandsInitialization("The value to store")]
            string value,
            [DemandsInitialization("True to validate propertyName against known properties.  False to allow custom named properties.  Defaults to true.",DefaultValue = true)]
            bool strict = true
            )
            : base(activator)
        {
            if (strict && !ExtendedProperty.KnownProperties.Contains(propertyName))
            {
                SetImpossible($"{propertyName} is not a known property.  Known properties are: {Environment.NewLine}{string.Join(Environment.NewLine,ExtendedProperty.KnownProperties)}");
            }
            SetOn = setOn;
            PropertyName = propertyName;
            Value = value;
            Strict = strict;
        }

        public override string GetCommandName()
        {
            if (!string.IsNullOrWhiteSpace(OverrideCommandName))
                return OverrideCommandName;

            return $"Set {PropertyName}";
        }

        public override void Execute()
        {
            base.Execute();

            var cataRepo = BasicActivator.RepositoryLocator.CatalogueRepository;
            
            // delete any old versions
            foreach(var d in cataRepo.GetExtendedProperties(PropertyName, SetOn))
            {
                d.DeleteInDatabase();
            }
            
            // Creates the new property into the db
            var prop = new ExtendedProperty(cataRepo, SetOn, PropertyName, Value);
            Publish(prop);
        }
    }
}