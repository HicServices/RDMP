using System;
using CatalogueLibrary.Repositories;

namespace ResearchDataManagementPlatform
{
    /// <summary>
    /// Cache of all the summary xml help comments text in RDMP.  This is class basically provides automated on startup version of 
    /// DocumentationReportFormsAndControls (and DocumentationReportFormsAndControlsUI) in which summary comments are extracted from 
    /// SourceCodeForSelfAwareness.zip
    /// 
    /// <para>Also populates the static KeywordHelpTextListbox.HelpKeywordsDictionary via AddToHelpDictionaryIfNotExists</para>
    /// </summary>
    public class RDMPDocumentationStore
    {
        private CatalogueRepository _catalogueRepository;

        public RDMPDocumentationStore(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _catalogueRepository = repositoryLocator.CatalogueRepository;
        }

        /// <summary>
        /// Returns documentation for the class specified up to maxLength characters (after which ... is appended).  Returns null if no documentation exists for the class
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeDocumentationIfExists(int maxLength, Type type)
        {
            if (!_catalogueRepository.CommentStore.ContainsKey(type.Name))
                return null;

            maxLength = Math.Max(10, maxLength - 3);

            string documentation = _catalogueRepository.CommentStore[type.Name];

            if (documentation.Length <= maxLength)
                return documentation;

            return documentation.Substring(0, maxLength) + "...";
        }
    }
}
