// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Rdmp.Core.Curation
{
    /// <inheritdoc/>
    public class ExternalAsset : DatabaseEntity, IExternalAsset
    {

        private string _name;
        private string _type;
        private int _ticketingConfigurationId;
        private int _objectId;
        private int _externalAssetId;

        /// <inheritdoc/>
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        /// <inheritdoc/>
        public string ObjectType
        {
            get => _type;
            set => SetField(ref _type, value);
        }
        /// <inheritdoc/>
        public int TicketingConfiguration_ID
        {
            get => _ticketingConfigurationId;
            set => SetField(ref _ticketingConfigurationId, value);
        }

        /// <inheritdoc/>
        public int ExternalAsset_ID
        {
            get => _externalAssetId;
            set => SetField(ref _externalAssetId, value);
        }

        /// <inheritdoc/>
        public int ObjectId
        {
            get => _objectId;
            set => SetField(ref _objectId, value);
        }

        public override string ToString() => Name;

        public ExternalAsset(ICatalogueRepository catalogueRepository, string name, int externalAssetId,int ticketingConfigurationId, string type, int id)
        {

            catalogueRepository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name },
                {"ExternalAssetID", externalAssetId},
                {"TicketingConfigurationID", ticketingConfigurationId},
                {"ObjectID", id},
                {"ObjectType", type}
            });
        }

        internal ExternalAsset(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            Name = r["Name"].ToString();
            ExternalAsset_ID = int.Parse(r["ExternalAssetID"].ToString());
            TicketingConfiguration_ID = int.Parse(r["TicketingConfigurationID"].ToString());
            ObjectType = r["ObjectType"].ToString();
            ObjectId = int.Parse(r["ObjectID"].ToString());
        }

        /// <summary>
        /// Fetched the associated RDMP object
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public object GetAssociatedObject(ICatalogueRepository repository)
        {
            if(ObjectType == typeof(Project).ToString())
            {
                return repository.GetAllObjectsWhere<Project>("ID", ObjectId).FirstOrDefault();
            }
            return null;
        }


    }
}
