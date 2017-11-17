using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Menus;
using NUnit.Framework;
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
            typeof(DecryptionPrivateKeyNode)
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
                //We are looking at something like AutomationServerSlotsMenu
                if (!menuClass.Name.EndsWith("Menu"))
                {
                    problems.Add("Class '" + menuClass + "' is a RDMPContextMenuStrip but it's name doesn't end with Menu");
                    continue;
                }

                var toLookFor = menuClass.Name.Substring(0, menuClass.Name.Length - "Menu".Length);
                var expectedClassName = GetExpectedClassOrInterface(toLookFor);

                if(expectedClassName == null)
                {
                    problems.Add("Found menu called '" + menuClass.Name + "' but couldn't find a corresponding data class called '" + toLookFor + ".cs'");
                    continue;
                }

                //expect something like this
                //public AutomationServerSlotsMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity)
                string expectedConstructorSignature = menuClass.Name + "(IActivateItems activator," + expectedClassName;
                ConfirmFileHasText(menuClass,expectedConstructorSignature);
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

        private void ConfirmFileHasText(Type type, string expectedString)
        {
            var file = _csFilesList.SingleOrDefault(f => Path.GetFileName(f).Equals(type.Name + ".cs"));

            //probably not our class
            if(file == null)
                return;

            if (!File.ReadAllText(file).Replace(" ","").ToLowerInvariant().Contains(expectedString.Replace(" ","").ToLowerInvariant()))
                problems.Add("File '" + file + "' did not contain expected text '" + expectedString + "'");
        }
    }
}
