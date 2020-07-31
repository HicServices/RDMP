// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using System;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class LoadProgressUnitTests : UnitTests
    {
        [Test]
        public void LoadProgress_Checks_BadDates()
        {
            var lp = WhenIHaveA<LoadProgress>();

            lp.Check(new ThrowImmediatelyCheckNotifier());

            //Bad Origin Date
            lp.OriginDate = DateTime.Now.AddDays(1);
            Assert.Throws<Exception>(()=>lp.Check(new ThrowImmediatelyCheckNotifier()));

            //Back to normal
            lp.RevertToDatabaseState();
            lp.Check(new ThrowImmediatelyCheckNotifier());

            //Bad ProgressDate
            lp.DataLoadProgress = DateTime.Now.AddDays(1);
            Assert.Throws<Exception>(()=>lp.Check(new ThrowImmediatelyCheckNotifier()));
                        
            //Back to normal
            lp.RevertToDatabaseState();
            lp.Check(new ThrowImmediatelyCheckNotifier());
            

            //negative progress
            lp.OriginDate = new DateTime(2001,1,1);
            lp.DataLoadProgress = new DateTime(2000,1,1);
            Assert.Throws<Exception>(()=>lp.Check(new ThrowImmediatelyCheckNotifier()));

            // valid progress (1 year)
            lp.OriginDate = new DateTime(2001,1,1);
            lp.DataLoadProgress = new DateTime(2002,1,1);
            lp.Check(new ThrowImmediatelyCheckNotifier());

            
            
        }
    }
}
