// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation;

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

    public string ExtractToDirectory(DirectoryInfo directory) => _document != null
        ? ExtractToDirectory(directory, _document)
        : throw new Exception("SupportingDocument was not specified!");

    private static string ExtractToDirectory(DirectoryInfo directory, SupportingDocument supportingDocument)
    {
        if (!supportingDocument.IsReleasable())
            throw new Exception(
                $"Cannot extract SupportingDocument {supportingDocument} because it was not evaluated as IsReleasable()");

        var toCopy = supportingDocument.GetFileName();

        if (!Directory.Exists(Path.Combine(directory.FullName, "SupportingDocuments")))
            Directory.CreateDirectory(Path.Combine(directory.FullName, "SupportingDocuments"));

        if (!toCopy.Exists)
            throw new FileNotFoundException(
                $"Could not find supporting document '{supportingDocument}' which was expected to be at path:{toCopy.FullName}");

        //copy with overwrite
        File.Copy(toCopy.FullName, Path.Combine(directory.FullName, "SupportingDocuments", toCopy.Name), true);

        return Path.Combine(directory.FullName, "SupportingDocuments", toCopy.Name);
    }

    public void Check(ICheckNotifier notifier)
    {
        if (_singleDocument)
            CheckDocument(_document, notifier);
        else
            foreach (var supportingDocument in _catalogue.GetAllSupportingDocuments(FetchOptions
                         .ExtractableGlobalsAndLocals))
                CheckDocument(supportingDocument, notifier);
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
            var toCopy = document.GetFileName();

            if (toCopy is { Exists: true })
                notifier.OnCheckPerformed(
                    new CheckEventArgs($"Found SupportingDocument {toCopy.Name} and it exists",
                        CheckResult.Success));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"SupportingDocument {document}(ID={document.ID}) does not map to an existing file despite being flagged as Extractable.  Expected it to exist and be at '{toCopy}'",
                        CheckResult.Fail));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Could not check supporting documents of {_catalogue}",
                CheckResult.Fail, e));
        }
    }
}