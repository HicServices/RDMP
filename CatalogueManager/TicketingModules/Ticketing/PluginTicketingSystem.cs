using System;
using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Ticketing
{

    [InheritedExport(typeof(ICheckable))]
    [InheritedExport(typeof(ITicketingSystem))]
    public abstract class PluginTicketingSystem : ICheckable, ITicketingSystem
    {
        protected IDataAccessCredentials Credentials { get; set; }
        protected string Url { get; set; }
        
        [ImportingConstructor]
        protected PluginTicketingSystem(TicketingSystemConstructorParameters parameters)
        {
            Credentials = parameters.Credentials;
            Url = parameters.Url;
        }

        public abstract void Check(ICheckNotifier notifier);
        public abstract bool IsValidTicketName(string ticketName);
        public abstract void NavigateToTicket(string ticketName);

        public abstract TicketingReleaseabilityEvaluation GetDataReleaseabilityOfTicket(string masterTicket, string requestTicket, string releaseTicket, out string reason, out Exception exception);
        
        public abstract string GetProjectFolderName(string masterTicket);
    }
}