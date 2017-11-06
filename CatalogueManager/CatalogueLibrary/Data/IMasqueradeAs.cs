using System;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// If you are a wrapper masquerading as another class e.g. ExtractableCohortUsedByProjectNode is a class masquerading as an ExtractableCohort
    /// </summary>
    public interface IMasqueradeAs
    {
        object MasqueradingAs();
    }
}