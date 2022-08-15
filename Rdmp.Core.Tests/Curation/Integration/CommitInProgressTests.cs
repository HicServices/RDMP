// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
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

            var start = new CommitInProgress(RepositoryLocator, c);

            // no changes but lets spam this for added complexity
            c.SaveToDatabase();
            c.SaveToDatabase();
            c.SaveToDatabase();

            Assert.IsNull(start.Finish(),"No changes made to Catalogue so expected no commit");

            c.Name = "abadaba";
            c.IsDeprecated = true;
            c.SaveToDatabase();

            var commit = start.Finish();
            Assert.IsNotNull(commit);

            Assert.AreEqual(1, commit.Mementos.Length);
            Assert.AreNotEqual(commit.Mementos[0].BeforeYaml, commit.Mementos[0].AfterYaml);
        }
    }
}
    