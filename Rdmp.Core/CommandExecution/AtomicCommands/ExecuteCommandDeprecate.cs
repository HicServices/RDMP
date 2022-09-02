// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Settings;
using System;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    public class ExecuteCommandDeprecate : BasicCommandExecution, IAtomicCommand
    {
        private readonly IMightBeDeprecated[] _o;
        private bool _desiredState;
      
        [UseWithObjectConstructor]
        public ExecuteCommandDeprecate(IBasicActivateItems itemActivator, 
            [DemandsInitialization("The object you want to deprecate/undeprecate")]
            IMightBeDeprecated[] o, 
            [DemandsInitialization("True to deprecate.  False to undeprecate",DefaultValue = true)]
            bool desiredState = true) : base(itemActivator)
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

            if(_o == null || _o.Length == 0)
                return;

            CommitInProgress commit = null;
            var revert = false;
            
            if(BasicActivator.UseCommits())
                commit = new CommitInProgress(BasicActivator.RepositoryLocator, new CommitInProgressSettings(_o)
                {
                    UseTransactions = true,
                    Description = GetDescription()
                });

            try
            {
                ExecuteImpl();

                // if user cancells transaction
                if(commit != null && commit.TryFinish(BasicActivator) == null)
                {
                    revert = true;
                }
            }
            finally
            {
                commit?.Dispose();
            }

            if(revert)
            {
                // go refresh ourselves to the db (uncommitted)
                foreach (var o in _o)
                {
                    o.RevertToDatabaseState();
                }
            }
            else
            {
                Publish((DatabaseEntity)_o[0]);
            }
        }

        private void ExecuteImpl()
        {
            foreach (var o in _o)
            {
                o.IsDeprecated = _desiredState;
                o.SaveToDatabase();
            }

            if (BasicActivator.IsInteractive && _o.Length == 1 && _o[0] is Catalogue)
            {
                if (_desiredState == true && BasicActivator.YesNo("Do you have a replacement Catalogue you want to link?", "Replacement"))
                {
                    var cmd = new ExecuteCommandReplacedBy(BasicActivator, _o[0], null)
                    {
                        PromptToPickReplacement = true
                    };
                    cmd.Execute();
                }
            }
        }

        private string GetDescription()
        {
            var verb = _desiredState ? "Deprecate" : "UnDeprecate";
            var noun = _o.Length == 1 ? _o[0].ToString() : _o.Length + " objects";

            return $"{verb} {noun}";
        }
    }
}