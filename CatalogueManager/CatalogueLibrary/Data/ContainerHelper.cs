using System.Collections.Generic;

namespace CatalogueLibrary.Data
{
    public class ContainerHelper
    {
        public List<IContainer> GetAllSubContainersRecursively(IContainer current)
        {
            List<IContainer> toReturn = new List<IContainer>();

            var currentSubs = current.GetSubContainers();
            toReturn.AddRange(currentSubs);

            foreach (IContainer sub in currentSubs)
                toReturn.AddRange(GetAllSubContainersRecursively(sub));

            return toReturn;
        }

        public IContainer GetRootContainerOrSelf(IContainer container)
        {
            var parent = container.GetParentContainerIfAny();
            if (parent != null)
                return GetRootContainerOrSelf(parent);

            return container;
        }

        public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively(IContainer container)
        {
            List<IFilter> toReturn = new List<IFilter>();

            toReturn.AddRange(container.GetFilters());

            IContainer[] subs = container.GetSubContainers();

            if (subs != null)
                foreach (IContainer sub in subs)
                    toReturn.AddRange(GetAllFiltersIncludingInSubContainersRecursively(sub));

            return toReturn;
        }
    }
}