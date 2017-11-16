using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using CatalogueManager.Menus;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    public class UserInterfaceStandardisationChecker
    {
        private List<string> _csFilesList;
        private List<string> problems = new List<string>();

        public void FindProblems(List<string> csFilesList,MEF mef)
        {
            _csFilesList = csFilesList;
            List<Exception> whoCares;

            //All node classes should have equality compare members so that tree expansion works properly
            foreach (Type nodeClass in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => t.Name.EndsWith("Node") && !t.IsAbstract && !t.IsInterface))
                ConfirmFileHasText(nodeClass, "public override int GetHashCode()");


            //All Menus should correspond to a data class
            foreach (Type menuClass in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => typeof (RDMPContextMenuStrip).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
            {
                //We are looking at something like AutomationServerSlotsMenu
                if (!menuClass.Name.EndsWith("Menu"))
                {
                    problems.Add("Class '" + menuClass + "' is a RDMPContextMenuStrip but it's name doesn't end with Menu");
                    continue;
                }

                string expectedClassName = menuClass.Name.Substring(0,menuClass.Name.Length - "Menu".Length);
                if (!csFilesList.Any(f=>Path.GetFileName(f).Equals(expectedClassName +".cs")))
                {
                    problems.Add("Found menu called '" + menuClass +"' but couldn't find a corresponding data class called '" + expectedClassName +".cs'");
                    continue;
                }

                //expect something like this
                //public AutomationServerSlotsMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity)
                string expectedConstructorSignature = menuClass.Name + "(IActivateItems activator," + expectedClassName;
                ConfirmFileHasText(menuClass,expectedConstructorSignature);
            }
            
            
            foreach (string problem in problems)
                Console.WriteLine("FATAL ERROR PROBLEM:" + problem);

            Assert.AreEqual(problems.Count,0);
        }

        private void ConfirmFileHasText(Type type, string expectedString)
        {
            var file = _csFilesList.SingleOrDefault(f => Path.GetFileName(f).Equals(type.Name + ".cs"));

            //probably not our class
            if(file == null)
                return;

            if (!File.ReadAllText(file).Contains(expectedString))
                problems.Add("File '" + file + "' did not contain expected text '" + expectedString + "'");
        }
    }
}
