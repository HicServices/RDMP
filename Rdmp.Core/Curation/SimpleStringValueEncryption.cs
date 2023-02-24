// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Rdmp.Core.Curation;

/// <summary>
/// Core RDMP implementation of RSA puublic/private key encryption.  In order to be secure you should create a private key (See PasswordEncryptionKeyLocationUI).  If
/// no private key is configured then the default Key will be used (this is not secure and anyone with access to the RDMP source code could decrypt your strings - which
///  is open source!). Strings are encrypted based on the key file.  Note that because RSA is a good encryption technique you will get a different output (encrypted) string
/// value for repeated calls to Encrypt even with the same input string.
/// </summary>
public class SimpleStringValueEncryption : IEncryptStrings
{
    private Encoding encoding = Encoding.ASCII;
        
    private const string Key =
        @"<?xml version=""1.0"" encoding=""utf-16""?>
<RSAParameters xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <Exponent>AQAB</Exponent>
    <Modulus>sMDeszVErUmbqOxQavw5OsWpL3frccEGtTJYM8G54Fw7NK6xFVUrq79nWB6px4/B</Modulus>
    <P>6kcXnTVJrVuD9j6qUm+F71jIL2H92lgN</P>
    <Q>wSRbrdj1qGBPBnYMO5dx11gvfNCKKdWF</Q>
    <DP>aKdxaQzQ6Nwkyu+bbk/baNwkMOZ5W/xR</DP>
    <DQ>B/B8rErM3l0HIpbbrd9t2JJRcWoJI+sZ</DQ>
    <InverseQ>NFv4Z26nbMpOkOcAnO3rktoMffza+3Ul</InverseQ>
    <D>Y8zC8dUF7gI9zeeAkKfReInauV6wpg4iVh7jaTDN5DAmKFURTAyv6Il6LEyr07JB</D>
</RSAParameters>";

    /// <summary>
    /// The private key file parameters required to encrypt/decrypt strings.  These will either be read from the secure location on disk (<see cref="Repositories.Managers.PasswordEncryptionKeyLocation"/>) or
    /// will match the the default decryption certificate (<see cref="Key"/>).
    /// </summary>
    public RSAParameters PrivateKey { get; set; }

    public SimpleStringValueEncryption(RSAParameters? parameters)
    {
        //use the memory one no parameters passed
        PrivateKey = parameters ?? (RSAParameters)new XmlSerializer(typeof(RSAParameters)).Deserialize(new StringReader(Key));
    }

    /// <summary>
    /// Encrypts using its Public Key then returns a the encrypted byte[] as a string by using BitConverter.ToString()
    /// </summary>
    /// <returns></returns>
    public string Encrypt(string toEncrypt)
    {
        using var RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(PrivateKey);
        return BitConverter.ToString(RSA.Encrypt(encoding.GetBytes(toEncrypt), false));
    }
        
    /// <summary>
    /// Takes an encrypted byte[] (in string format as produced by BitConverter.ToString() 
    /// </summary>
    /// <param name="toDecrypt"></param>
    /// <returns></returns>
    public string Decrypt(string toDecrypt)
    {
        using var RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(PrivateKey);

        try
        {
            return encoding.GetString(RSA.Decrypt(ByteConverterGetBytes(toDecrypt), false));
        }
        catch (CryptographicException e)
        {
            throw new CryptographicException("Could not decrypt an encrypted string, possibly you are trying to decrypt it after having changed the PrivateKey to a different one than at the time it was encrypted?",e);
        }
    }

    private static byte[] ByteConverterGetBytes(string encodedstring)
    {
        var arr = encodedstring.Split('-');
        var array = new byte[arr.Length];

        for (var i = 0; i < arr.Length; i++)
            array[i] = Convert.ToByte(arr[i], 16);

        return array;
    }

    public bool IsStringEncrypted(string value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value))
            return false;

        return value.Count(c=>c== '-') >= 47;
    }
}