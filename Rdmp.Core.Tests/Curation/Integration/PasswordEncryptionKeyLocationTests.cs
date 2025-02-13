// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Managers;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class PasswordEncryptionKeyLocationTests : DatabaseTests
{
    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);

        if (keyLocation.GetKeyFileLocation() != null)
            Assert.Inconclusive();
    }

    [Test]
    public void NoKeyFileToStartWith()
    {
        var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);

        //there shouldn't already be a key
        Assert.That(keyLocation.GetKeyFileLocation(), Is.Null);

        var e = Assert.Throws<NotSupportedException>(keyLocation.DeleteKey);
        Assert.That(e.Message, Is.EqualTo("Cannot delete key because there is no key file configured"));
    }

    [Test]
    public void CreateKeyFile()
    {
        var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);
        var file = keyLocation.CreateNewKeyFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "my.key"));

        Console.WriteLine($"Key file location is:{file.FullName}");
        Console.WriteLine($"Text put into file is:{Environment.NewLine}{File.ReadAllText(file.FullName)}");

        Assert.Multiple(() =>
        {
            Assert.That(file.FullName, Does.EndWith("my.key"));

            Assert.That(keyLocation.GetKeyFileLocation(), Is.EqualTo(file.FullName));
        });
        keyLocation.DeleteKey();

        Assert.That(keyLocation.GetKeyFileLocation(), Is.Null);
    }

    [Test]
    public void Encrypt()
    {
        const string value = "MyPieceOfText";

        Console.WriteLine($"String is:{value}");

        var encrypter = new EncryptedString(CatalogueRepository);
        Assert.That(encrypter.IsStringEncrypted(value), Is.False);

        //should do pass through encryption
        encrypter.Value = value;
        Assert.Multiple(() =>
        {
            Assert.That(encrypter.Value, Is.Not.EqualTo(value));
            Assert.That(encrypter.GetDecryptedValue(), Is.EqualTo(value));
        });

        Console.WriteLine($"Encrypted (stock) is:{encrypter.Value}");
        Console.WriteLine($"Decrypted (stock) is:{encrypter.GetDecryptedValue()}");

        var keyLocation = new PasswordEncryptionKeyLocation(CatalogueRepository);
        keyLocation.CreateNewKeyFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "my.key"));
        var p = keyLocation.OpenKeyFile();

        CatalogueRepository.EncryptionManager.ClearAllInjections();

        var s = CatalogueRepository.EncryptionManager.GetEncrypter();
        var exception = Assert.Throws<CryptographicException>(() => s.Decrypt(encrypter.Value));
        Assert.That(exception.Message, Does.StartWith("Could not decrypt an encrypted string, possibly you are trying to decrypt it after having changed the PrivateKey "));

        var encrypted = s.Encrypt(value);
        Console.WriteLine($"Encrypted (with key) is:{encrypted}");
        Console.WriteLine($"Decrypted (with key) is:{s.Decrypt(encrypted)}");

        Assert.That(encrypter.IsStringEncrypted(encrypted));

        keyLocation.DeleteKey();
    }
}