// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.ANOEngineeringUIs;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class ANOTableUITests:UITests
    {
        [Test, UITimeout(5000)]
        public void Test_ANOTableUI_NormalState()
        {
            var anoTable = WhenIHaveA<ANOTable>();
            AndLaunch<ANOTableUI>(anoTable);
            AssertNoCrash();
        }

        [Test, UITimeout(5000)]
        public void Test_ANOTableUI_ServerWrongType()
        {
            ExternalDatabaseServer srv;
            var anoTable = WhenIHaveA<ANOTable>(out srv);
            srv.CreatedByAssembly = null;
            srv.SaveToDatabase();

            var ui = AndLaunch<ANOTableUI>(anoTable);
            
            //no exceptions
            AssertNoCrash();

            //but there should be an error on this UI element
            Assert.AreEqual("Server is not an ANO server", ui.ServerErrorProvider.GetError(ui.llServer));
        }

        
    }
}
