// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueLibrary.Nodes.UsedByNodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.DashboardTabs;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Dashboard.CatalogueSummary.DataQualityReporting;
using Dashboard.CatalogueSummary.LoadEvents;
using Dashboard.Overview;
using NUnit.Framework;
using ResearchDataManagementPlatform;
using ReusableUIComponents.CommandExecution.Proposals;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    public class UserInterfaceStandardisationChecker
    {
        private List<string> _csFilesList;
        private List<string> problems = new List<string>();

        private Type[] excusedNodeClasses = new Type[]
        {
            //it's a singleton because you can only have one decryption certificate for an RDMP as opposed to other SingletonNode classses that represent collections e.g. AllTableInfos is the only collection of TableInfos but it's a collection
            typeof(DecryptionPrivateKeyNode),
            typeof(ArbitraryFolderNode),
            
            //excused because although singletons they have dynamic names / they are basically a collection
            typeof(OtherPipelinesNode),
            typeof(StandardPipelineUseCaseNode)
        };


        /// <summary>
        /// UI classes that are allowed not to end with the suffix UI
        /// </summary>
        private Type[] excusedUIClasses = new[]
        {
            typeof (RDMPUserControl),
            typeof (RDMPForm),
            typeof(RDMPSingleDatabaseObjectControl<>),
            typeof(DashboardableControlHostPanel),
            typeof(TimePeriodicityChart),
            typeof(LoadEventsTreeView),
            
            typeof(ResolveFatalErrors),
            typeof(DataLoadsGraph),
            typeof(RDMPMainForm)

        };

        public void FindProblems(List<string> csFilesList,MEF mef)
        {
            _csFilesList = csFilesList;
            List<Exception> whoCares;

            //All node classes should have equality compare members so that tree expansion works properly
            foreach (Type nodeClass in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => t.Name.EndsWith("Node") && !t.IsAbstract && !t.IsInterface))
            {
                //class is excused
                if (excusedNodeClasses.Contains(nodeClass))
                    continue;

                //it's something like ProposeExecutionWhenTargetIsIDirectoryNode.cs i.e. it's not a Node!
                if (typeof (ICommandExecutionProposal).IsAssignableFrom(nodeClass))
                    continue;

                //if it's an ObjectUsedByOtherObjectNode then it will already have GetHashCode implemented
                if (typeof (IObjectUsedByOtherObjectNode).IsAssignableFrom(nodeClass))
                    continue;

                //these are all supported at base class level
                if (typeof (SingletonNode).IsAssignableFrom(nodeClass))
                {

                    if(!nodeClass.Name.StartsWith("All"))
                        problems.Add("Class '" + nodeClass.Name+ "' is a SingletonNode but it's name doesn't start with All");
                    
                    continue;
                }

                ConfirmFileHasText(nodeClass, "public override int GetHashCode()");
            }

            //All Menus should correspond to a data class
            foreach (Type menuClass in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => typeof (RDMPContextMenuStrip).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
            {
                if(menuClass == typeof(RDMPContextMenuStrip)) //the basic class from which all are inherited
                    continue;
                
                //We are looking at something like AutomationServerSlotsMenu
                if (!menuClass.Name.EndsWith("Menu"))
                {
                    problems.Add("Class '" + menuClass + "' is a RDMPContextMenuStrip but it's name doesn't end with Menu");
                    continue;
                }

                foreach (ConstructorInfo c in menuClass.GetConstructors())
                {
                    if(c.GetParameters().Count() != 2)
                        problems.Add("Constructor of class '" + menuClass + "' which is an RDMPContextMenuStrip contained " + c.GetParameters().Count() + " constructor arguments.  These menus are driven by reflection (See RDMPCollectionCommonFunctionality.GetMenuWithCompatibleConstructorIfExists )");
                }


                var toLookFor = menuClass.Name.Substring(0, menuClass.Name.Length - "Menu".Length);
                var expectedClassName = GetExpectedClassOrInterface(toLookFor);

                if(expectedClassName == null)
                {
                    problems.Add("Found menu called '" + menuClass.Name + "' but couldn't find a corresponding data class called '" + toLookFor + ".cs'");
                    continue;
                }

                ConfirmFileHasText(menuClass, "AddCommonMenuItems()",false);

                //expect something like this
                //public AutomationServerSlotsMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity)
                string expectedConstructorSignature = menuClass.Name + "(RDMPContextMenuStripArgs args," + expectedClassName;
                ConfirmFileHasText(menuClass,expectedConstructorSignature);
                
                FieldInfo[] fields = menuClass.GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

                //find private fields declared at the object level (i.e. not in base class that are of type IActivateItem)
                var activatorField = fields.FirstOrDefault(f =>f.DeclaringType == menuClass &&  f.FieldType == typeof (IActivateItems));
                if(activatorField != null)
                    problems.Add("Menu '" + menuClass + "' contains a private field called '" + activatorField.Name + "'.  You should instead use base class protected field RDMPContextMenuStrip._activator");
            }
            
            //Drag and drop / Activation - Execution Proposal system
            foreach (Type proposalClass in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => typeof(ICommandExecutionProposal).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
            {
                //We are looking at something like AutomationServerSlotsMenu
                if (!proposalClass.Name.StartsWith("ProposeExecutionWhenTargetIs"))
                {
                    problems.Add("Class '" + proposalClass + "' is a ICommandExecutionProposal but it's name doesn't start with ProposeExecutionWhenTargetIs");
                    continue;
                }

                var toLookFor = proposalClass.Name.Substring("ProposeExecutionWhenTargetIs".Length);
                string expectedClassName = GetExpectedClassOrInterface(toLookFor);

                if (expectedClassName == null)
                    problems.Add("Found proposal called '" + proposalClass + "' but couldn't find a corresponding data class called '" + toLookFor + ".cs'");
            }
            
            //Make sure all user interface classes have the suffix UI
            foreach(Type uiType in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => 
                 (typeof(RDMPUserControl).IsAssignableFrom(t)||(typeof(RDMPForm).IsAssignableFrom(t))
                 && !t.IsAbstract && !t.IsInterface)))
            {
                
                if(!uiType.Name.EndsWith("UI") && !uiType.Name.EndsWith("_Design"))
                {
                    if (excusedUIClasses.Contains(uiType))
                        continue;
                    
                    //also allow Screen1, Screen2 etc
                    if(Regex.IsMatch(uiType.Name,@"Screen\d") && uiType.IsNotPublic)
                        continue;

                    problems.Add("Class " + uiType.Name + " does not end with UI");
                }
            }


            foreach (string problem in problems)
                Console.WriteLine("FATAL ERROR PROBLEM:" + problem);

            Assert.AreEqual(problems.Count,0);
        }

        private string GetExpectedClassOrInterface(string expectedClassName)
        {
            //found it?
            if (_csFilesList.Any(f => Path.GetFileName(f).Equals(expectedClassName + ".cs", StringComparison.InvariantCultureIgnoreCase)))
                return expectedClassName;

            //expected Filter but found IFilter - acceptable
            if (_csFilesList.Any(f => Path.GetFileName(f).Equals("I" + expectedClassName + ".cs", StringComparison.InvariantCultureIgnoreCase)))
                return "I" + expectedClassName;

            return null;
        }

        private void ConfirmFileHasText(Type type, string expectedString,bool mustHaveText = true)
        {
            var file = _csFilesList.SingleOrDefault(f => Path.GetFileName(f).Equals(type.Name + ".cs"));

            //probably not our class
            if(file == null)
                return;
            bool hasText = File.ReadAllText(file)
                .Replace(" ", "")
                .ToLowerInvariant()
                .Contains(expectedString.Replace(" ", "").ToLowerInvariant());

            if (mustHaveText)
            {
                if(!hasText)
                    problems.Add("File '" + file + "' did not contain expected text '" + expectedString + "'");
            }
            else
            {
                if(hasText)
                    problems.Add("File '" + file + "' contains unexpected text '" + expectedString + "'");
            }
            
        }
    }
}

