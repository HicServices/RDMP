using System;
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
        private readonly SupportingDocument _document;
        private readonly Catalogue _catalogue;
        private readonly bool _singleDocument;

        public SupportingDocumentsFetcher(Catalogue catalogue)
        {
            _catalogue = catalogue;
        }

        public SupportingDocumentsFetcher(SupportingDocument document)
        {
            _document = document;
            _catalogue = document.Repository.GetObjectByID<Catalogue>(document.Catalogue_ID);
            _singleDocument = true;
        }

        public string ExtractToDirectory(DirectoryInfo directory)
        {
            if (_document != null)
                return ExtractToDirectory(directory, _document);

            throw new Exception("SupportingDocument was not specified!");
        }

        private string ExtractToDirectory(DirectoryInfo directory, SupportingDocument supportingDocument)
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
            if (_singleDocument)
                CheckDocument(_document, notifier);
            else
            {
                foreach (SupportingDocument supportingDocument in _catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableGlobalsAndLocals))
                    CheckDocument(supportingDocument, notifier);
            }
        }

        private void CheckDocument(SupportingDocument document, ICheckNotifier notifier)
        {
            if (document == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("SupportingDocument has not been set", CheckResult.Fail));
                return;
            }

            try
            {
                FileInfo toCopy = new FileInfo(document.URL.LocalPath);
                if (toCopy.Exists)
                    notifier.OnCheckPerformed(new CheckEventArgs("Found SupportingDocument " + toCopy.Name + " and it exists",
                        CheckResult.Success));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("SupportingDocument " + document + "(ID=" + document.ID +
                                                                 ") does not map to an existing file despite being flagged as Extractable", CheckResult.Fail));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not check supporting documents of " + _catalogue, CheckResult.Fail, e));
            }
        }
    }
}
