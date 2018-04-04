using System;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Ticketing
{
    /// <summary>
    /// How RDMP interacts with an (optional) ticketing sytem such as Jira, Fogbugz etc.  RDMP allows you to control governance via your ticketing system and
    /// to associate ticket identifiers with project extracts, catalogues, issues etc.  By implementing this interface for your ticketing system you can prevent
    /// data releases unless a ticket is in a given state etc.
    /// 
    /// <para>IMPORTANT: you must have a constructor that takes a single parameter of Type TicketingSystemConstructorParameters</para>
    /// </summary>
    public interface ITicketingSystem:ICheckable
    {
        /// <summary>
        /// Called when the user enters a string he thinks is a valid ticket.  You should respond quickly (e.g. by Regex pattern matching not database query
        /// - unless that is trivially fast) to tell him whether the string is valid as a ticket.
        /// </summary>
        /// <param name="ticketName"></param>
        /// <returns></returns>
        bool IsValidTicketName(string ticketName);

        /// <summary>
        /// Occurs when the user has a ticket associated with an object and clicks Go To button.  This method should fire up a browser at the specified
        /// ticket so it is visible to the user.
        /// </summary>
        /// <param name="ticketName"></param>
        void NavigateToTicket(string ticketName);

        /// <summary>
        /// Called when the user (or RDMP) attempts to perform a release of an extracted project dataset.  You can pass back a state based on the tickets (which
        /// might be null) to indicate whether the ticketing system thinks the release should go ahead.  This empowers your ticketing system to control governance
        /// of when a release is allowed or not.  If you don't want this functionality just return Releasable.
        /// </summary>
        /// <param name="masterTicket"></param>
        /// <param name="requestTicket"></param>
        /// <param name="releaseTicket"></param>
        /// <param name="reason"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
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
