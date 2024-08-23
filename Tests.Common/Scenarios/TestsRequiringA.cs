// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using SynthEHR;
using SynthEHR.Datasets;
using FAnsi.Discovery;
using FAnsi.Discovery.TableCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Tests.Common.Scenarios;

/// <summary>
/// Base class for all tests that need a lot of objects created for them (e.g. a <see cref="Catalogue"/> with a <see cref="LoadMetadata"/>
/// </summary>
public abstract class TestsRequiringA : FromToDatabaseTests, IDatabaseColumnRequestAdjuster
{
    public void AdjustColumns(List<DatabaseColumnRequest> columns)
    {
        //create string columns as varchar(500) to avoid load errors  when creating new csv files you want to load into the database
        foreach (var c in columns.Where(c =>
                     c.TypeRequested.CSharpType == typeof(string) && c.TypeRequested.Width.HasValue))
            c.TypeRequested.Width = Math.Max(500, c.TypeRequested.Width.Value);
    }

    protected DiscoveredTable CreateDataset<T>(DiscoveredDatabase db, int people, int rows, Random r,
        out PersonCollection peopleGenerated) where T : IDataGenerator
    {
        peopleGenerated = new PersonCollection();
        peopleGenerated.GeneratePeople(people, r);
        return CreateDataset<T>(db, peopleGenerated, rows, r);
    }

    protected DiscoveredTable CreateDataset<T>(DiscoveredDatabase db, int people, int rows, Random r)
        where T : IDataGenerator => CreateDataset<T>(db, people, rows, r, out _);

    protected DiscoveredTable CreateDataset<T>(DiscoveredDatabase db, PersonCollection people, int rows, Random r)
        where T : IDataGenerator
    {
        var instance = DataGeneratorFactory.Create<T>(r);

        var dt = instance.GetDataTable(people, rows);

        return db.CreateTable(typeof(T).Name, dt, null, false, this);
    }
}