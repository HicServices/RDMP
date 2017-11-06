using System;
using ReusableLibraryCode.Checks;

namespace Ticketing
{

    public interface ITicketingSystem:ICheckable
    {
        bool IsValidTicketName(string ticketName);
        void NavigateToTicket(string ticketName);

        TicketingReleaseabilityEvaluation GetDataReleaseabilityOfTicket(string masterTicket, string requestTicket, string releaseTicket, out string reason, out Exception exception);

        string GetProjectFolderName(string masterTicket);
    }

    public enum TicketingReleaseabilityEvaluation
    {
        TicketingLibraryCrashed,
        TicketingLibraryMissingOrNotConfiguredCorrectly,
        CouldNotReachTicketingServer,
        CouldNotAuthenticateAgainstServer,
        NotReleaseable,
        Releaseable
    }
}
