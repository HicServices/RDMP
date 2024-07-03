// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
namespace Rdmp.Core.Curation
{
    /// <summary>
    /// Stored a status name and which ticketing system it coresponds to
    /// </summary>
    public class TicketingSystemReleaseStatus : DatabaseEntity, INamed
    {

        private string _status;
        private int? _statusID;
        private int _ticketingSystemConfiguratonID;

        public string Name { get => _status; set => SetField(ref _status, value); }
        public int? StatusID { get => _statusID; set => SetField(ref _statusID, value); }
        public int TicketingSystemConfigurationID { get => _ticketingSystemConfiguratonID; set => SetField(ref _ticketingSystemConfiguratonID, value); }

        public TicketingSystemReleaseStatus() { }

        public TicketingSystemReleaseStatus(ICatalogueRepository repository, string status, int? statusID, TicketingSystemConfiguration config) : base()
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                //{"Status", status},
                {"StatusID", statusID is null? statusID: DBNull.Value },
                {"TicketingSystemConfigurationID", config.ID }
            });
        }

        public TicketingSystemReleaseStatus(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            Name = r["Status"] as string;
            StatusID = r["StatusID"] as int?;
            TicketingSystemConfigurationID = int.Parse(r["TicketingSystemConfigurationID"].ToString());
        }
    }
}
