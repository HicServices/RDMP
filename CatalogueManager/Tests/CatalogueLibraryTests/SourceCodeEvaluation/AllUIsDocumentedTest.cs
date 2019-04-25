// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Reports;
using Rdmp.UI.ProjectUI;
using Rdmp.UI.Raceway;
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
            CatalogueRepository.CommentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
            
            List<string> undocumented = new List<string>();

            Console.WriteLine("////////////////////Documentation of UI Controls////////////////");

            Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
            Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
            Assembly.Load(typeof(ActivateItems).Assembly.FullName);

            List<Exception> ex;
            var types = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(
                t =>
                    (typeof(Form).IsAssignableFrom(t) || typeof(UserControl).IsAssignableFrom(t))
                    &&
                    !t.FullName.StartsWith("Microsoft")
                    &&
                    !t.FullName.StartsWith("System")
                    ).ToArray();


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
            "System.Windows.Forms",
            "ReusableUIComponents.ScintillaHelper"
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