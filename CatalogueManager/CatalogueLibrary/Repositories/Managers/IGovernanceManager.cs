using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;

namespace CatalogueLibrary.Repositories.Managers
{
    public interface IGovernanceManager
    {
        void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue);
        void Link(GovernancePeriod governancePeriod, ICatalogue catalogue);
        
        /// <summary>
        /// Returns the IDs of all <see cref="GovernancePeriod"/> with the corresponding set of <see cref="Catalogue"/> IDs which are covered by the governance.
        /// </summary>
        /// <returns></returns>
        Dictionary<int, HashSet<int>> GetAllGovernedCataloguesForAllGovernancePeriods();

        IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod);
    }
}