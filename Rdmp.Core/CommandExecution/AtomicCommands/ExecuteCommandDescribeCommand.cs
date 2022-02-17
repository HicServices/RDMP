// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup;
using ReusableLibraryCode.Comments;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribeCommand : BasicCommandExecution
    {
        private readonly Type _commandType;
        
        /// <summary>
        /// The help that the command will/did show
        /// </summary>
        public string HelpShown { get; private set; }

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

            var commandCtor = invoker.GetConstructor(_commandType, new CommandLineObjectPicker(new string[0],BasicActivator));

            var sb = new StringBuilder();

            PopulateBasicCommandInfo(sb);

            var dynamicCtorAttribute = commandCtor?.GetCustomAttribute<UseWithCommandLineAttribute>();
            
            //it is a basic command, one that expects a fixed number of proper objects
            var sbParameters = new StringBuilder();
            sbParameters.AppendLine();
            sbParameters.AppendLine("PARAMETERS:");

            // Usage
            if(dynamicCtorAttribute != null)
            {
                //is it a dynamic command (one that processes it's own CommandLinePicker)
                
                // Added to the call line e.g. "./rdmp cmd MyCall <param1> <someotherParam>"
                sb.Append(dynamicCtorAttribute.ParameterHelpList);
                sbParameters.Append(dynamicCtorAttribute.ParameterHelpBreakdown);
            }
            else
            if(commandCtor == null || !invoker.IsSupported(commandCtor))
            {
                sb.AppendLine($"Command '{_commandType.Name}' is not supported by the current input type ({BasicActivator.GetType().Name})");
                
                if(commandCtor != null)
                {
                    var unsupported = commandCtor.GetParameters().Where(p=>!invoker.IsSupported(p)).ToArray();

                    if(unsupported.Any())
                        sb.AppendLine("The following parameter types (required by the command's constructor) were not supported:" + Environment.NewLine + string.Join(Environment.NewLine,unsupported.Select(p=> $"{p.Name }({p.ParameterType})")));
                }
                
            }
                
            else
            {
                // For each thing the constructor takes
                foreach(ParameterInfo p in commandCtor.GetParameters())
                {
                    var req = new RequiredArgument(p);

                    //automatic delegates require no user input or CLI entry (e.g. IActivateItems)                
                    if(invoker.GetDelegate(req).IsAuto)
                        continue;

                    // Added to the call line e.g. "./rdmp cmd MyCall <param1> <someotherParam>"
                    sb.Append($"<{req.Name}> ");

                    //document it for the breakdown table
                    sbParameters.AppendLine(FormatParameterDescription(req,commandCtor));
                }

            }

            sb.AppendLine();
            sb.AppendLine(sbParameters.ToString());
                
            BasicActivator.Show(HelpShown = sb.ToString());

        }

        private string FormatParameterDescription(RequiredArgument req,ConstructorInfo ctor)
        {
            int nameColWidth = ctor.GetParameters().Max(p=>p.Name.Length);
            int typeColWidth = ctor.GetParameters().Max(p=>p.ParameterType.Name.Length);

            try
            {
                if(BasicActivator is ConsoleInputManager)
                {              
                    var name = req.Name.Length < nameColWidth ? req.Name.PadRight(nameColWidth) : req.Name.Substring(0,nameColWidth);
                    var type = req.Type.Name.Length < typeColWidth ? req.Type.Name.PadRight(typeColWidth) : req.Type.Name.Substring(0,typeColWidth);
                    
                    var desc = req.DemandIfAny?.Description;

                    if(string.IsNullOrWhiteSpace(desc))
                        return $"{name} {type}";
                    else
                    {
                        var availableWidth = Console.WindowWidth;
                        var occupied = nameColWidth + 1 + typeColWidth + 1;

                        var availableDescriptionWidth = availableWidth - occupied;

                        if(availableDescriptionWidth < 0)
                            return $"{name} {type}";

                        var wrappedDesc = Wrap(desc,availableDescriptionWidth,occupied);

                        return $"{name} {type} {wrappedDesc}";
                    }

                }

            }
            catch (Exception)
            {
                return $"{req.Name}\t{req.Type.Name}\t{req.DemandIfAny?.Description}";
            }

            return $"{req.Name}\t{req.Type.Name}\t{req.DemandIfAny?.Description}";
        }
        
        private string Wrap(string longString, int width, int indent)
        {
            string[] words = longString.Split(' ');

            StringBuilder newSentence = new StringBuilder(longString.Length);

            StringBuilder line = new StringBuilder(width);
            foreach (string word in words)
            {
                if ((line.Length + word.Length) >= width)
                {
                    newSentence.AppendLine(line.ToString().TrimEnd());
                    newSentence.Append(new string (' ',indent));
                    line.Clear();
                }

                line.Append(word);
                line.Append(" ");
            }

            if (line.Length > 0)
            {
                newSentence.AppendLine(line.ToString().TrimEnd());
                newSentence.Append(new string (' ',indent));
            }

            return newSentence.ToString().Trim();
        }

        private void PopulateBasicCommandInfo(StringBuilder sb)
        {
            var help = new CommentStore();
            help.ReadComments(Environment.CurrentDirectory);

            // Basic info about command
            sb.AppendLine("Name: " + _commandType.Name);
                
            var helpText = help.GetTypeDocumentationIfExists(_commandType);

            if(helpText != null)
            {
                sb.AppendLine();
                sb.AppendLine("Description: " + Environment.NewLine + helpText);
            }

            sb.AppendLine();
            sb.AppendLine("USAGE: ");
                
            sb.Append(EnvironmentInfo.IsLinux ? "./rdmp" : "./rdmp.exe");
            sb.Append(" cmd ");

            sb.Append(BasicCommandExecution.GetCommandName(_commandType.Name));
            sb.Append(" ");

        }
    }
}