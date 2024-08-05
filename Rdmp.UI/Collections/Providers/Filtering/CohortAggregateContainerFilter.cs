using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections.Providers.Filtering;

public class CohortAggregateContainerFilter : IModelFilter
{
    private readonly bool _filter = false;

    public CohortAggregateContainerFilter()
    {
        _filter = UserSettings.ScoreZeroForCohortAggregateContainers;
    }
    public bool Filter(object modelObject)
    {
        if (_filter)
        {
            return modelObject is CohortAggregateContainer;
        }
        return true;
    }
}

