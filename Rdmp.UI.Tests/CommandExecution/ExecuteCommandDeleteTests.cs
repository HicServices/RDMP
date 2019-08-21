// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.UI.Tests.CommandExecution
{
    class ExecuteCommandDeleteTests : UITests
    {

        /// <summary>
        /// RDMPDEV-1551 Tests system behaviour when user selects a <see cref="CatalogueItem"/> and the <see cref="ExtractionInformation"/>
        /// that is the immediate child of it and issues a multi delete.  The first delete will also CASCADE delete the other
        /// </summary>
        [Test, UITimeout(50000)]
        public void TestDeleteMultiple_OneObjectInheritsAnother()
        {
            var ei = WhenIHaveA<ExtractionInformation>();
            var ci = ei.CatalogueItem;

            ItemActivator.YesNoResponse = true;

            Assert.IsTrue(ci.Exists());
            Assert.IsTrue(ei.Exists());

            //now because we don't actually have a CASCADE in memory we will have to fake it
            ei.DeleteInDatabase();

            var cmd = new ExecuteCommandDelete(ItemActivator, new IDeleteable[] { ci, ei });
            cmd.Execute();

            Assert.IsFalse(ci.Exists());
            Assert.IsFalse(ei.Exists());
        }

    }
}
