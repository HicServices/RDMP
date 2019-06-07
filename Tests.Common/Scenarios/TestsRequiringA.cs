// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using BadMedicine;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Tests.Common.Scenarios
{
    /// <summary>
    /// Base class for all tests that need a lot of objects created for them (e.g. a <see cref="Catalogue"/> with a <see cref="LoadMetadata"/>
    /// </summary>
    public abstract class TestsRequiringA : DatabaseTests
    {
        protected DiscoveredTable CreateDataset<T>(int people, int rows,Random r, out PersonCollection peopleGenerated) where T:IDataGenerator
        {
            var f = new DataGeneratorFactory();
            T instance = f.Create<T>(r);

            peopleGenerated = new PersonCollection();
            peopleGenerated.GeneratePeople(people,r);

            var dt = instance.GetDataTable(peopleGenerated,rows);

            return DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable(typeof(T).Name,dt);            
        }
        protected DiscoveredTable CreateDataset<T>(int people, int rows,Random r) where T:IDataGenerator
        {
            return CreateDataset<T>(people,rows,r,out _);
        }
    }
}
