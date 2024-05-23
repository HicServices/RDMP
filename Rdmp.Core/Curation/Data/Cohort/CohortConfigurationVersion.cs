using Amazon.Runtime.Internal.Transform;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Cohort;

public class CohortConfigurationVersion : DatabaseEntity
{

    private int _cohortIdentificationConfigurationId;
    private string _name;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    public int CohortIdentificationConfigurationId
    {
        get => _cohortIdentificationConfigurationId;
        set => SetField(ref _cohortIdentificationConfigurationId, value);
    }


    public CohortConfigurationVersion() { }

    public CohortConfigurationVersion(int cohortIdentificationConfigurationId, string name)
    {
        _cohortIdentificationConfigurationId = cohortIdentificationConfigurationId;
        _name = name;
    }

    public CohortConfigurationVersion(ICatalogueRepository repository, int cohortIdentificationConfigurationId, string name)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "CohortIdentificationConfigurationId", cohortIdentificationConfigurationId}
        });
    }

    internal CohortConfigurationVersion(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        Name = r["Name"].ToString();
        CohortIdentificationConfigurationId = Int32.Parse(r["CohortIdentificationConfigurationId"].ToString());
    }
}
