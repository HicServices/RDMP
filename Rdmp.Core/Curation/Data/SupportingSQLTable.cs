// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes an SQL query that can be run to generate useful information for the understanding of a given Catalogue
///     (dataset).  If it is marked as
///     Extractable then it will be bundled along with the Catalogue every time it is extracted.  This can be used as an
///     alternative to definining Lookups
///     through the Lookup class or to extract other useful administrative data etc to be provided to researchers
///     <para>
///         It is VITAL that you do not use this as a method of extracting sensitive/patient data as this data is run as is
///         and is not joined against a cohort
///         or anonymised in anyway.
///     </para>
///     <para>
///         If the Global flag is set then the SQL will be run and the result provided to every researcher regardless of
///         what datasets they have asked for in
///         an extraction, this is useful for large lookups like ICD / SNOMED CT which are likely to be used by many
///         datasets.
///     </para>
/// </summary>
public class SupportingSQLTable : DatabaseEntity, INamed, ISupportingObject
{
    /// <summary>
    ///     The subfolder of extractions in which to put <see cref="Extractable" /> <see cref="SupportingSQLTable" /> when
    ///     doing project extractions
    /// </summary>
    public const string ExtractionFolderName = "SupportingDataTables";

    #region Database Properties

    private int _catalogue_ID;
    private string _description;
    private string _name;
    private string _sQL;
    private bool _extractable;
    private int? _externalDatabaseServer_ID;
    private string _ticket;
    private bool _isGlobal;

    /// <summary>
    ///     The dataset the <see cref="SupportingSQLTable" /> helps you to understand
    /// </summary>
    public int Catalogue_ID
    {
        get => _catalogue_ID;
        set => SetField(ref _catalogue_ID, value);
    }

    /// <summary>
    ///     Human readable description of what the table referenced by <see cref="SQL" /> contains
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <inheritdoc />
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Sql to execute on the server to return the supplemental table that helps with understanding/interpreting
    ///     <see cref="Catalogue_ID" />
    /// </summary>
    public string SQL
    {
        get => _sQL;
        set => SetField(ref _sQL, value);
    }

    /// <summary>
    ///     If true then the query will be executed and the resulting table will be copied to the output directory of project
    ///     extractions that include the <see cref="Catalogue_ID" />.
    ///     <para>
    ///         This will fail if the query contains mulitple select statements.  Ensure that there is no identifiable data
    ///         returned by the query since it will not be linked
    ///         against the project cohort / anonymised in any way.
    ///     </para>
    /// </summary>
    public bool Extractable
    {
        get => _extractable;
        set => SetField(ref _extractable, value);
    }

    /// <summary>
    ///     The server on which the <see cref="SQL" /> should be executed on
    /// </summary>
    public int? ExternalDatabaseServer_ID
    {
        get => _externalDatabaseServer_ID;
        set => SetField(ref _externalDatabaseServer_ID, value);
    }

    /// <summary>
    ///     <see cref="ITicketingSystem" /> identifier of a ticket for logging time curating / updating etc the table
    /// </summary>
    public string Ticket
    {
        get => _ticket;
        set => SetField(ref _ticket, value);
    }

    /// <summary>
    ///     If <see cref="Extractable" />  and <see cref="IsGlobal" /> then the table will be copied to the ouptut directory of
    ///     all project extractions
    ///     regardless of whether or not the <see cref="Catalogue_ID" /> dataset is included in the extraction
    /// </summary>
    public bool IsGlobal
    {
        get => _isGlobal;
        set => SetField(ref _isGlobal, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="Catalogue_ID" />
    [NoMappingToDatabase]
    public Catalogue Catalogue => Repository.GetObjectByID<Catalogue>(Catalogue_ID);

    /// <inheritdoc cref="ExternalDatabaseServer_ID" />
    [NoMappingToDatabase]
    public ExternalDatabaseServer ExternalDatabaseServer => ExternalDatabaseServer_ID == null
        ? null
        : Repository.GetObjectByID<ExternalDatabaseServer>((int)ExternalDatabaseServer_ID);

    #endregion

    public SupportingSQLTable()
    {
    }

    /// <summary>
    ///     Defines a new table that helps in understanding the given dataset <paramref name="parent" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    public SupportingSQLTable(ICatalogueRepository repository, ICatalogue parent, string name)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "Catalogue_ID", parent.ID }
        });
    }

    internal SupportingSQLTable(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString());
        Description = r["Description"] as string;
        Name = r["Name"] as string;
        Extractable = (bool)r["Extractable"];
        IsGlobal = (bool)r["IsGlobal"];
        SQL = r["SQL"] as string;

        if (r["ExternalDatabaseServer_ID"] == null || r["ExternalDatabaseServer_ID"] == DBNull.Value)
            ExternalDatabaseServer_ID = null;
        else
            ExternalDatabaseServer_ID = Convert.ToInt32(r["ExternalDatabaseServer_ID"]);

        Ticket = r["Ticket"] as string;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Returns the decrypted connection string you can use to access the data (fetched from ExternalDatabaseServer_ID -
    ///     which can be null).  If there is no
    ///     ExternalDatabaseServer_ID associated with the SupportingSQLTable then a NotSupportedException will be thrown
    /// </summary>
    /// <returns></returns>
    public DiscoveredServer GetServer()
    {
        return ExternalDatabaseServer_ID == null
            ? throw new NotSupportedException(
                $"No external database server has been selected for SupportingSQL table called :{ToString()} (ID={ID}).  The SupportingSQLTable currently belongs to Catalogue {Catalogue.Name}")
            : ExternalDatabaseServer.Discover(DataAccessContext.DataExport).Server;
    }
}

/// <summary>
///     Identifies which collection of extractable resources should be returned from the database
/// </summary>
public enum FetchOptions
{
    /// <summary>
    ///     All resources
    /// </summary>
    AllGlobalsAndAllLocals,

    /// <summary>
    ///     Global resources only
    /// </summary>
    AllGlobals,

    /// <summary>
    ///     Non Global resources only
    /// </summary>
    AllLocals,

    /// <summary>
    ///     Global resources only AND only if they are marked Extractable
    /// </summary>
    ExtractableGlobals,

    /// <summary>
    ///     Non Global resources only AND only if they are marked Extractable
    /// </summary>
    ExtractableLocals,

    /// <summary>
    ///     All resources that are marked Extractable
    /// </summary>
    ExtractableGlobalsAndLocals
}