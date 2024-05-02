// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Governance;

/// <summary>
///     Contains the path to a useful file which reflects either a request or a granting of governance e.g. a letter from
///     your local healthboard authorising you to host/use 1 or more
///     datasets for a given period of time.  Also includes a name (which should really match the file name) and a
///     description which should be a plain summary of what is in the document
///     such that lay users can appreciate what the document contains/means for the system.
/// </summary>
public class GovernanceDocument : DatabaseEntity, INamed
{
    #region Database Properties

    private int _governancePeriodID;
    private string _name;
    private string _description;
    private string _url;

    /// <summary>
    ///     The <see cref="GovernancePeriod" /> for which this document is part of (either as a letter requesting approval,
    ///     granting approval etc)
    /// </summary>
    public int GovernancePeriod_ID
    {
        get => _governancePeriodID;
        private set => SetField(ref _governancePeriodID, value);
    } //every document belongs to only one period of governance knoweldge (via fk relationship)

    /// <inheritdoc />
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Human readable description/summary of the contents of the document, who sent it why it exists etc
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///     The location of the referenced document on disk
    /// </summary>
    [AdjustableLocation]
    public string URL
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    #endregion

    public GovernanceDocument()
    {
    }

    /// <summary>
    ///     Marks a given <paramref name="file" /> as being important in the obtaining of the <paramref name="parent" />
    ///     <see cref="GovernancePeriod" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="file"></param>
    public GovernanceDocument(ICatalogueRepository repository, GovernancePeriod parent, FileInfo file)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "GovernancePeriod_ID", parent.ID },
            { "URL", file.FullName },
            { "Name", file.Name }
        });
    }

    internal GovernanceDocument(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        //cannot be null
        Name = r["Name"].ToString();
        URL = r["URL"].ToString();

        GovernancePeriod_ID = Convert.ToInt32(r["GovernancePeriod_ID"]);

        //can be null
        Description = r["Description"] as string;
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Checks that the file exists
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        try
        {
            var fileInfo = new FileInfo(URL);

            if (fileInfo.Exists)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Found intact attachment file {fileInfo} with length {UsefulStuff.GetHumanReadableByteSize(fileInfo.Length)}",
                        CheckResult.Success));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"File {fileInfo.FullName} does not exist (for GovernanceDocument '{this}' (ID={ID})",
                        CheckResult.Fail));
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Failed to check for existence of the file described by GovernanceDocument '{this}' (ID={ID})",
                    CheckResult.Fail, ex));
        }
    }

    /// <summary>
    ///     Returns the name of the file (See also <see cref="URL" />)
    /// </summary>
    /// <returns></returns>
    public string GetFilenameOnly()
    {
        return Path.GetFileName(URL);
    }
}