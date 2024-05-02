// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Curation.Data.Governance;

/// <summary>
///     A GovernancePeriod is used to track the fact that a given set of datasets requires external approval for your
///     agency to hold.  This is not the same as releasing data
///     to researchers or researcher approval to get specific extracts from you.  Governance Periods are concerned only
///     with your agency and its ability to hold datasets.  A
///     GovernancePeriod starts at a specific date and can optionally expire.  A GovernancePeriod relates to one or more
///     Catalogues but Catalogues can have multiple GovernancePeriods
///     e.g. if you require to get approval from 2 different external agencies to hold a specific dataset.
///     <para>
///         GovernancePeriods are entirely optional, you can happily get by without configuring any for any of your
///         Catalogues.  However once you have configured a GovernancePeriod for a
///         specific Catalogue once then it will always require governance and be reported as Governance Expired in the
///         Dashboard once its GovernancePeriod has expired.
///     </para>
///     <para>
///         The correct usage of GovernancePeriods is to never delete them e.g. your dataset MyDataset1 would have
///         Governacne 2001-2002 (with attachment
///         letters of approval) and another one for 2003-2004 and another from 2005 onwards etc.
///     </para>
/// </summary>
public class GovernancePeriod : DatabaseEntity, ICheckable, INamed
{
    private readonly IGovernanceManager _manager;

    #region Database Properties

    private DateTime _startDate;
    private DateTime? _endDate;
    private string _name;
    private string _description;
    private string _ticket;


    /// <summary>
    ///     When did the governance come into effect (in realtime not dataset time)
    /// </summary>
    public DateTime StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    /// <summary>
    ///     Does governane for the described datasets ever expire (e.g. if you need to get annual approval for holding
    ///     datasets)
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    /// <inheritdoc />
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Who gave the governance and what it covers in human readable broad terms
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///     <see cref="ITicketingSystem" /> ticket number for tracking effort / progress towards obtaining the governance
    /// </summary>
    public string Ticket
    {
        get => _ticket;
        set => SetField(ref _ticket, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    ///     All documents (emails sent, pdfs, letters of permission etc) that were involved in obtaining and which grant
    ///     permission to hold the datasets described by the
    ///     <see cref="GovernancePeriod" />
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<GovernanceDocument> GovernanceDocuments =>
        Repository.GetAllObjectsWithParent<GovernanceDocument>(this);

    /// <summary>
    ///     All datasets to which this governance grants permission to hold
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<ICatalogue> GovernedCatalogues => _manager.GetAllGovernedCatalogues(this);

    #endregion

    public GovernancePeriod()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="GovernancePeriod" /> in the database.  This grants (ethical) permission to hold datasets
    ///     referenced by <see cref="GovernedCatalogues" />.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public GovernancePeriod(ICatalogueRepository repository, string name = null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name ?? $"GovernancePeriod{Guid.NewGuid()}" },
            { "StartDate", DateTime.Now.Date }
        });

        _manager = CatalogueRepository.GovernanceManager;
    }

    internal GovernancePeriod(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        //cannot be null
        Name = r["Name"].ToString();
        StartDate = Convert.ToDateTime(r["StartDate"]);

        //can be null
        Ticket = r["Ticket"] as string;
        EndDate = ObjectToNullableDateTime(r["EndDate"]);
        Description = r["Description"] as string;

        _manager = CatalogueRepository.GovernanceManager;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Checks that the governance has not expired before it began etc
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (EndDate == null)
            notifier.OnCheckPerformed(new CheckEventArgs($"There is no end date for GovernancePeriod {Name}",
                CheckResult.Warning));
        else if (EndDate <= StartDate)
            notifier.OnCheckPerformed(new CheckEventArgs($"GovernancePeriod {Name} expires before it begins!",
                CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs($"GovernancePeriod {Name} expiry date is after the start date",
                CheckResult.Success));

        foreach (var doc in GovernanceDocuments)
            doc.Check(notifier);
    }

    /// <summary>
    ///     Marks the given <see cref="Catalogue" /> as no longer requiring governance approval from this
    ///     <see cref="GovernancePeriod" />.
    /// </summary>
    /// <param name="c"></param>
    public void DeleteGovernanceRelationshipTo(ICatalogue c)
    {
        _manager.Unlink(this, c);
    }

    /// <summary>
    ///     Declares that the given <see cref="Catalogue" /> requires governance to hold and that this
    ///     <see cref="GovernancePeriod" /> describes the specifics
    ///     as well as any <see cref="EndDate" /> to the governance.
    ///     <para>
    ///         A <see cref="Catalogue" /> belonging to 0 <see cref="GovernancePeriod" /> is not assumed to require any
    ///         governance.  A <see cref="Catalogue" /> can
    ///         belong to multiple <see cref="GovernancePeriod" /> e.g. 'Tayside Governance 2001', 'Tayside Governance 2002'
    ///         etc
    ///     </para>
    /// </summary>
    /// <param name="c"></param>
    public void CreateGovernanceRelationshipTo(ICatalogue c)
    {
        _manager.Link(this, c);
    }

    /// <summary>
    ///     True if the current date is after the <see cref="EndDate" /> (if there is one)
    /// </summary>
    /// <returns></returns>
    public bool IsExpired()
    {
        return EndDate != null && DateTime.Now.Date > EndDate.Value.Date;
    }
}