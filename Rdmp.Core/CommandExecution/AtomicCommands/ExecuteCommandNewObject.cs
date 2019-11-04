// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandNewObject:BasicCommandExecution
    {
        private Type _type;

        public ExecuteCommandNewObject(IBasicActivateItems activator,Type type):base(activator)
        {
            if(!typeof(DatabaseEntity).IsAssignableFrom(type))
                SetImpossible("Type must be derived from DatabaseEntity");
            _type = type;
        }

        public override void Execute()
        {
            base.Execute();

            var objectConstructor = new ObjectConstructor();

            var constructor = objectConstructor.GetRepositoryConstructor(_type);

            var invoker = new CommandInvoker(BasicActivator);

            var instance = objectConstructor.ConstructIfPossible(_type,
                constructor.GetParameters().Select(invoker.GetValueForParameterOfType).ToArray());

            if(instance == null)
                throw new Exception("Failed to construct object with provided parameters");

            Publish((DatabaseEntity) instance);
        }
    }
}
