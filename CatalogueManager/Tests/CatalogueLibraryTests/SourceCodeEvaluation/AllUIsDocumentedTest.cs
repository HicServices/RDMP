using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.Reports;
using Dashboard.Raceway;
using DataExportManager.ProjectUI;
using NUnit.Framework;
using ResearchDataManagementPlatform.WindowManagement;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.CommandExecution.Proposals;
using Tests.Common;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class AllUIsDocumentedTest : DatabaseTests
    {
        [Test]
        public void AllUIControlsDocumented()
        {
            CatalogueRepository.SuppressHelpLoading = false;
            CatalogueRepository.LoadHelp(TestContext.CurrentContext.WorkDirectory);
            CatalogueRepository.SuppressHelpLoading = true;


            List<string> undocumented = new List<string>();

            Console.WriteLine("////////////////////Documentation of UI Controls////////////////");

            Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
            Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
            Assembly.Load(typeof(ActivateItems).Assembly.FullName);

            DocumentationReportFormsAndControlsUI controlsFinding = new DocumentationReportFormsAndControlsUI(null);
            controlsFinding.RepositoryLocator = RepositoryLocator;
            var types = controlsFinding.GetAllFormsAndControlTypes();

            DocumentationReportFormsAndControls controlsDescriptions = new DocumentationReportFormsAndControls(CatalogueRepository.CommentStore,types.ToArray());
            controlsDescriptions.Check(new IgnoreAllErrorsCheckNotifier());

            
            foreach (var key in controlsDescriptions.Summaries.Keys.OrderBy(t=>t.ToString()))
            {
                var kvp = new KeyValuePair<Type,string>(key,controlsDescriptions.Summaries[key]);

                if(kvp.Value.Equals("Not documented",StringComparison.CurrentCultureIgnoreCase))
                {
                    undocumented.Add(kvp.Key.ToString());
                    Console.WriteLine(kvp.Key + " - Missing Documentation");
                }
                else
                    Console.WriteLine(kvp.Key);
    
            }
                
            Assert.AreEqual(0,undocumented.Count);
        }

        private int evaluatedClasses = 0;

        [Test]
        public void EveryClassInAppropriateNamespace()
        {
            List<string> Errors = new List<string>();

            Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
            Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
            Assembly.Load(typeof(ActivateItems).Assembly.FullName);

            //commands
            Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ICommandExecution),
                "CommandExecution",
                "CommandExecution.AtomicCommands",
                "CommandExecution.AtomicCommands.PluginCommands",
                "CommandExecution.AtomicCommands.WindowArranging"));//legal namespaces

            Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(IAtomicCommand), 
                "CommandExecution.AtomicCommands",
                "CommandExecution.AtomicCommands.PluginCommands",
                "CommandExecution.AtomicCommands.WindowArranging"));//legal namespaces

            //proposals
            Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ICommandExecutionProposal), "CommandExecution.Proposals"));

            //menus
            Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ContextMenuStrip), "Menus"));
            Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ToolStripMenuItem), "Menus.MenuItems"));
            
            foreach (string error in Errors)
                Console.WriteLine("FATAL NAMESPACE ERROR FAILURE:" + error);

            Assert.AreEqual(Errors.Count,0);
        }

        private string[] _exemptNamespaces = new string[]
        {
            "System.ComponentModel.Design",
            "System.Windows.Forms"
        };

        private IEnumerable<string> EnforceTypeBelongsInNamespace(Type InterfaceType, params string[] legalNamespaces)
        {
            List<Exception> whoCares;
            foreach (Type type in CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(InterfaceType.IsAssignableFrom))
            {
                if (type.Namespace == null) 
                    continue;

                //theese guys can be wherever they want
                if (_exemptNamespaces.Any(e => type.Namespace.Contains(e)))
                    continue;

                if (!legalNamespaces.Any(ns=>type.Namespace.Contains(ns)))
                    yield return "Expected Type '" + type.Name + "' to be in namespace(s) '" + string.Join("' or '",legalNamespaces) + "' but it was in '" + type.Namespace + "'";
                
                evaluatedClasses++;
            }

            Console.WriteLine("Evaluated " + evaluatedClasses + " classes for namespace compatibility");
        }
    }
}