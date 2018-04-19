using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Sharing.Sharing
{
    [Serializable]
    public class MapsDirectlyToDatabaseTableStatelessDefinition
    {
        public Guid SharingGuid { get; set; }
        public Type Type { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        protected MapsDirectlyToDatabaseTableStatelessDefinition(Guid sharingGuid,Type type) : this(type, new Dictionary<string, object>())
        {
            SharingGuid = sharingGuid;
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Type type, Dictionary<string, object> properties)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable", "type");

            Type = type;
            Properties = properties;
        }

        public IMapsDirectlyToDatabaseTable ImportObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {

            var shareManager = new ShareManager(repositoryLocator);

            if(shareManager.IsImported(SharingGuid.ToString()))
                throw new NotImplementedException("Object "+SharingGuid+" has already been imported");

            //todo could be DataExport too
            var target = repositoryLocator.CatalogueRepository;
            using (var con = target.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM " + Type.Name, con.Connection);
                var cmdbuilder = new DiscoveredServer(target.ConnectionStringBuilder).Helper.GetCommandBuilder(cmd);

                DbCommand cmdInsert = cmdbuilder.GetInsertCommand(true);
                cmdInsert.CommandText += ";SELECT @@IDENTITY;";

                target.PrepareCommand(cmdInsert, Properties);

                var id = Convert.ToInt32(cmdInsert.ExecuteScalar());

                var newObj = target.GetObjectByID(Type, id);

                if(SharingGuid != Guid.Empty)
                    shareManager.GetImportAs(SharingGuid.ToString(), newObj);

                return newObj;

            }
        }
    }

    [Serializable]
    public class MapsDirectlyToDatabaseTableStatelessDefinition<T> : MapsDirectlyToDatabaseTableStatelessDefinition where T : IMapsDirectlyToDatabaseTable
    {
        public MapsDirectlyToDatabaseTableStatelessDefinition(T mappedObject) : this(Guid.Empty, mappedObject)
        {
            
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Guid sharinGuid,T mappedObject) : base(sharinGuid,typeof(T))
        {
            Properties = TableRepository.GetPropertyInfos(mappedObject.GetType()).ToDictionary(p => p.Name, p2 => p2.GetValue(mappedObject));
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Dictionary<string, object> properties) : base(typeof(T), properties)
        {
        }

        public void Rehydrate(T instance)
        {
            foreach (var property in Properties)
            {
                //HACK:
                if(property.Key.EndsWith("_ID"))
                    continue;
                
                instance.GetType().GetProperty(property.Key).GetSetMethod().Invoke(instance, new [] {property.Value});
            }
        }
    }
}