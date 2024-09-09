// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction;

/// <summary>
/// Stores the configuration used to perform regex redactions
/// </summary>
public class RegexRedactionConfiguration : DatabaseEntity, IRegexRedactionConfiguration
{


    private string _regexPattern;
    private string _redactionString;
    private string _name;
    private string _description;
    private string _folder;



    public override string ToString()
    {
        return $"{_name}";
    }

    [Unique]
    [NotNull]
    [UsefulProperty]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    [NotNull]
    public string RegexPattern
    {
        get => _regexPattern;
        set => SetField(ref _regexPattern, value.ToString());
    }

    [NotNull]
    public string RedactionString
    {
        get => _redactionString;
        set => SetField(ref _redactionString, value);
    }
    public string Folder { get => _folder; set => SetField(ref _folder, value); }

    public RegexRedactionConfiguration() { }

    public RegexRedactionConfiguration(ICatalogueRepository repository, string name, Regex regexPattern, string redactionString, string description = null,string folder="\\")
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "RegexPattern", regexPattern.ToString() },
            {"RedactionString", redactionString },
            {"Description",description },
            {"Folder",folder }
        });
    }

    internal RegexRedactionConfiguration(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"].ToString();
        Description = r["Description"].ToString();
        RedactionString = r["RedactionString"].ToString();
        RegexPattern = r["RegexPattern"].ToString();
        Folder = r["Folder"].ToString();
    }


}
