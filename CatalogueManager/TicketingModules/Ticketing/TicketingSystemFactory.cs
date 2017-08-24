using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace Ticketing
{
    public class TicketingSystemFactory
    {
        private readonly CatalogueRepository _repository;

        public TicketingSystemFactory(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public Type[] GetAllKnownTicketingSystems()
        {
            return _repository.MEF.GetTypes<ITicketingSystem>().ToArray();
        }

        //public ITicketingSystem Create(string )
        public ITicketingSystem Create(string typeName, string url, IDataAccessCredentials credentials)
        {
            if(string.IsNullOrWhiteSpace(typeName))
                throw new NullReferenceException("Type name was blank, cannot create ITicketingSystem");

            return _repository.MEF.FactoryCreateA<ITicketingSystem, TicketingSystemConstructorParameters>(typeName, new TicketingSystemConstructorParameters(url, credentials));
        }

        public ITicketingSystem CreateIfExists(TicketingSystemConfiguration ticketingSystemConfiguration)
        {
            //if there is no ticketing system
            if (ticketingSystemConfiguration == null)
                return null;

            //if there is no Type
            if (string.IsNullOrWhiteSpace(ticketingSystemConfiguration.Type))
                return null;

            IDataAccessCredentials creds = null;
            
            //if there are credentials create with those (otherwise create with null credentials)
            if(ticketingSystemConfiguration.DataAccessCredentials_ID != null)
                creds = _repository.GetObjectByID<DataAccessCredentials>((int)ticketingSystemConfiguration.DataAccessCredentials_ID);
            
            return Create(ticketingSystemConfiguration.Type, ticketingSystemConfiguration.Url, creds);
        }
    }
}
