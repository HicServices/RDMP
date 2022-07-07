// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribe:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable[] _databaseObjectToDescribe;
        private object _nonDatabaseObjectToDescribe;

        /// <summary>
        /// The help that the command will/did show
        /// </summary>
        public string HelpShown { get; private set; }

        [UseWithCommandLine(
            ParameterHelpList = "<command,type or object>", 
            ParameterHelpBreakdown = @"An object (or array of objects) to describe (e.g. Catalogue:bob) or Type name, or the name of a command")]
        public ExecuteCommandDescribe(IBasicActivateItems activator,CommandLineObjectPicker picker):base(activator)
        {
            if(picker.Length != 1)
            {
                SetImpossible($"Expected only a single parameter but there were {picker.Length}");
                return;
            }
            if(picker[0].HasValueOfType(typeof(IMapsDirectlyToDatabaseTable[])))
            {
                _databaseObjectToDescribe = (IMapsDirectlyToDatabaseTable[])picker[0].GetValueForParameterOfType(typeof(IMapsDirectlyToDatabaseTable[]));
            }
            else
            if (picker[0].HasValueOfType(typeof(Type)))
            {
                _nonDatabaseObjectToDescribe = picker[0].GetValueForParameterOfType(typeof(Type));
            }
            else
            {
                // Maybe they typed the alias or name of a command
                _nonDatabaseObjectToDescribe = new CommandInvoker(BasicActivator).GetSupportedCommands()
                .FirstOrDefault(t=>BasicCommandExecution.HasCommandNameOrAlias(t,picker[0].RawValue));
                
                    
                if(_nonDatabaseObjectToDescribe == null)
                    SetImpossible("Did not recognise parameter as a valid command");
            }
        }

        public ExecuteCommandDescribe(IBasicActivateItems activator, 
            IMapsDirectlyToDatabaseTable[] toDescribe):base(activator)
        {
            _databaseObjectToDescribe = toDescribe;
        }

        public ExecuteCommandDescribe(IBasicActivateItems basicActivator, object randomThing) : base(basicActivator)
        {
            _nonDatabaseObjectToDescribe = randomThing;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            if (_nonDatabaseObjectToDescribe != null)
                return iconProvider.GetImage(_nonDatabaseObjectToDescribe);

            return base.GetImage(iconProvider);
        }
        public override string GetCommandName()
        {
            if (_nonDatabaseObjectToDescribe != null)
            {
                return _nonDatabaseObjectToDescribe is Type t ? t.Name : _nonDatabaseObjectToDescribe.ToString();
            }

            return base.GetCommandName();
        }
        public override string GetCommandHelp()
        {
            if(_nonDatabaseObjectToDescribe != null)
            {
                string summary = BuildDescribe(_nonDatabaseObjectToDescribe, out string title);

                return title + Environment.NewLine + summary;
            }

            return base.GetCommandHelp();
        }
        public override void Execute()
        {
            base.Execute();

            if (_nonDatabaseObjectToDescribe is Type t && typeof(ICommandExecution).IsAssignableFrom(t))
            {
                DisplayCommandHelp(t);
            }
            else
            if(_nonDatabaseObjectToDescribe != null)
            {
                var description = BuildDescribe(_nonDatabaseObjectToDescribe,out string title);
                Show(title, description);
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (IMapsDirectlyToDatabaseTable o in _databaseObjectToDescribe)
                {
                    BuildDescribe(o, sb);
                }

                if (sb.Length > 0)
                    Show(HelpShown = sb.ToString());
            }

        }

        public static string Describe(IMapsDirectlyToDatabaseTable o)
        {
            var sb = new StringBuilder();
            BuildDescribe(o,sb);
            return sb.ToString();
        }

        private static void BuildDescribe(IMapsDirectlyToDatabaseTable o, StringBuilder sb)
        {

            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                // don't describe helper properties
                if(p.GetCustomAttributes(typeof(NoMappingToDatabase)).Any())
                {
                    continue;
                }

                sb.Append(p.Name);
                sb.Append(":");
                sb.AppendLine(p.GetValue(o)?.ToString() ?? "NULL");
            }

            sb.AppendLine("-----------------------------------------");
        }
        private string BuildDescribe(object o,out string title)
        {
            // if its a Type tell them about the Type
            if (o is Type t)
            {
                title = t.Name;
                var help = BasicActivator.CommentStore ?? CreateCommentStore();
                var docs = help.GetTypeDocumentationIfExists(t, true, true);
                return docs?.Trim() ?? "";
            }

            // its an actual object so give them a summary of it
            title = _nonDatabaseObjectToDescribe.GetType().Name;
            return o is ICanBeSummarised s ? s.GetSummary(true, true) : o.ToString();
        }

        public void DisplayCommandHelp(Type commandType)
        {
            var invoker = new CommandInvoker(BasicActivator);

            var commandCtor = invoker.GetConstructor(commandType, new CommandLineObjectPicker(new string[0],BasicActivator));

            var sb = new StringBuilder();

            PopulateBasicCommandInfo(sb,commandType);

            var dynamicCtorAttribute = commandCtor?.GetCustomAttribute<UseWithCommandLineAttribute>();
            
            //it is a basic command, one that expects a fixed number of proper objects
            var sbParameters = new StringBuilder();
            sbParameters.AppendLine();
            sbParameters.AppendLine("PARAMETERS:");

            var sbSyntaxes = new StringBuilder();
            sbSyntaxes.AppendLine();
            sbSyntaxes.AppendLine("The following syntaxes may be used for parameters in this query:");
            bool anySyntaxes = false;

            // Usage
            if (dynamicCtorAttribute != null)
            {
                //is it a dynamic command (one that processes its own CommandLinePicker)

                // Added to the call line e.g. "./rdmp cmd MyCall <param1> <someotherParam>"
                sb.Append(dynamicCtorAttribute.ParameterHelpList);
                sbParameters.Append(dynamicCtorAttribute.ParameterHelpBreakdown);
            }
            else
            if (commandCtor == null || !invoker.IsSupported(commandCtor))
            {
                sb.AppendLine($"Command '{commandType.Name}' is not supported by the current input type ({BasicActivator.GetType().Name})");

                if (commandCtor != null)
                {
                    var unsupported = commandCtor.GetParameters().Where(p => !invoker.IsSupported(p)).ToArray();

                    if (unsupported.Any())
                        sb.AppendLine("The following parameter types (required by the command's constructor) were not supported:" + Environment.NewLine + string.Join(Environment.NewLine, unsupported.Select(p => $"{p.Name}({p.ParameterType})")));
                }
            }
            else
            {
                // For each thing the constructor takes
                var parameters = commandCtor.GetParameters();

                foreach (ParameterInfo p in parameters)
                {
                    var req = new RequiredArgument(p);

                    //automatic delegates require no user input or CLI entry (e.g. IActivateItems)                
                    if (invoker.GetDelegate(req).IsAuto)
                        continue;

                    // Added to the call line e.g. "./rdmp cmd MyCall <param1> <someotherParam>"
                    sb.Append($"<{req.Name}> ");

                    //document it for the breakdown table
                    sbParameters.AppendLine(FormatParameterDescription(req, commandCtor));
                }

                anySyntaxes = ShowSyntax("Pick Database",sbSyntaxes, parameters ,(p) => typeof(DiscoveredDatabase).IsAssignableFrom(p.ParameterType), new PickDatabase()) || anySyntaxes;
                anySyntaxes = ShowSyntax("Pick Table",sbSyntaxes, parameters, (p) => typeof(DiscoveredTable).IsAssignableFrom(p.ParameterType), new PickTable()) || anySyntaxes;

                anySyntaxes = ShowSyntax("Pick RDMP Object",sbSyntaxes, parameters, (p) => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(p.ParameterType) || anySyntaxes,
                    new PickObjectByID(BasicActivator),
                    new PickObjectByName(BasicActivator),
                    new PickObjectByQuery(BasicActivator)) || anySyntaxes;

            };


            sb.AppendLine();
            sb.AppendLine(sbParameters.ToString());            

            if(anySyntaxes)
                sb.AppendLine(sbSyntaxes.ToString());

            BasicActivator.Show(HelpShown = sb.ToString());

        }

        private bool ShowSyntax(string title, StringBuilder sbSyntaxes, ParameterInfo[] parameters, Func<ParameterInfo, bool> selector, params PickObjectBase[] pickers)
        {
            if(parameters.Any(selector))
            {
                sbSyntaxes.AppendLine(Environment.NewLine + title + ":");
                sbSyntaxes.AppendLine("Formats:");
                foreach (var p in pickers)
                    sbSyntaxes.AppendLine(p.Format);

                sbSyntaxes.AppendLine("Examples:");
                foreach (var p in pickers)
                    foreach(var e in p.Examples)
                        sbSyntaxes.AppendLine(e);

                return true;
            }

            return false;
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

        private void PopulateBasicCommandInfo(StringBuilder sb,Type commandType)
        {
            var help = BasicActivator.CommentStore ?? CreateCommentStore();

            // Basic info about command
            sb.AppendLine("Name: " + commandType.Name);


            var aliases = commandType.GetCustomAttributes(false).OfType<AliasAttribute>().ToArray();
            if(aliases.Any())
            {
                sb.AppendLine("Aliases:" + string.Join(',',aliases.Select(a=>a.Name).ToArray()));
            }
                
            var helpText = help.GetTypeDocumentationIfExists(commandType);

            if(helpText != null)
            {
                sb.AppendLine();
                sb.AppendLine("Description: " + Environment.NewLine + helpText);
            }

            sb.AppendLine();
            sb.AppendLine("USAGE: ");
                
            sb.Append(EnvironmentInfo.IsLinux ? "./rdmp" : "./rdmp.exe");
            sb.Append(" ");

            sb.Append(BasicCommandExecution.GetCommandName(commandType.Name));
            sb.Append(" ");

        }

    }
}
