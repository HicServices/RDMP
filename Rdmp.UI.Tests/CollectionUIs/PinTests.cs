// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Collections;

namespace Rdmp.UI.Tests.CollectionUIs
{
    class PinTests : UITests
    {

        [Test,UITimeout(50000)]
        public void Test_DeletePinnedObject_PinClears()
        {
            var ti = WhenIHaveA<TableInfo>();

            var collection = AndLaunch<TableInfoCollectionUI>();
            
            //nothing selected to start with
            Assert.IsNull(collection.CommonTreeFunctionality.Tree.SelectedObject);
            
            //emphasise and pin it (like Ctrl+F to object)
            ItemActivator.RequestItemEmphasis(this,new EmphasiseRequest(ti){Pin = true });

            //the TableInfo should now be selected
            Assert.AreEqual(ti,collection.CommonTreeFunctionality.Tree.SelectedObject);

            //and pinned
            Assert.AreEqual(ti,collection.CommonTreeFunctionality.CurrentlyPinned);

            //delete the object
            ItemActivator.DeleteWithConfirmation(ti);

            //selection should now be cleared
            Assert.IsNull(collection.CommonTreeFunctionality.Tree.SelectedObject);

            //as should the pin
            Assert.IsNull(collection.CommonTreeFunctionality.CurrentlyPinned);

        }
    }
}
