// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Helper class for becomming an IEncryptedPasswordHost via SimpleStringValueEncryption.  This class needs an ICatalogueRepository because
/// SimpleStringValueEncryption is only secure when there is a private RSA encryption key specified in the CatalogueRepository.  This key
/// certificate will be a file location.  This allows you to use windows file system based user authentication to securely encrypt strings
/// within RDMP databases.
/// 
/// <para>See also PasswordEncryptionKeyLocationUI</para>
/// </summary>
public class EncryptedPasswordHost : IEncryptedPasswordHost
{
    /// <summary>
    /// This is only to support XML de-serialization
    /// </summary>
    internal class FakeEncryptedString : IEncryptedString
    {
        public string Value { get; set; }
        public string GetDecryptedValue() => Value;

        public bool IsStringEncrypted(string value) => false;
    }

    private IEncryptedString _encryptedString;

    /// <summary>
    /// For XML serialization
    /// </summary>
    public EncryptedPasswordHost()
    {
        // This is to get around the issue where during de-serialization we cannot create an EncryptedString because there is no access to a repository.
        // If there is not a valid _encryptedString then de-serialization will fail (_encryptedString.Value is needed).
        // This provides an implementation of IEncryptedString which is only valid for deserializing the encrypted password from an XML representation and providing the encrypted password to a 'real' EncryptedPasswordHost
        _encryptedString = new FakeEncryptedString();
    }

    /// <summary>
    /// Prepares the object for decrypting/encrypting passwords based on the <see cref="Repositories.Managers.PasswordEncryptionKeyLocation"/>
    /// </summary>
    /// <param name="repository"></param>
    public EncryptedPasswordHost(ICatalogueRepository repository)
    {
        _encryptedString = new EncryptedString(repository);
    }

    /// <summary>
    /// Updates the encryption method to use a real encryption strategy.  Should be called
    /// after deserialization and only if the blank constructor was used.
    /// </summary>
    /// <param name="repository"></param>
    public void SetRepository(ICatalogueRepository repository)
    {
        if (_encryptedString is FakeEncryptedString f)
            _encryptedString = new EncryptedString(repository)
            {
                Value = f.Value
            };
    }

    /// <inheritdoc/>
    public string Password
    {
        get =>
            _encryptedString is FakeEncryptedString
                ? throw new Exception(
                    $"Encryption setup failed, API caller must have forgotten to call {nameof(SetRepository)}")
                : _encryptedString.Value;
        set => _encryptedString.Value = value;
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    public string GetDecryptedPassword()
    {
        if (_encryptedString != null)
        {
            var value = _encryptedString.GetDecryptedValue();
            Console.WriteLine(value);
        }
        return _encryptedString == null
            ? throw new Exception(
                $"Passwords cannot be decrypted until {nameof(SetRepository)} has been called and decryption strategy is established")
            : _encryptedString.GetDecryptedValue() ?? "";
    }
}