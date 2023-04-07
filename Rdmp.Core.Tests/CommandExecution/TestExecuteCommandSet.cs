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
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

class TestExecuteCommandSet : CommandCliTests
{
    [Test]
    public void Test_CatalogueDescription_Normal()
    {
        var cata = new Catalogue(Repository.CatalogueRepository, "Bob");

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","Some long description"}, GetActivator()));

        cata.RevertToDatabaseState();
        Assert.AreEqual("Some long description",cata.Description);

    }

    [Test]
    public void Test_CatalogueDescription_Null()
    {
        var cata = new Catalogue(Repository.CatalogueRepository, "Bob");
        cata.Description = "something cool";
        cata.SaveToDatabase();

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","NULL"}, GetActivator()));

        cata.RevertToDatabaseState();
        Assert.IsNull(cata.Description);

    }

    [Test]
    public void TestExecuteCommandSet_SetArrayValueFromCLI()
    {
        var pta = WhenIHaveA<ProcessTaskArgument>();
        pta.SetType(typeof(TableInfo[]));
        pta.Name = "TablesToIsolate";
        pta.SaveToDatabase();

        var t1 = WhenIHaveA<TableInfo>();
        var t2 = WhenIHaveA<TableInfo>();
        var t3 = WhenIHaveA<TableInfo>();
        var t4 = WhenIHaveA<TableInfo>();

        var ids = t1.ID + "," + t2.ID + "," + t3.ID + "," + t4.ID;

        Assert.IsNull(pta.Value);
        Assert.IsNull(pta.GetValueAsSystemType());

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"ProcessTaskArgument:TablesToIsolate" ,"Value",ids}, GetActivator()));

        Assert.AreEqual(ids,pta.Value);

        Assert.Contains(t1,(TableInfo[])pta.GetValueAsSystemType());
        Assert.Contains(t2,(TableInfo[])pta.GetValueAsSystemType());
        Assert.Contains(t3,(TableInfo[])pta.GetValueAsSystemType());
        Assert.Contains(t4,(TableInfo[])pta.GetValueAsSystemType());
    }
}