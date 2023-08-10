// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class GetDatabaseDiagramBinaryTest : DatabaseTests
{
    [Test]
    public void GetBinaryText()
    {
        using var con = CatalogueTableRepository.GetConnection();
        using var cmd = DatabaseCommandHelper.GetCommand(
            "SELECT definition  FROM sysdiagrams where name = 'Catalogue_Data_Diagram' ",
            con.Connection, con.Transaction);
        using var reader = cmd.ExecuteReader();
        //The system diagram exists
        Assert.IsTrue(reader.Read());

        var bytes = (byte[])reader[0];
        var bytesAsString = ByteArrayToString(bytes);

        Console.WriteLine(bytesAsString);
        Assert.Greater(bytesAsString.Length, 100000);
    }

    public static string ByteArrayToString(byte[] ba)
    {
        var hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
}