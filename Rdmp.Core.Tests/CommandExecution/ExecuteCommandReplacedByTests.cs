// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandReplacedByTests : CommandCliTests
    {
        [Test]
        public void CommandImpossible_BecauseNotDeprecated()
        {
            var c1 = WhenIHaveA<Catalogue>();
            var c2 = WhenIHaveA<Catalogue>();
            
            var cmd = new ExecuteCommandReplacedBy(GetMockActivator().Object,c1,c2);
            
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains("is not marked IsDeprecated",cmd.ReasonCommandImpossible);
        }

        [Test]
        public void CommandImpossible_BecauseDifferentTypes()
        {
            var c1 = WhenIHaveA<Catalogue>();
            var ci1 = WhenIHaveA<CatalogueItem>();
            
            c1.IsDeprecated = true;
            c1.SaveToDatabase();
            
            var cmd = new ExecuteCommandReplacedBy(GetMockActivator().Object,c1,ci1);
            
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains("because it is a different object Type",cmd.ReasonCommandImpossible);
        }
        [Test]
        public void CommandImpossible_Allowed()
        {
            var c1 = WhenIHaveA<Catalogue>();
            var c2 = WhenIHaveA<Catalogue>();
            
            c1.IsDeprecated = true;
            c1.SaveToDatabase();
            
            var cmd = new ExecuteCommandReplacedBy(GetMockActivator().Object,c1,c2);
            Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);

            cmd.Execute();

            var replacement = RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name",ExecuteCommandReplacedBy.ReplacedBy)
                    .Single(r=>r.IsReferenceTo(c1));

            Assert.IsTrue(replacement.IsReferenceTo(c1));
            Assert.AreEqual(c2.ID.ToString(),replacement.Value);

            // running command multiple times shouldn't result in duplicate objects
            cmd.Execute();
            cmd.Execute();
            cmd.Execute();
            cmd.Execute();

            Assert.AreEqual(1,RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name",ExecuteCommandReplacedBy.ReplacedBy)
                    .Count(r=>r.IsReferenceTo(c1)));

            cmd = new ExecuteCommandReplacedBy(GetMockActivator().Object,c1,null);
            cmd.Execute();

            Assert.IsEmpty(RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name",ExecuteCommandReplacedBy.ReplacedBy)
                    .Where(r=>r.IsReferenceTo(c1)));
           
        }
    }
}
