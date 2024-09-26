using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using SynthEHR;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rdmp.Core.Datasets;
#nullable enable

public class ENGBWrapper
{
    public ENGBWrapper(string? text) { en_GB = text; }
    public string? en_GB { get; set; }
}

public class URITerm
{
    public URITerm(string? uri, ENGBWrapper enGBWrapper)
    {
        URI = uri;
        term = enGBWrapper;
    }
    public string? URI { get; set; }
    public ENGBWrapper term { get; set; }
}
public class PureDescription
{
    public int? PureID { get; set; }
    public ENGBWrapper? Value { get; set; }
}

public class System
{
    public System(string? uuid, string? systemName)
    {
        UUID = uuid;
        SystemName = systemName;
    }
    public string? SystemName { get; set; }
    public string? UUID { get; set; }
}

public class Name()
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class PurePerson
{
    public string? TypeDiscriminator { get; set; }
    public int? PureID { get; set; }

    public Name? Name { get; set; }
    public URITerm? Role { get; set; }

    public List<System>? Organizations { get; set; }
}

public class PureDate
{
    public PureDate(int year, int? month = null, int? day = null)
    {
        Year = year;
        if (month != null) Month = month;
        if (day != null) Day = day;
    }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
}

public class Visibility
{
    public Visibility(string? key, ENGBWrapper description)
    {
        Key = key;
        Description = description;
    }
    public string? Key { get; set; }
    public ENGBWrapper Description { get; set; }
}

public class Workflow
{
    public string? Step { get; set; }
    public ENGBWrapper? Description { get; set; }
}

public class Geolocation
{
    public ENGBWrapper? GeographicalCoverage { get; set; }
    public string? Point { get; set; }

    public string? Polygon { get; set; }
}

public class TemporalcoveragePeriod
{
    public PureDate? StartDate { get; set; }
    public PureDate? EndDate { get; set; }
}

public class PureDataset : PluginDataset
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PureID { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UUID { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public string? CreatedBy { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public string? ModifiedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public string? ModifiedBy { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PortalURL { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ENGBWrapper? Title { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PureDescription>? Descriptions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public System? ManagingOrganization { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new URITerm? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public System? Publisher { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Geolocation? Geolocation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PurePerson>? Persons { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<System>? Organizations { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PureDate? PublicationAvailableDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? OpenAireCompliant { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Visibility? Visibility { get; set; }

    //TODO custom defined fields

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Workflow? Workflow { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SystemName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public TemporalcoveragePeriod? TemporalcoveragePeriod { get; set; }

#nullable disable

    public PureDataset() { }

}
