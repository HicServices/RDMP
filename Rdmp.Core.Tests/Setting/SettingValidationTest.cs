// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;

using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.Setting;

public class SettingValidationTest : DatabaseTests
{

    [Test]
    public void SettingManagement()
    {
        //create new setting
        var setting = new Core.Setting.Setting(RepositoryLocator.CatalogueRepository, "Key", "Value");
        Assert.DoesNotThrow(()=>setting.SaveToDatabase());
        var knownSettings = RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Setting.Setting>().ToList();
        Assert.That(knownSettings.Count, Is.EqualTo(1));
        Assert.That(knownSettings[0].Key, Is.EqualTo("Key"));
        Assert.That(knownSettings[0].Value, Is.EqualTo("Value"));
        //update value
        setting.Value = "OtherValue";
        Assert.DoesNotThrow(() => setting.SaveToDatabase());
        knownSettings = RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Setting.Setting>().ToList();
        Assert.That(knownSettings.Count, Is.EqualTo(1));
        Assert.That(knownSettings[0].Key, Is.EqualTo("Key"));
        Assert.That(knownSettings[0].Value, Is.EqualTo("OtherValue"));
        //add a second setting
        setting = new Core.Setting.Setting(RepositoryLocator.CatalogueRepository, "SecondKey", "SecondValue");
        Assert.DoesNotThrow(() => setting.SaveToDatabase());
        knownSettings = RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Setting.Setting>().ToList();
        Assert.That(knownSettings.Count, Is.EqualTo(2));
        Assert.That(knownSettings[0].Key, Is.EqualTo("Key"));
        Assert.That(knownSettings[0].Value, Is.EqualTo("OtherValue"));
        Assert.That(knownSettings[1].Key, Is.EqualTo("SecondKey"));
        Assert.That(knownSettings[1].Value, Is.EqualTo("SecondValue"));
        //create a key that already exists
        Assert.Throws<SqlException>(() => {
            setting = new Core.Setting.Setting(RepositoryLocator.CatalogueRepository, "Key", "Value");
        });
        Assert.That(knownSettings.Count, Is.EqualTo(2));
        Assert.That(knownSettings[0].Key, Is.EqualTo("Key"));
        Assert.That(knownSettings[0].Value, Is.EqualTo("OtherValue"));
        Assert.That(knownSettings[1].Key, Is.EqualTo("SecondKey"));
        Assert.That(knownSettings[1].Value, Is.EqualTo("SecondValue"));
        foreach(Core.Setting.Setting s in knownSettings)
        {
            s.DeleteInDatabase();
        }

    }
}
