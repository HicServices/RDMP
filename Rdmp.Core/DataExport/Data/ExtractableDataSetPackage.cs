// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IExtractableDataSetPackage" />
public class ExtractableDataSetPackage : DatabaseEntity, IExtractableDataSetPackage
{
    #region Database Properties

    private string _name;
    private string _creator;
    private DateTime _creationDate;

    /// <summary>
    ///     Name for the collection of datasets (e.g. 'Core Datasets').
    /// </summary>
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Environment.UserName of the person who created the <see cref="ExtractableDataSetPackage" />
    /// </summary>
    public string Creator
    {
        get => _creator;
        set => SetField(ref _creator, value);
    }

    /// <summary>
    ///     When the <see cref="ExtractableDataSetPackage" /> was created
    /// </summary>
    public DateTime CreationDate
    {
        get => _creationDate;
        set => SetField(ref _creationDate, value);
    }

    #endregion


    public ExtractableDataSetPackage()
    {
    }

    /// <summary>
    ///     Reads an <see cref="ExtractableDataSetPackage" /> out of the data export database
    /// </summary>
    /// <param name="dataExportRepository"></param>
    /// <param name="r"></param>
    public ExtractableDataSetPackage(IDataExportRepository dataExportRepository, DbDataReader r)
        : base(dataExportRepository, r)
    {
        Name = r["Name"].ToString();
        Creator = r["Creator"].ToString();
        CreationDate = Convert.ToDateTime(r["CreationDate"]);
    }


    /// <summary>
    ///     Creates a new <see cref="ExtractableDataSetPackage" /> in the data export database with the supplied
    ///     <paramref name="name" />
    /// </summary>
    /// <param name="dataExportRepository"></param>
    /// <param name="name"></param>
    public ExtractableDataSetPackage(IDataExportRepository dataExportRepository, string name)
    {
        dataExportRepository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "Creator", Environment.UserName },
            { "CreationDate", DateTime.Now }
        });
    }

    /// <summary>
    ///     Returns <see cref="Name" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Name;
    }
}