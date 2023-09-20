// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using Rdmp.Core.Reports.DublinCore;

namespace Rdmp.UI.Tests.CommandExecution;

internal class ExecuteCommandImportDublinCoreFormatTests
{
    /// <summary>
    /// This test also appears in the Rdmp.Tests project since it behaves differently in different runtime.
    /// </summary>
    [Test]
    public void Test_DublinCore_WriteReadFile_Net461()
    {
        var def1 = new DublinCoreDefinition
        {
            Title = "ssssshh",
            Alternative = "O'Rly",
            Description = "Description of stuff",
            Format = "text/html",
            Identifier = new Uri("http://foo.com"),
            Publisher = "University of Dundee",
            IsPartOf = new Uri("http://foo2.com"),
            Modified = new DateTime(2001, 1, 1),
            Subject = "Interesting, PayAttention, HighPriority, Omg"
        };

        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "dublin.xml"));

        using (var outStream = fi.OpenWrite())
        {
            def1.WriteXml(outStream);
        }

        using var inStream = fi.OpenRead();
        var def2 = new DublinCoreDefinition();
        var doc = XDocument.Load(inStream);
        def2.LoadFrom(doc.Root);
    }
}