// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Diagnostics.CodeAnalysis;
using Rdmp.Core.Curation.Data;



namespace HICPlugin.Curation.Data;

public class RedactedCHI : DatabaseEntity, IRedactedCHI
{
    private string _potentialCHI;
    private string _chiContext;
    private string _chiLocation;
    //string _name;
    //string _digitalObjectIdentifier;
    //string _source;
    //private string _folder = FolderHelper.Root;

    ///// <inheritdoc/>
    //[DoNotImportDescriptions]
    //[UsefulProperty]
    //public string Folder
    //{
    //    get => _folder;
    //    set => SetField(ref _folder, FolderHelper.Adjust(value));
    //}

    [NotNull]
    public string PotentialCHI
    {
        get => _potentialCHI;
        set => SetField(ref _potentialCHI, value);
    }

    [NotNull]
    public string CHIContext
    {
        get => _chiContext;
        set => SetField(ref _chiContext, value);
    }

    [NotNull]
    public string CHILocation
    {
        get => _chiLocation;
        set => SetField(ref _chiLocation, value);
    }

    //[Unique]
    //public string DigitalObjectIdentifier
    //{
    //    get => _digitalObjectIdentifier;
    //    set => SetField(ref _digitalObjectIdentifier, value);
    //}

    //public string Source
    //{
    //    get => _source;
    //    set => SetField(ref _source, value);
    //}

    public RedactedCHI(ICatalogueRepository catalogueRepository, string potentialCHI, string chiContext,string chiLocation)
    {
        catalogueRepository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"potentialCHI", potentialCHI },{"CHIContext",chiContext},{"CHILocation", chiLocation}
        });
    }

    public RedactedCHI() { }
    internal RedactedCHI(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        PotentialCHI = r["PotentialChi"].ToString();
        CHIContext = r["CHIContext"].ToString();
        //    Name = r["Name"].ToString();
        //    Folder = r["Folder"].ToString();
        //    if (r["DigitalObjectIdentifier"] != DBNull.Value)
        //        DigitalObjectIdentifier = r["DigitalObjectIdentifier"].ToString();
        //    if (r["Source"] != DBNull.Value)
        //        Source = r["Source"].ToString();
    }
}