using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using SynthEHR;
using System.Collections.Generic;

namespace Rdmp.Core.Datasets;

public class ENGBWrapper
{
    public string en_GB { get; set; }
}

public class URITerm
{
    public string URI { get; set; }
    public ENGBWrapper term { get; set; }
}
public class PureDescription
{
    public string PureID { get; set; }
    public ENGBWrapper Value { get; set; }
}

public class System
{
    public string SystemName { get; set; }
    public string UUID { get; set; }
}

public class Name()
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class PurePerson
{
    public string TypeDiscriminator { get; set; }
    public string PureID { get; set; }

    public Name Name { get; set; }
    public URITerm Role { get; set; }

    public List<System> Organizations { get; set; }
}

public class PureDate
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}

public class Visability
{
    public string Key { get; set; }
    public ENGBWrapper Description { get; set; }
}

public class Workflow
{
    public string Step { get; set; }
    public ENGBWrapper Description { get; set; }
}

public class Geolocation
{
    public ENGBWrapper GeographicalCoverage { get; set; }
}

public class PureDataset : PluginDataset
{
    public int PureID { get; set; }
    public string UUID { get; set; }
    public string CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }

    public string PortalURL { get; set; }

    public string Version { get; set; }

    public ENGBWrapper Title { get; set; }

    public List<PureDescription> Descriptions { get; set; }
    public System ManagingOrganisation { get; set; }

    public new URITerm Type { get; set; }

    public System Publisher { get; set; }

    public Geolocation Geolocation { get; set; }

    public List<PurePerson> Persons { get; set; }

    public List<System> Organizations { get; set; }
    public PureDate PublicationAvailableDate { get; set; }

    public bool OpenAireCompliant { get; set; }

    public Visability Visability { get; set; }

    //TODO custom defined fields

    public Workflow Workflow { get; set; }

    public string SystemName { get; set; }


    public PureDataset() { }

}
