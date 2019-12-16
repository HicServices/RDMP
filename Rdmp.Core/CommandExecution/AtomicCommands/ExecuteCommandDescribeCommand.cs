// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using System.Text;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Startup;
using ReusableLibraryCode.Comments;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribeCommand : BasicCommandExecution
    {
        private readonly Type _commandType;

        public ExecuteCommandDescribeCommand(IBasicActivateItems activator, 
            [DemandsInitialization("Command to describe",TypeOf = typeof(IAtomicCommand))]
            Type commandType):base(activator)
        {
            _commandType = commandType;
        }

        public override void Execute()
        {
            base.Execute();

            var invoker = new CommandInvoker(BasicActivator);

            var commandCtor = invoker.GetConstructor(_commandType);

            var help = new CommentStore();
            help.ReadComments(Environment.CurrentDirectory);

            var sb = new StringBuilder();

            if(commandCtor == null || !invoker.IsSupported(commandCtor))
                sb.AppendLine($"Command '{_commandType.Name}' is not supported by the current input type ({BasicActivator.GetType().Name})");
            else
            {
                sb.AppendLine("COMMAND:" + _commandType.FullName);
                
                var helpText = help.GetTypeDocumentationIfExists(_commandType);

                if(helpText != null)
                    sb.AppendLine(helpText);

                sb.AppendLine("USAGE:");
                
                sb.Append(EnvironmentInfo.IsLinux ? "./rdmp" : "./rdmp.exe");
                sb.Append(" cmd ");

                sb.Append(BasicCommandExecution.GetCommandName(_commandType.Name));
                sb.Append(" ");

                var sbParameters = new StringBuilder();
                sbParameters.AppendLine("PARAMETERS:");

                foreach(ParameterInfo p in commandCtor.GetParameters())
                {
                    var req = new RequiredArgument(p);

                    //automatic delegates require no user input or CLI entry (e.g. IActivateItems)                
                    if(invoker.GetDelegate(req).IsAuto)
                        continue;

                    sb.Append($"<{req.Name}> ");
                    sbParameters.AppendLine($"{req.Name}\t{req.Type.Name}\t{req.DemandIfAny?.Description}");
                }

                sb.AppendLine();
                sb.AppendLine(sbParameters.ToString());
            }
                

            BasicActivator.Show(sb.ToString());

        }
    }
}