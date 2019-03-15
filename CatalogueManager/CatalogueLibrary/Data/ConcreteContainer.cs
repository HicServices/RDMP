using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Managers;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Common abstract base class for IContainer (AND/OR Where logic) classes that are persisted in the database as a <see cref="DatabaseEntity"/>
    /// </summary>
    public abstract class ConcreteContainer:DatabaseEntity, IContainer
    {
        private readonly IFilterManager _manager;
        private FilterContainerOperation _operation;
        /// <inheritdoc/>
        public FilterContainerOperation Operation
        {
            get { return _operation; }
            set { SetField(ref  _operation, value); }
        }

        protected ConcreteContainer(IFilterManager manager,IRepository repository, DbDataReader r):base(repository,r)
        {
            _manager = manager;
            Operation = (FilterContainerOperation) Enum.Parse(typeof(FilterContainerOperation), r["Operation"].ToString());
        }

        protected ConcreteContainer(IFilterManager manager)
        {
            _manager = manager;
        }

        /// <inheritdoc/>
        public IContainer GetParentContainerIfAny()
        {
            return _manager.GetParentContainerIfAny(this);
        }

        /// <inheritdoc/>
        public IContainer[] GetSubContainers()
        {
            return _manager.GetSubContainers(this);
        }

        /// <inheritdoc/>
        public IFilter[] GetFilters()
        {
            return _manager.GetFilters(this);
        }

        /// <inheritdoc/>
        public void AddChild(IContainer child)
        {
            _manager.AddSubContainer(this,child);
        }

        /// <inheritdoc/>
        public void AddChild(IFilter filter)
        {
            if (filter.FilterContainer_ID.HasValue)
                if (filter.FilterContainer_ID == ID)
                    return; //It's already a child of us
                else
                    throw new NotSupportedException("Filter " + filter + " is already a child of nother container (ID=" + filter.FilterContainer_ID + ")");

            _manager.AddChild(this, filter);
        }

        /// <inheritdoc/>
        public void MakeIntoAnOrphan()
        {
            _manager.MakeIntoAnOrphan(this);
        }

        /// <inheritdoc/>
        public IContainer GetRootContainerOrSelf()
        {
            return GetRootContainerOrSelf(this);
        }

        private IContainer GetRootContainerOrSelf(IContainer container)
        {
            var parent = container.GetParentContainerIfAny();
            if (parent != null)
                return GetRootContainerOrSelf(parent);

            return container;
        }

        /// <inheritdoc/>
        public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively()
        {
            return GetAllFiltersIncludingInSubContainersRecursively(this);
        }
        
        private List<IFilter> GetAllFiltersIncludingInSubContainersRecursively(IContainer container)
        {
            List<IFilter> toReturn = new List<IFilter>();

            toReturn.AddRange(container.GetFilters());

            IContainer[] subs = container.GetSubContainers();

            if (subs != null)
                foreach (IContainer sub in subs)
                    toReturn.AddRange(GetAllFiltersIncludingInSubContainersRecursively(sub));

            return toReturn;
        }

        public abstract Catalogue GetCatalogueIfAny();

        /// <inheritdoc/>
        public List<IContainer> GetAllSubContainersRecursively()
        {
            return GetAllSubContainersRecursively(this);
        }

        private List<IContainer> GetAllSubContainersRecursively(IContainer current)
        {
            List<IContainer> toReturn = new List<IContainer>();

            var currentSubs = current.GetSubContainers();
            toReturn.AddRange(currentSubs);

            foreach (IContainer sub in currentSubs)
                toReturn.AddRange(GetAllSubContainersRecursively(sub));

            return toReturn;
        }
    }
}