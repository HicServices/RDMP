// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Each Catalogue database can have 0 or 1 TicketingSystemConfiguration, this is a pointer to a plugin that handles communicating with a ticketing/issue system
/// such as JIRA.  This ticketing system is used to record ticket numbers of a variety of objects (e.g. SupportingDocuments, extraction projects etc) and allows them
/// to accrue man hours without compromising your current workflow.
/// 
/// <para>In addition to tying objects to your ticketing system, the ticketing system will also be consulted about wheter data extraction projects are good to go or should
/// not be released (e.g. do not release project X until it has been paid for / signed off by the governancer).  The exact implementation of this is mostly left to the
/// ticketing class you write.</para>
/// 
/// <para>The Type field refers to a class that implements PluginTicketingSystem (see LoadModuleAssembly for how to write your own handler or use one of the compatible existing ones).
/// this class will handle all communication with the ticketing system/server.</para>
///
/// <para>There is also a reference to DataAccessCredentials record which stores optional username and encrypted password to use in the plugin for communicating with the ticketing system.</para>
/// 
/// </summary>
public class TicketingSystemConfiguration : DatabaseEntity, INamed
{
    #region Database Properties

    private bool _isActive;
    private string _url;
    private string _type;
    private string _name;
    private int? _dataAccessCredentials_ID;

    /// <summary>
    /// True if the ticketing system should be used/consulted.  Set to false if you want to temporarily disable the ticketing system link to RDMP
    /// without actually deleting the object.
    /// 
    /// <para>See:</para><see cref="CatalogueRepository.GetTicketingSystem"/>
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetField(ref _isActive, value);
    }

    /// <summary>
    /// The Url for communicating with the <see cref="ITicketingSystem"/>
    /// </summary>
    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    /// <summary>
    /// The C# System.Type of the <see cref="ITicketingSystem"/> which should be used to interact with the ticketing service
    /// </summary>
    public string Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    /// <inheritdoc/>
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// The credentials to use to connect to the ticketing service (username/password)
    /// </summary>
    public int? DataAccessCredentials_ID
    {
        get => _dataAccessCredentials_ID;
        set => SetField(ref _dataAccessCredentials_ID, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    /// Fetches the credentials to use when connecting to the ticketing service.  Returns null if no credentials have been
    /// configured.
    /// </summary>
    [NoMappingToDatabase]
    public DataAccessCredentials DataAccessCredentials =>
        DataAccessCredentials_ID == null
            ? null
            : Repository.GetObjectByID<DataAccessCredentials>((int)DataAccessCredentials_ID);

    #endregion

    public TicketingSystemConfiguration()
    {
    }

    /// <inheritdoc/>
    public TicketingSystemConfiguration(ICatalogueRepository repository, string name) : base()
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name != null ? (object)name : DBNull.Value },
            { "IsActive", true }
        });
    }

    public List<TicketingSystemReleaseStatus> GetReleaseStatuses()
    {
        var x = Repository.GetAllObjects<TicketingSystemReleaseStatus>();
        return [.. Repository.GetAllObjectsWhere<TicketingSystemReleaseStatus>("TicketingSystemConfigurationID", this.ID)];
    }

    /// <inheritdoc/>
    internal TicketingSystemConfiguration(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        IsActive = (bool)r["IsActive"];
        Url = r["Url"] as string;
        Type = r["Type"] as string;
        Name = r["Name"] as string;
        DataAccessCredentials_ID = ObjectToNullableInt(r["DataAccessCredentials_ID"]);
    }

}