using System.Collections.Generic;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    public class CatalogueObscureDependencyFinder : IObscureDependencyFinder
    {
        private readonly CatalogueRepository _repository;

        public CatalogueObscureDependencyFinder(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public List<IObscureDependencyFinder> OtherDependencyFinders = new List<IObscureDependencyFinder>();
        
        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            foreach (IObscureDependencyFinder obscureDependencyFinder in OtherDependencyFinders)
                obscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            foreach (IObscureDependencyFinder obscureDependencyFinder in OtherDependencyFinders)
                obscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);

            //Delete any SQLFilterParameters associated with the parent object (which has just been deleted!)
            if(AnyTableSqlParameter.IsSupportedType(oTableWrapperObject.GetType()))
                foreach (var p in _repository.GetAllParametersForParentTable(oTableWrapperObject))
                    p.DeleteInDatabase();
        }
    }
}