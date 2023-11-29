// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

[Category("Unit")]
internal class RowPeekerTests
{
        [Test]
        public void Peeker()
        {
                using var dt = new DataTable();
                dt.Columns.Add("MyCol");
                dt.Rows.Add("fish");
                dt.Rows.Add("dish");
                dt.Rows.Add("splish");

                var mock = Substitute.For<IDbDataCommandDataFlowSource>();
                mock.ReadOneRow()
                    .Returns(dt.Rows[0],
                    dt.Rows[1],
                    dt.Rows[2],
                    null);

                var p = new RowPeeker();
                using var dt2 = new DataTable();
                dt2.Columns.Add("MyCol");

                //Reads fish and peeks dish
                p.AddWhile(mock, r => (string)r["MyCol"] == "fish", dt2);

        //read one row
        Assert.That(dt2.Rows, Has.Count.EqualTo(1));
        Assert.That(dt2.Rows[0]["MyCol"], Is.EqualTo("fish"));

                using var dt3 = new DataTable();
                dt3.Columns.Add("MyCol");

                //cannot add while there is a peek stored
                Assert.Throws<Exception>(() => p.AddWhile(mock, r => (string)r["MyCol"] == "fish", dt2));

                //clear the peek
                //unpeeks dish
                p.AddPeekedRowsIfAny(dt3);
        Assert.That(dt3.Rows, Has.Count.EqualTo(1));
        Assert.That(dt3.Rows[0]["MyCol"], Is.EqualTo("dish"));

                //now we can read into dt4 but the condition is false
                //Reads nothing but peeks splish
                using var dt4 = new DataTable();
                dt4.Columns.Add("MyCol");
                p.AddWhile(mock, r => (string)r["MyCol"] == "fish", dt4);

        Assert.That(dt4.Rows, Is.Empty);

                //we passed a null chunk and that pulls back the legit data table
                var dt5 = p.AddPeekedRowsIfAny(null);

        Assert.That(dt5, Is.Not.Null);
        Assert.That(dt5.Rows[0]["MyCol"], Is.EqualTo("splish"));

                using var dt6 = new DataTable();
                dt6.Columns.Add("MyCol");
                p.AddWhile(mock, r => (string)r["MyCol"] == "fish", dt6);

        Assert.That(dt6.Rows, Is.Empty);
        }
}