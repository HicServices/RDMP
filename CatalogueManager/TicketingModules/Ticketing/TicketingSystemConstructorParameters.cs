using ReusableLibraryCode.DataAccess;

namespace Ticketing
{
    public class TicketingSystemConstructorParameters
    {
        public string Url { get; set; }
        public IDataAccessCredentials Credentials { get; set; }

        public TicketingSystemConstructorParameters(string url, IDataAccessCredentials credentials)
        {
            Url = url;
            Credentials = credentials;
        }
    }
}
