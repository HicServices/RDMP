// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes a document (e.g. PDF / Excel file etc) which is useful for understanding a given dataset (Catalogue).
///     This can be marked as Extractable in which case
///     every time the dataset is extracted the file will also be bundled along with it (so that researchers can also
///     benefit from the file).
///     <para>
///         You can also mark SupportingDocuments as Global in which case they will be provided (if Extractable) to
///         researchers regardless of which datasets they have selected
///         e.g. a PDF on data governance or a copy of an empty 'data use contract document'
///     </para>
///     <para>Finally you can tie in the Ticketing system so that you can audit time spent curating the document etc.</para>
/// </summary>
public class SupportingDocument : DatabaseEntity, INamed, ISupportingObject
{
    #region Database Properties

    private string _name;
    private Uri _uRL;
    private string _description;
    private bool _extractable;
    private string _ticket;
    private bool _isGlobal;
    private int _catalogueID;

    /// <inheritdoc />
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Path to the document on disk
    /// </summary>
    [AdjustableLocation]
    public Uri URL
    {
        get => _uRL;
        set => SetField(ref _uRL, value);
    }

    /// <summary>
    ///     Human readable description of what the document contains and why it is in the system
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///     If true then the file will be copied to the output directory of project extractions that include the
    ///     <see cref="Catalogue_ID" />.
    /// </summary>
    public bool Extractable
    {
        get => _extractable;
        set => SetField(ref _extractable, value);
    }

    /// <summary>
    ///     <see cref="ITicketingSystem" /> identifier of a ticket for logging time curating / updating etc the document
    /// </summary>
    public string Ticket
    {
        get => _ticket;
        set => SetField(ref _ticket, value);
    }

    /// <summary>
    ///     If <see cref="Extractable" />  and <see cref="IsGlobal" /> then the document will be copied to the ouptut directory
    ///     of all project extractions
    ///     regardless of whether or not the <see cref="Catalogue_ID" /> dataset is included in the extraction
    /// </summary>
    public bool IsGlobal
    {
        get => _isGlobal;
        set => SetField(ref _isGlobal, value);
    }

    /// <summary>
    ///     The dataset the document relates to
    /// </summary>
    public int Catalogue_ID
    {
        get => _catalogueID;
        set => SetField(ref _catalogueID, value);
    }

    #endregion

    public SupportingDocument()
    {
    }

    /// <summary>
    ///     Creates a new supporting document for helping understand the dataset <paramref name="parent" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    public SupportingDocument(ICatalogueRepository repository, ICatalogue parent, string name)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "Catalogue_ID", parent.ID }
        });
    }

    internal SupportingDocument(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Catalogue_ID =
            int.Parse(r["Catalogue_ID"]
                .ToString()); //gets around decimals and other random crud number field types that sql returns
        Name = r["Name"].ToString();
        URL = ParseUrl(r, "URL");
        Description = r["Description"].ToString();
        Ticket = r["Ticket"] as string;
        Extractable = (bool)r["Extractable"];
        IsGlobal = Convert.ToBoolean(r["IsGlobal"]);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Returns true if <see cref="Extractable" /> and has a valid <see cref="URL" />
    /// </summary>
    /// <returns></returns>
    public bool IsReleasable()
    {
        if (!Extractable)
            return false;

        //if it has no url or the url is blank or the url is to something that isn't a file
        if (URL == null || string.IsNullOrWhiteSpace(URL.AbsoluteUri) || !URL.IsFile)
            return false;

        //ok let the user download it through the website <- Yes that's right, this method when returns true lets anyone grab the file via the website CatalogueWebService.cs
        return true;
    }

    /// <summary>
    ///     Returns the name of the file referenced by <see cref="URL" /> or null if it is not a file URL
    /// </summary>
    /// <returns></returns>
    public FileInfo GetFileName()
    {
        if (URL == null || string.IsNullOrWhiteSpace(URL.AbsoluteUri) || !URL.IsFile)
            return null;

        var unescaped = Uri.UnescapeDataString(URL.AbsolutePath);

        return URL.IsUnc ? new FileInfo($@"\\{URL.Host}{unescaped}") : new FileInfo(unescaped);
    }
}