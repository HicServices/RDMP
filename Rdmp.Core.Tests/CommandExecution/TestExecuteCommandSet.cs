// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using NPOI.Util;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandSet : CommandCliTests
    {
        [Test]
        public void Test_CatalogueDescription_Normal()
        {
            var cata = new Catalogue(Repository.CatalogueRepository, "Bob");

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","Some long description"},RepositoryLocator));

            cata.RevertToDatabaseState();
            Assert.AreEqual("Some long description",cata.Description);

        }

        [Test]
        public void Test_CatalogueDescription_Null()
        {
            var cata = new Catalogue(Repository.CatalogueRepository, "Bob");
            cata.Description = "something cool";
            cata.SaveToDatabase();

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","NULL"},RepositoryLocator));

            cata.RevertToDatabaseState();
            Assert.IsNull(cata.Description);

        }
    }
}
