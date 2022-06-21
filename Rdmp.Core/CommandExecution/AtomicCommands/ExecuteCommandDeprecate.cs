// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    public class ExecuteCommandDeprecate : BasicCommandExecution, IAtomicCommand
    {
        private readonly IMightBeDeprecated _o;
        private bool _desiredState;

        public ExecuteCommandDeprecate(IBasicActivateItems itemActivator, IMightBeDeprecated o) : this(itemActivator, o, true)
        {

        }

        public ExecuteCommandDeprecate(IBasicActivateItems itemActivator, IMightBeDeprecated o, bool desiredState) : base(itemActivator)
        {
            _o = o;
            _desiredState = desiredState;
        }

        public override string GetCommandName()
        {
            if (!string.IsNullOrEmpty(OverrideCommandName))
                return OverrideCommandName;

            return _desiredState ? "Deprecate" : "Undeprecate";
        }

        public override void Execute()
        {
            base.Execute();

            if(_o == null)
                return;

            _o.IsDeprecated = _desiredState;
            _o.SaveToDatabase();

            if(BasicActivator.IsInteractive && _o is Catalogue)
            {
                if(_desiredState == true && BasicActivator.YesNo("Do you have a replacement Catalogue you want to link?","Replacement"))
                {
                    var cmd = new ExecuteCommandReplacedBy(BasicActivator,_o,null)
                        {PromptToPickReplacement = true};
                    cmd.Execute();
                }
            }

            Publish((DatabaseEntity)_o);
        }
    }
}