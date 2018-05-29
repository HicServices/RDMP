using System;
using System.Collections.Generic;
using System.IO;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary
{
    /// <summary>
    /// Copies SupportingDocuments associated with a project extraction request to the output directory.
    /// </summary>
    public class SupportingDocumentsFetcher
    {
        private readonly Catalogue _catalogue;

        public SupportingDocumentsFetcher(Catalogue catalogue)
        {
            _catalogue = catalogue;
        }

        public List<Exception> TryExtractAllToDirectory(DirectoryInfo directory,FetchOptions fetch)
        {
            List<Exception> thingsThatFailed = new List<Exception>();

            foreach (SupportingDocument supportingDocument in _catalogue.GetAllSupportingDocuments(fetch))
            {
                try
                {
                    if (!supportingDocument.Extractable)
                        continue;

                    ExtractToDirectory(directory,supportingDocument);
                }
                catch (Exception e)
                {

                    thingsThatFailed.Add(e);
                }
            }

            return thingsThatFailed;
        }

        public string ExtractToDirectory(DirectoryInfo directory, SupportingDocument supportingDocument)
        {
            if(!supportingDocument.IsReleasable())
                throw new Exception("Cannot extract SupportingDocument " + supportingDocument + " because it was not evaluated as IsReleasable()");
            
            FileInfo toCopy = new FileInfo(supportingDocument.URL.LocalPath);

            if (!Directory.Exists(Path.Combine(directory.FullName, "SupportingDocuments")))
                Directory.CreateDirectory(Path.Combine(directory.FullName, "SupportingDocuments"));

            //copy with overwritte
            File.Copy(toCopy.FullName, Path.Combine(directory.FullName, "SupportingDocuments", toCopy.Name), true);
            
            return Path.Combine(directory.FullName, "SupportingDocuments", toCopy.Name);
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (SupportingDocument supportingDocument in _catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableGlobalsAndLocals))
            {
                try
                {
                    FileInfo toCopy = new FileInfo(supportingDocument.URL.LocalPath);
                    if (toCopy.Exists)
                        notifier.OnCheckPerformed(new CheckEventArgs("Found SupportingDocument " + toCopy.Name + " and it exists",CheckResult.Success));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("SupportingDocument " + supportingDocument + "(ID=" + supportingDocument .ID+ ") does not map to an existing file despite being flagged as Extractable", CheckResult.Fail));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not check supporting documents of " + _catalogue, CheckResult.Fail, e));
                }
            }
        }
    }
}
