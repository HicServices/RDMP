using System;
using System.Reflection;
using System.Web.Security.AntiXss;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Reflection;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Helper for standardising access to properties on a class which reference a Pipeline.  Because many classes reference Pipelines and some reference multiple Pipelines 
    /// we use this class to abstract that away.  For example the CacheProgress constructor says to use "Pipeline_ID" int property.
    /// 
    /// Currently used primarily by PipelineSelectionUIFactory 
    /// </summary>
    public class PipelineUser:IPipelineUser
    {
        private readonly PropertyInfo _property;
        private ICatalogueRepository _catalogueRepository;

        public DatabaseEntity User { get; private set; }
        public PipelineGetter Getter { get; private set; }
        public PipelineSetter Setter { get; private set; }

        public PipelineUser(PropertyInfo property, DatabaseEntity user,ICatalogueRepository repository = null)
        {
            _property = property;
            User = user;

            //if user passed in an explicit one
            _catalogueRepository = repository;

            //otherwise get it from the user
            if (_catalogueRepository == null)
            {
                if (User.Repository == null)
                    throw new Exception("User does not have a Repository! how can it be a DatabaseEntity!");

                _catalogueRepository = User.Repository as ICatalogueRepository;
                var dataExportRepo = User.Repository as IDataExportRepository;

                if (dataExportRepo != null)
                    _catalogueRepository = dataExportRepo.CatalogueRepository;

                if (_catalogueRepository == null)
                    throw new Exception("Repository of Host '" + User + "' was not an ICatalogueRepository or a IDataExportRepository.  user came from a Repository called '" + user.Repository.GetType().Name + "' in this case you will need to specify the ICatalogueRepository property to this method so we know where to fetch Pipelines from");
                
            }
            Getter = Get;
            Setter = Set;
            
        }
        public PipelineUser(string nameOfProperty, DatabaseEntity user, ICatalogueRepository repository = null): this(user.GetType().GetProperty(nameOfProperty), user, repository)
        {

        }

        public PipelineUser(CacheProgress cacheProgress):this(typeof(CacheProgress).GetProperty("Pipeline_ID"),cacheProgress)
        {
            
        }

        private Pipeline Get()
        {
            var id = (int?) _property.GetValue(User);

            if(id == null)
                return null;

            return _catalogueRepository.GetObjectByID<Pipeline>(id.Value);

        }

        private void Set(Pipeline newPipelineOrNull)
        {
            if (newPipelineOrNull != null)
                _property.SetValue(User, newPipelineOrNull.ID);
            else
                _property.SetValue(User, null);

            User.SaveToDatabase();
        }
    }

    //for classes which have a Pipeline_ID column but btw the reason we don't just have IPipelineHost as an interface is because you can have 2+ Pipeline IDs e.g. ExtractionConfiguration which has a Default and a Refresh
    public delegate void PipelineSetter(Pipeline newPipelineOrNull);
    public delegate Pipeline PipelineGetter();
}