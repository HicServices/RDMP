// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Repositories.Managers;
using CatalogueLibrary.Ticketing;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// See CatalogueRepository
    /// </summary>
    public interface ICatalogueRepository : IRepository
    {
        /// <summary>
        /// Allows creation/discover/deletion of <see cref="AggregateForcedJoin"/> objects
        /// </summary>
        IAggregateForcedJoinManager AggregateForcedJoinManager { get;}

        IGovernanceManager GovernanceManager { get; }

        /// <summary>
        /// Allows linking/unlinking <see cref="DataAccessCredentials"/> to <see cref="TableInfo"/>
        /// </summary>
        ITableInfoCredentialsManager TableInfoCredentialsManager { get; }
        
        /// <summary>
        /// Allows creation/discover of <see cref="JoinInfo"/> objects which describe how to join two <see cref="TableInfo"/> together in SQL
        /// </summary>
        IJoinManager JoinManager { get;}

        /// <summary>
        /// Supports creation of objects using Reflection and discovery of Types based on Managed Extensibility Framework Export attributes.
        /// </summary>
        MEF MEF { get; set; }

        /// <summary>
        /// Stores class comments discovered at startup using NuDoq
        /// </summary>
        CommentStore CommentStore { get; }

        /// <summary>
        /// Manages information about what set containers / subcontainers exist under a <see cref="CohortIdentificationConfiguration"/>
        /// </summary>
        ICohortContainerManager CohortContainerManager { get;}

        /// <summary>
        /// Handles encrypting/decrypting strings with private/public key encryption
        /// </summary>
        IEncryptionManager EncryptionManager { get; }

        /// <summary>
        /// Handles forbidding deleting stuff / cascading deletes into other objects
        /// </summary>
        IObscureDependencyFinder ObscureDependencyFinder { get; set; }

        /// <summary>
        /// Manager for AND/OR WHERE containers and filters
        /// </summary>
        IFilterManager FilterManager {get;}

        /// <summary>
        /// Returns a new <see cref="HIC.Logging.LogManager"/> that audits in the default logging server specified by <see cref="ServerDefaults"/>
        /// </summary>
        /// <returns></returns>
        LogManager GetDefaultLogManager();

        /// <summary>
        /// Returns all sql parameters declared in the immediate scope of the <paramref name="parent"/> (does not include parameters that are declared at a lower scope).
        /// 
        /// <para>To determine which parent types are supported see <see cref="AnyTableSqlParameter.IsSupportedType"/></para>
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent);

        /// <summary>
        /// Returns the persistence object which describes which <see cref="ITicketingSystem"/> should be consulted when making governance decisions (e.g. according to
        /// ticketing system, can I release this dataset?).  Returns null if no ticketing system has been configured.
        /// 
        /// <para>Use <see cref="TicketingSystemFactory"/> to instantiate an <see cref="ITicketingSystem"/> instance</para>
        /// </summary>
        /// <returns></returns>
        TicketingSystemConfiguration GetTicketingSystem();
        
        T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null)
            where T : IMapsDirectlyToDatabaseTable;
        
        DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null);

        T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity;

        IServerDefaults GetServerDefaults();

        /// <summary>
        /// True if the <paramref name="tableInfo"/> has <see cref="Lookup"/> relationships declared which make it a linkable lookup table in queries.
        /// </summary>
        /// <returns></returns>
        bool IsLookupTable(ITableInfo tableInfo);

        /// <summary>
        /// Returns all Catalogues which have any CatalogueItems which are associated with any of the ColumnInfos of this TableInfo.  If this is a lookup table then expect to get back 
        /// a whole bunch of catalogues.
        /// </summary>
        /// <returns></returns>
        Catalogue[] GetAllCataloguesUsing(TableInfo tableInfo);
    }
}