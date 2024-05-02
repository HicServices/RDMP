// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Security.Cryptography;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Encrypts a string, providing access to both the encrypted and decrypted values.
/// </summary>
/// <exception cref="InvalidOperationException">Value is too long to be encrypted</exception>
/// <exception cref="CryptographicException" />
public class EncryptedString : IEncryptedString
{
    private readonly IEncryptStrings _encrypter;
    private string _value;

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    /// <inheritdoc />
    public string Value
    {
        get =>
            //if there is a password in memory it will be encrypted (probably) so return that, to decrypt call DecryptPassword
            _value;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) //if it is null
                _value = null;
            else if (!_encrypter.IsStringEncrypted(value)) //it is not null, is it already encrypted?
                try
                {
                    _value = _encrypter.Encrypt(value); //not yet encrypted so encrypt it
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Bad Length") || e.Message.Contains("data too large for key size"))
                        throw new InvalidOperationException(
                            $"The free text Value supplied to this class was too long to be encrypted (Length of string was {value.Length})",
                            e);

                    //it's some other exception
                    throw;
                }
            else
                _value = value; //it is encrypted already so just store in normally
        }
    }

    /// <summary>
    ///     Creates a new encrypted string using <see cref="SimpleStringValueEncryption" />
    /// </summary>
    /// <param name="repository"></param>
    public EncryptedString(ICatalogueRepository repository)
    {
        _encrypter = repository.EncryptionManager.GetEncrypter();
    }

    /// <inheritdoc />
    public string GetDecryptedValue()
    {
        if (string.IsNullOrWhiteSpace(Value))
            return null;

        if (_encrypter.IsStringEncrypted(Value))
            return _encrypter.Decrypt(Value);

        //it's not decrypted... how did that happen
        throw new Exception("Found Value in memory that was not encrypted");
    }

    /// <inheritdoc />
    public bool IsStringEncrypted(string value)
    {
        return _encrypter.IsStringEncrypted(value);
    }
}