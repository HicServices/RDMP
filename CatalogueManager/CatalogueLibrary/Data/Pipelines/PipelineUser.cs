using System;
using System.Reflection;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.Pipelines
{
    public class PipelineUser:IPipelineUser
    {
        private readonly PropertyInfo _property;
        private ICatalogueRepository _catalogueRepository;

        public DatabaseEntity User { get; private set; }
        public PipelineGetter Getter { get; private set; }
        public PipelineSetter Setter { get; private set; }

        public PipelineUser(PropertyInfo property, DatabaseEntity user)
        {
            _property = property;
            User = user;

            _catalogueRepository = User.Repository as ICatalogueRepository;
            var dataExportRepo = User.Repository as IDataExportRepository;

            if (dataExportRepo != null)
                _catalogueRepository = dataExportRepo.CatalogueRepository;

            if (_catalogueRepository == null)
                throw new Exception("Repository of Host '" + User + "' was not an ICatalogueRepository or a IDataExportRepository");
            
            Getter = Get;
            Setter = Set;
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