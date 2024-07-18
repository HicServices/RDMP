// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Ticketing;

public abstract class PluginTicketingSystem : ITicketingSystem
{
    protected IDataAccessCredentials Credentials { get; set; }
    protected string Url { get; set; }

    protected PluginTicketingSystem(TicketingSystemConstructorParameters parameters)
    {
        Credentials = parameters.Credentials;
        Url = parameters.Url;
    }

    public abstract void Check(ICheckNotifier notifier);
    public abstract bool IsValidTicketName(string ticketName);
    public abstract void NavigateToTicket(string ticketName);

    public abstract TicketingReleaseabilityEvaluation GetDataReleaseabilityOfTicket(string masterTicket,
        string requestTicket, string releaseTicket, List<TicketingSystemReleaseStatus> acceptedStatuses,out string reason, out Exception exception);

    public abstract string GetProjectFolderName(string masterTicket);

    public abstract List<string> GetAvailableStatuses();

}