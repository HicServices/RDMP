using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using NPOI.SS.Formula.Atp;
using NPOI.SS.Formula.Functions;
using SynthEHR;
using System;
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
    public int? PureId { get; set; }
    public ENGBWrapper? Value { get; set; }

    public URITerm Term { get => new URITerm("/dk/atira/pure/dataset/descriptions/datasetdescription", new ENGBWrapper("Description")); }
}

public class Name()
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class PurePerson
{
    public string? TypeDiscriminator { get; set; }
    public int? PureId { get; set; }

    public Name? Name { get; set; }
    public URITerm? Role { get; set; }

    public List<PureSystem>? Organizations { get; set; }
}

public class PureDate
{
    public PureDate(DateTime dateTime)
    {
        Year = dateTime.Year;
        Month = dateTime.Month;
        Day = dateTime.Day;
    }

    public PureDate() { }


    public DateTime ToDateTime()
    {
        return new DateTime(Year, Month??1, Day??1, 0, 0, 0);
    }

    public bool IsBefore(PureDate date)
    {
        if (Year < date.Year) return true;
        if (Year == date.Year)
        {
            if (Month < date.Month) return true;
            if (Month == date.Month)
            {
                return Day < date.Day;
            }
        }

        return false;
    }

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

public class TemporalCoveragePeriod
{
    public PureDate? StartDate { get; set; }
    public PureDate? EndDate { get; set; }
}

public class PureSystem
{
    public PureSystem(string? uuid, string? systemName)
    {
        UUID = uuid;
        SystemName = systemName;
    }
    public string? SystemName { get; set; }
    public string? UUID { get; set; }
}

public class PureDataset : PluginDataset
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PureId { get; set; }

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
    public PureSystem? ManagingOrganization { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public new URITerm? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PureSystem? Publisher { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Geolocation? Geolocation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PurePerson>? Persons { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PureSystem>? Organizations { get; set; }

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

    public TemporalCoveragePeriod? TemporalCoveragePeriod { get; set; }

#nullable disable

    public PureDataset() { }
}
