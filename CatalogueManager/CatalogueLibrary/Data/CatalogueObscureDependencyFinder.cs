using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Handles rules for cascading/preventing deleting database objects which cannot be directly implemented by database constraints (e.g. foreign keys).  This includes
    /// things such as preventing deleting Catalogues which have been used in data extraction projects.  Use property OtherDependencyFinders to add new rules / logic for
    /// tailoring deleting.
    /// </summary>
    public class CatalogueObscureDependencyFinder : IObscureDependencyFinder
    {
        private readonly CatalogueRepository _repository;

        /// <summary>
        /// Sets the target upon which to apply delete/cascade obscure dependency rules
        /// </summary>
        /// <param name="repository"></param>
        public CatalogueObscureDependencyFinder(CatalogueRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Plugin and supplemental child IObscureDependencyFinders
        /// </summary>
        public List<IObscureDependencyFinder> OtherDependencyFinders = new List<IObscureDependencyFinder>();
        
        /// <inheritdoc/>
        /// <remarks>Consults each <see cref="OtherDependencyFinders"/> to see if deleting is disallowed</remarks>
        /// <param name="oTableWrapperObject"></param>
        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            foreach (IObscureDependencyFinder obscureDependencyFinder in OtherDependencyFinders)
                obscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);
        }

        /// <inheritdoc/>
        /// <remarks>Consults each <see cref="OtherDependencyFinders"/> for CASCADE logic then deletes any <see cref="AnyTableSqlParameter"/> declared
        /// on the deleted object</remarks>
        /// <param name="oTableWrapperObject"></param>
        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            foreach (IObscureDependencyFinder obscureDependencyFinder in OtherDependencyFinders)
                obscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);

            //Delete any SQLFilterParameters associated with the parent object (which has just been deleted!)
            if(AnyTableSqlParameter.IsSupportedType(oTableWrapperObject.GetType()))
                foreach (var p in _repository.GetAllParametersForParentTable(oTableWrapperObject))
                    p.DeleteInDatabase();
        }

        /// <summary>
        /// Adds a new instance of the supplied  <see cref="IObscureDependencyFinder"/> to <see cref="OtherDependencyFinders"/> if it is a unique Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repositoryLocator"></param>
        public void AddOtherDependencyFinderIfNotExists<T>(IRDMPPlatformRepositoryServiceLocator repositoryLocator) where T:IObscureDependencyFinder
        {
            if (OtherDependencyFinders.All(f => f.GetType() != typeof(T)))
            {
                ObjectConstructor constructor = new ObjectConstructor();
                OtherDependencyFinders.Add((T)constructor.Construct(typeof(T), repositoryLocator));
            }
        }
    }
}