// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Ticketing;

/// <summary>
/// Simple implementation of an RDMP ticketing system.  Opens a browser
/// at the Url + ticket name.  Has no validation restrictions based on
/// ticket states.
/// </summary>
public class SimpleTicketingSystem : ICheckable, ITicketingSystem
{
    protected IDataAccessCredentials Credentials { get; set; }
    protected string Url { get; set; }

    protected SimpleTicketingSystem(TicketingSystemConstructorParameters parameters)
    {
        Credentials = parameters.Credentials;
        Url = parameters.Url;
    }

    public void Check(ICheckNotifier notifier)
    {
        // all ticket names are valid
    }

    public bool IsValidTicketName(string ticketName) =>
        // all ticket names are valid
        true;

    public void NavigateToTicket(string ticketName)
    {
        // if the user has added a URL just append the ticket name to it
        // and open e.g. "www.myticketing?q=" + "HDD-123"
        if (!string.IsNullOrWhiteSpace(Url))
            UsefulStuff.OpenUrl(Url + ticketName);
    }

    public TicketingReleaseabilityEvaluation GetDataReleaseabilityOfTicket(string masterTicket, string requestTicket,
        string releaseTicket, List<TicketingSystemReleaseStatus> acceptedStatuses, out string reason, out Exception exception)
    {
        reason = null;
        exception = null;
        // No restrictions on releasability
        return TicketingReleaseabilityEvaluation.Releaseable;
    }

    public string GetProjectFolderName(string masterTicket) =>
        UsefulStuff.RegexThingsThatAreNotNumbersOrLettersOrUnderscores.Replace(masterTicket, "");

    public List<string> GetAvailableStatuses()
    {
        return new List<string>();
    }
}