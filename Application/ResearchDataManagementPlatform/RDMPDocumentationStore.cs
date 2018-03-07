using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.Reports;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform
{
    /// <summary>
    /// Cache of all the summary xml help comments text in RDMP.  This is class basically provides automated on startup version of 
    /// DocumentationReportFormsAndControls (and DocumentationReportFormsAndControlsUI) in which summary comments are extracted from 
    /// SourceCodeForSelfAwareness.zip
    /// 
    /// Also populates the static KeywordHelpTextListbox.HelpKeywordsDictionary via AddToHelpDictionaryIfNotExists
    /// </summary>
    public class RDMPDocumentationStore
    {
        /// <summary>
        /// Source code for each class 
        /// </summary>
        public Dictionary<Type, string> TypeDocumentation { get; set; }

        public RDMPDocumentationStore(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            var ui = new DocumentationReportFormsAndControlsUI(null);
            ui.RepositoryLocator = repositoryLocator;
            var controlTypes = ui.GetAllFormsAndControlTypes();

            DocumentationReportFormsAndControls controlsDescriptions = new DocumentationReportFormsAndControls(controlTypes.ToArray());
            controlsDescriptions.Check(new IgnoreAllErrorsCheckNotifier());

            TypeDocumentation = controlsDescriptions.Summaries;

            foreach (KeyValuePair<Type, string> kvp in TypeDocumentation)
                KeywordHelpTextListbox.AddToHelpDictionaryIfNotExists(kvp.Key.Name, kvp.Value);
        }

        /// <summary>
        /// Returns documentation for the class specified up to maxLength characters (after which ... is appended).  Returns null if no documentation exists for the class
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeDocumentationIfExists(int maxLength, Type type)
        {
            if (!TypeDocumentation.ContainsKey(type))
                return null;

            maxLength = Math.Max(10, maxLength - 3);

            string documentation = TypeDocumentation[type];

            if (documentation.Length <= maxLength)
                return documentation;

            return documentation.Substring(0, maxLength) + "...";
        }
    }
}