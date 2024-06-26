// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class LoadProgressTest : DatabaseTests
{
    [Test]
    public void CreateNewScheduleTest()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var loadProgress = new LoadProgress(CatalogueRepository, loadMetadata);

        Assert.That(loadMetadata.ID, Is.EqualTo(loadProgress.LoadMetadata_ID));

        loadProgress.DeleteInDatabase();
        loadMetadata.DeleteInDatabase();
    }

    /// <summary>
    /// This tests that when fetching 2 copies (references) to the same database
    /// record that the instances are considered 'Equal' (for purposes of <see cref="object.Equals(object?)"/>)
    /// </summary>
    [Test]
    public void LoadProgress_Equals()
    {
        // only applies to databases
        if (CatalogueRepository is not TableRepository)
            return;

        var loadMetadata = new LoadMetadata(CatalogueRepository);

        var progress = new LoadProgress(CatalogueRepository, loadMetadata);
        var progressCopy = CatalogueRepository.GetObjectByID<LoadProgress>(progress.ID);

        progressCopy.Name = "fish";
        progressCopy.OriginDate = new DateTime(2001, 01, 01);

        try
        {
            Assert.Multiple(() =>
            {
                //values are different
                Assert.That(progress.OriginDate, Is.Not.EqualTo(progressCopy.OriginDate));
                Assert.That(progress.Name, Is.Not.EqualTo(progressCopy.Name));

                //IDs are the same
                Assert.That(progress.ID, Is.EqualTo(progressCopy.ID));

                //therefore objects are the same
                Assert.That(progressCopy, Is.EqualTo(progress));
            });
        }
        finally
        {
            progress.DeleteInDatabase();
            loadMetadata.DeleteInDatabase();
        }
    }
}