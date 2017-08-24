using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Reports;
using CatalogueManager.SimpleDialogs.Reports;
using Dashboard.Raceway;
using DataExportManager.ProjectUI;
using DatasetLoaderUI;
using NUnit.Framework;
using RDMPStartup;
using ResearchDataManagementPlatform.WindowManagement;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class AllUIsDocumentedTest : DatabaseTests
    {
        [Test]
        public void AllUIControlsDocumented()
        {
            List<string> undocumented = new List<string>();

            Console.WriteLine("////////////////////Documentation of UI Controls////////////////");

            Assembly.Load(typeof (DatasetLoadControl).Assembly.FullName);
            Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
            Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
            Assembly.Load(typeof(ContentWindowManager).Assembly.FullName);

            DocumentationReportFormsAndControlsUI controlsFinding = new DocumentationReportFormsAndControlsUI(null);
            controlsFinding.RepositoryLocator = RepositoryLocator;
            var types = controlsFinding.GetAllFormsAndControlTypes();

            DocumentationReportFormsAndControls controlsDescriptions = new DocumentationReportFormsAndControls(types.ToArray());
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

    }
}