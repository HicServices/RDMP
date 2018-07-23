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

        public SupportingDocumentsFetcher(Catalogue catalogue)
        {
            _catalogue = catalogue;
        }

        public SupportingDocumentsFetcher(SupportingDocument document)
        {
            _document = document;
            _catalogue = document.Repository.GetObjectByID<Catalogue>(document.Catalogue_ID);
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

        public void CheckSingle(ICheckNotifier notifier)
        {
            if (_document == null)
                notifier.OnCheckPerformed(new CheckEventArgs("SupportingDocument " + _document + " has not been set", CheckResult.Fail));
            else
            {
                try
                {
                    FileInfo toCopy = new FileInfo(_document.URL.LocalPath);
                    if (toCopy.Exists)
                        notifier.OnCheckPerformed(new CheckEventArgs("Found SupportingDocument " + toCopy.Name + " and it exists", CheckResult.Success));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("SupportingDocument " + _document + "(ID=" + _document.ID + ") " +
                                                                     "does not map to an existing file despite being flagged as Extractable", CheckResult.Fail));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not check supporting document " + _document, CheckResult.Fail, e));
                }   
            }
        }
    }
}
