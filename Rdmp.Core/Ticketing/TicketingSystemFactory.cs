// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Ticketing;

/// <summary>
/// RDMP can have a single ITicketingSystem configured (optional).  This factory creates the ITicketingSystem instance based on the
/// TicketingSystemConfiguration the uer has set up
/// </summary>
public class TicketingSystemFactory
{
    private readonly RDMPDbContext _catalogueDbContext;

    public TicketingSystemFactory(RDMPDbContext catalogueDbContext)
    {
        _catalogueDbContext = catalogueDbContext;
    }

    public static Type[] GetAllKnownTicketingSystems() => MEF.GetTypes<ITicketingSystem>().ToArray();

    //public ITicketingSystem Create(string )
    public static ITicketingSystem Create(string typeName, string url, IDataAccessCredentials credentials) =>
        string.IsNullOrWhiteSpace(typeName)
            ? throw new NullReferenceException("Type name was blank, cannot create ITicketingSystem")
            : MEF.CreateA<ITicketingSystem>(typeName, new TicketingSystemConstructorParameters(url, credentials));

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
        if (ticketingSystemConfiguration.DataAccessCredentials_ID != null)
            creds = _catalogueDbContext.GetObjectByID<DataAccessCredentials>((int)ticketingSystemConfiguration
                .DataAccessCredentials_ID);

        return Create(ticketingSystemConfiguration.Type, ticketingSystemConfiguration.Url, creds);
    }
}