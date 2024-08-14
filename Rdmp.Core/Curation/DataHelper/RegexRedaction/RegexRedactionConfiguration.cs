using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction;

public class RegexRedactionConfiguration: DatabaseEntity, IRegexRedactionConfiguration
{


    private string _regexPattern;
    private string _redactionString;
    private string _name;
    private string _description;

    [Unique]
    [NotNull]
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


    public RegexRedactionConfiguration() { }

    public RegexRedactionConfiguration(ICatalogueRepository repository,string name, Regex regexPattern, string redactionString,string description=null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "RegexPattern", regexPattern.ToString() },
            {"RedactionString", redactionString },
            {"Description",description }
        });
    }

    internal RegexRedactionConfiguration(ICatalogueRepository repository, DbDataReader r): base(repository,r)
    {
        Name = r["Name"].ToString();
        Description = r["Description"].ToString();
        RedactionString= r["RedactionString"].ToString();
        RegexPattern = r["RegexPattern"].ToString();
    }


}
