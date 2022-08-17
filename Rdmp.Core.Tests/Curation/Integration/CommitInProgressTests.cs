// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class CommitInProgressTests : DatabaseTests
    {
        [Test]
        public void CommitInProgress_CatalogueModify()
        {
            var c = new Catalogue(CatalogueRepository, "Hey");

            using var start = new CommitInProgress(RepositoryLocator, c);

            // no changes but lets spam this for added complexity
            c.SaveToDatabase();
            c.SaveToDatabase();
            c.SaveToDatabase();
            
            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            
            Assert.IsNull(start.TryFinish(activator),"No changes made to Catalogue so expected no commit");

            c.Name = "abadaba";
            c.IsDeprecated = true;
            c.SaveToDatabase();

            var commit = start.TryFinish(activator);
            Assert.IsNotNull(commit);

            Assert.AreEqual(1, commit.Mementos.Length);
            Assert.AreNotEqual(commit.Mementos[0].BeforeYaml, commit.Mementos[0].AfterYaml);
        }

        /// <summary>
        /// Tests that when there is a <see cref="CommitInProgress"/> on object(s) e.g. <see cref="Catalogue"/>
        /// then any <see cref="ISaveable.SaveToDatabase"/> does not write to db until commit ends.        /// 
        /// </summary>
        [Test]
        public void CommitInProgress_TestSaveDelaying()
        {
            
            var c = new Catalogue(CatalogueRepository, "Hey");

            Assert.AreEqual(ChangeDescription.NoChanges,c.HasLocalChanges().Evaluation,
                "We just created this Catalogue, how can db copy be different?!");

            using var start = new CommitInProgress(RepositoryLocator, c);
            var activator = new ThrowImmediatelyActivator(RepositoryLocator);

            // there is a CommitInProgress on c so db should not have
            c.Name = "abadaba";
            c.IsDeprecated = true;

            Assert.AreEqual(ChangeDescription.DatabaseCopyDifferent, c.HasLocalChanges().Evaluation,
                "We have local changes");

            c.SaveToDatabase();

            Assert.AreEqual(ChangeDescription.DatabaseCopyDifferent, c.HasLocalChanges().Evaluation,
                "Since there is a transaction ongoing we should be suppressing writes to db");

            // finish the commit
            var commit = start.TryFinish(activator);
            Assert.IsNotNull(commit);

            Assert.AreEqual(ChangeDescription.NoChanges, c.HasLocalChanges().Evaluation,
                "Now that we finished the Commit all objects should be saved");
        }
    }
}
    