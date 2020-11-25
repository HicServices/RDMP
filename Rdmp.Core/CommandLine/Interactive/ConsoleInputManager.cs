// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Implementation of <see cref="IBasicActivateItems"/> that handles object selection and message notification but is <see cref="IsInteractive"/>=false and throws <see cref="InputDisallowedException"/> on any attempt to illicit user feedback
    /// </summary>
    public class ConsoleInputManager : BasicActivateItems
    {
        /// <inheritdoc/>
        public override bool IsInteractive => !DisallowInput;

        /// <summary>
        /// Set to true to throw on any blocking input methods (e.g. <see cref="TypeText"/>)
        /// </summary>
        public bool DisallowInput { get; set; }
        
        /// <summary>
        /// Creates a new instance connected to the provided RDMP platform databases
        /// </summary>
        /// <param name="repositoryLocator">The databases to connect to</param>
        /// <param name="globalErrorCheckNotifier">The global error provider for non fatal issues</param>
        public ConsoleInputManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier):base(repositoryLocator,globalErrorCheckNotifier)
        {
        }
        public override void Show(string message)
        {
            Console.WriteLine(message);
        }

        public override bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{header}' ({prompt})");

            Console.WriteLine(header);
            Console.Write(prompt +":");
            text = ReadLine();
            return !string.IsNullOrWhiteSpace(text);
        }

        public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{taskDescription}'");

            Console.WriteLine(taskDescription);
            var value = ReadLine(new PickDatabase());
            return value.Database;
        }

        public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{taskDescription}'");

            Console.WriteLine(taskDescription);
            var value = ReadLine(new PickTable());
            return value.Table;
        }

        public override void ShowException(string errorText, Exception exception)
        {
            throw exception ?? new Exception(errorText);
        }
        
        public override bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            string chosenStr = GetString(prompt, Enum.GetNames(enumType).ToList());
            chosen = (Enum)Enum.Parse(enumType, chosenStr);
            return true;
        }

        public override bool SelectType(string prompt, Type[] available,out Type chosen)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'"); 

            string chosenStr = GetString(prompt, available.Select(t=>t.Name).ToList());

            if (string.IsNullOrWhiteSpace(chosenStr))
            {
                chosen = null;
                return false;
            }

            chosen = available.SingleOrDefault(t => t.Name.Equals(chosenStr));

            if(chosen == null)
                throw new Exception($"Unknown or incompatible Type '{chosen}'");

            return true;
        }


        public override IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt);

            var value = ReadLine(new PickObjectBase[]
                {new PickObjectByID(RepositoryLocator), new PickObjectByName(RepositoryLocator)},
                availableObjects.Select(t=>t.GetType().Name).Distinct());
            
            var unavailable = value.DatabaseEntities.Except(availableObjects).ToArray();

            if(unavailable.Any())
                throw new Exception("The following objects were not among the listed available objects " + string.Join(",",unavailable.Select(o=>o.ToString())));

            return value.DatabaseEntities.ToArray();
        }

        public override IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt);

            if (availableObjects.Length == 0)
                throw new Exception("No available objects found");

            //handle auto selection when there is one object
            if (availableObjects.Length == 1 && allowAutoSelect)
                return availableObjects[0];

            var value = ReadLine(new PickObjectBase[]
                {new PickObjectByID(RepositoryLocator), new PickObjectByName(RepositoryLocator)},
                availableObjects.Select(t=>t.GetType().Name).Distinct());

            var chosen = value.DatabaseEntities?.SingleOrDefault();

            if (chosen == null)
                return null;

            if(!availableObjects.Contains(chosen))
                throw new Exception("Picked object was not one of the listed available objects");

            return chosen;
        }

        private string ReadLine(IEnumerable<string> autoComplete = null)
        {
            if (DisallowInput)
                throw new InputDisallowedException("Value required");

            return autoComplete != null ? GetString("", autoComplete.ToList()) : Console.ReadLine();
        }

        private CommandLineObjectPickerArgumentValue ReadLine(PickObjectBase picker)
        {
            if (DisallowInput)
                throw new InputDisallowedException("Value required");

            Console.WriteLine($"Format: {picker.Format}");
            string line = ReadLine(picker.GetAutoCompleteIfAny());

            return picker.Parse(line, 0);
        }
        private CommandLineObjectPickerArgumentValue ReadLine(PickObjectBase[] pickers,IEnumerable<string> autoComplete)
        {
            if (DisallowInput)
                throw new InputDisallowedException("Value required");

            Console.WriteLine("Enter value in one of the following formats:");

            foreach (PickObjectBase p in pickers)
                Console.WriteLine($"Format: {p.Format}");
            
            string line = ReadLine(autoComplete);
            
            var picker = new CommandLineObjectPicker(new[]{line},RepositoryLocator,pickers);
            return picker[0];
        }

        public override DirectoryInfo SelectDirectory(string prompt)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt);
            return new DirectoryInfo(Console.ReadLine());
        }

        public override FileInfo SelectFile(string prompt)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            return SelectFile(prompt, null, null);
        }

        public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt);
            var file = Console.ReadLine();
            
            if(file != null)
                return new FileInfo(file);

            return null;
        }
        
        public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt);
            Console.WriteLine(@"Enter path with optional wildcards (e.g. c:\*.csv):");

            var file = Console.ReadLine();
            
            if(file != null)
            {
                var asterixIdx = file.IndexOf('*');

                if(asterixIdx != -1)
                {
                    int idxLastSlash = file.LastIndexOfAny(new []{Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar });

                    if(idxLastSlash == -1 || asterixIdx < idxLastSlash)
                        throw new Exception("Wildcards are only supported at the file level");

                    var searchPattern = file.Substring(idxLastSlash+1);
                    var dirStr = file.Substring(0,idxLastSlash);
                    
                    var dir = new DirectoryInfo(dirStr);

                    if(!dir.Exists)
                        throw new DirectoryNotFoundException("Could not find directory:" + dirStr);
                                        
                    return dir.GetFiles(searchPattern).ToArray();
                }
                else
                {
                    return new[]{new FileInfo(file) };
                }
            }

            return null;
        }
        

        protected override bool SelectValueTypeImpl(string prompt, Type paramType, object initialValue,out object chosen)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine("Enter value for " + prompt +":");
            chosen = UsefulStuff.ChangeType(ReadLine(), paramType);

            return true;
        }
        
        public override bool YesNo(string text, string caption, out bool chosen)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{text}'");

            Console.WriteLine(text + "(Y/n)");
            
            //if user picks no then it's false otherwise true
            if (string.Equals(Console.ReadLine()?.Trim(), "n", StringComparison.CurrentCultureIgnoreCase))
                chosen = false;
            else
                chosen = true;

            //user made a conscious decision
            return true;
        }
        
        public string GetString(string prompt, List<string> options)
        {
            if (DisallowInput)
                throw new InputDisallowedException($"Value required for '{prompt}'");

            Console.WriteLine(prompt +":");

            //This implementation does not play nice with linux
            if (EnvironmentInfo.IsLinux)
                return Console.ReadLine();
            
            var cyclingAutoComplete = new CyclingAutoComplete();
            while (true)
            {
                var result = ConsoleExt.ReadKey();
                switch (result.Key)
                {
                    case ConsoleKey.Enter:
                        var lowerLine = result.LineBeforeKeyPress.Line.ToLower();
                        if (lowerLine == "exit")
                            return "exit";
                        else
                        {
                            var match = options.FirstOrDefault(c => c.ToLower() == lowerLine);

                            if (match != null)
                                return match;

                            return result.LineBeforeKeyPress.Line;
                        }

                    case ConsoleKey.Tab:
                        var shiftPressed = (result.Modifiers & ConsoleModifiers.Shift) != 0;
                        var cyclingDirection = shiftPressed ? CyclingDirections.Backward : CyclingDirections.Forward;
                        var autoCompletedLine =
                            cyclingAutoComplete.AutoComplete(result.LineBeforeKeyPress.LineBeforeCursor,
                                options, cyclingDirection,true);
                        ConsoleExt.SetLine(autoCompletedLine);
                        break;
                }
            }
        }

        public override CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable, IProject project, string cohortInitialDescription)
        {
            throw new NotImplementedException();
        }
    }
}
