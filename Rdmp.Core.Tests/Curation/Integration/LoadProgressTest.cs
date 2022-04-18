// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{

    public class LoadProgressTest : DatabaseTests
    {
        [Test]
        public void CreateNewScheduleTest()
        {
            var loadMetadata = new LoadMetadata(CatalogueRepository);
            var loadProgress = new LoadProgress(CatalogueRepository, loadMetadata);

            Assert.AreEqual(loadProgress.LoadMetadata_ID, loadMetadata.ID);

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

            LoadProgress progress = new LoadProgress(CatalogueRepository, loadMetadata);
            LoadProgress progressCopy = CatalogueRepository.GetObjectByID<LoadProgress>(progress.ID);
            
            progressCopy.Name = "fish";
            progressCopy.OriginDate = new DateTime(2001,01,01);
            
            try
            {
                //values are different
                Assert.AreNotEqual(progressCopy.OriginDate, progress.OriginDate);
                Assert.AreNotEqual(progressCopy.Name, progress.Name);

                //IDs are the same
                Assert.AreEqual(progressCopy.ID, progress.ID);

                //therefore objects are the same
                Assert.IsTrue(progressCopy.Equals(progress));

            }
            finally
            {
                progress.DeleteInDatabase();
                loadMetadata.DeleteInDatabase();
            }
        }
    }
}
