// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers.Nodes.UsedByNodes;

namespace Rdmp.Core.Providers.Nodes.CohortNodes
{
    /// <summary>
    /// Indicates that a <see cref="CohortIdentificationConfiguration"/> has a database in which to store temporary tables in that reflect the reuslts of subcomponents 
    /// of the full query.  This improves query performance and allows cross server / database type cohort generation.  
    /// 
    /// <para>Cache invalidation automatically occurs when subcomponents are changed</para>
    /// </summary>
    class QueryCacheUsedByCohortIdentificationNode : ObjectUsedByOtherObjectNode<CohortIdentificationConfiguration, ExternalDatabaseServer>, IDeletableWithCustomMessage
    {
        public QueryCacheUsedByCohortIdentificationNode(CohortIdentificationConfiguration cic, ExternalDatabaseServer cacheServer)
            : base(cic, cacheServer)
        {
        }

        public void DeleteInDatabase()
        {
            User.QueryCachingServer_ID = null;
            User.SaveToDatabase();
        }

        /// <inheritdoc/>
        public string GetDeleteMessage()
        {
            return "Stop using cache database '" + ObjectBeingUsed + "'";
        }

        /// <inheritdoc/>
        public string GetDeleteVerb()
        {
            return "Remove";
        }
    }
}
