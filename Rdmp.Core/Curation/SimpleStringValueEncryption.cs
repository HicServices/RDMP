// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rdmp.Core.Curation;

/// <summary>
/// Core RDMP implementation of RSA public/private key encryption.  In order to be secure you should create a private key (See PasswordEncryptionKeyLocationUI).  If
/// no private key is configured then the default Key will be used (this is not secure and anyone with access to the RDMP source code could decrypt your strings - which
///  is open source!). Strings are encrypted based on the key file.  Note that because RSA is a good encryption technique you will get a different output (encrypted) string
/// value for repeated calls to Encrypt even with the same input string.
/// </summary>
public class SimpleStringValueEncryption : IEncryptStrings
{
    private readonly RSACryptoServiceProvider _turing = new();

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

    public SimpleStringValueEncryption(string parameters)
    {
        _turing.FromXmlString(parameters ?? Key);
    }

    /// <summary>
    /// Encrypt the payload using a new AES key and IV.  The AES key is then encrypted using the RSA public key and the IV is prepended to the encrypted payload.
    /// A prefix '$js1$' is added to the encrypted string to indicate that it is encrypted using the new (2023) RDMP default encryption algorithm.
    /// </summary>
    /// <returns></returns>
    public string Encrypt(string toEncrypt)
    {

        // Fall back on bad encryption if no private key is configured
        if (_turing.KeySize < 1024)
            return string.Join('-',
                _turing.Encrypt(Encoding.UTF8.GetBytes(toEncrypt), false).Select(octet => octet.ToString("X2")));
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateIV();
        aes.GenerateKey();
        var cipherText = Convert.ToBase64String(aes.EncryptCfb(Encoding.UTF8.GetBytes(toEncrypt), aes.IV));
        var keyBlock = new byte[1 + aes.IV.Length + aes.Key.Length];
        keyBlock[0] = (byte)aes.IV.Length; // Note: this encoding assumes IV cannot exceed 255 bytes!
        Array.Copy(aes.IV, 0, keyBlock, 1, aes.IV.Length);
        Array.Copy(aes.Key, 0, keyBlock, 1 + aes.IV.Length, aes.Key.Length);
        var key = Convert.ToBase64String(_turing.Encrypt(keyBlock, true));
        return $"$js1${key}${cipherText}$";
    }

    /// <summary>
    /// Takes an encrypted byte[] (in string format as produced by BitConverter.ToString()
    /// </summary>
    /// <param name="toDecrypt"></param>
    /// <returns></returns>
    public string Decrypt(string toDecrypt)
    {
        if (toDecrypt.StartsWith("$js1$", StringComparison.Ordinal) && toDecrypt.EndsWith("$", StringComparison.Ordinal))
        {
            // Good, it's a new-style AES+RSA encrypted string
            var parts = toDecrypt.Split('$');
            if (parts.Length != 5)
                throw new CryptographicException(
                    "Could not decrypt an encrypted string, it was not in the expected format of $js1$<base64key>$<base64ciphertext>$");
            var keyBlock = _turing.Decrypt(Convert.FromBase64String(parts[2]), true);
            var ivLength = keyBlock[0];
            var iv = new byte[ivLength];
            var key = new byte[keyBlock.Length - 1 - ivLength];
            Array.Copy(keyBlock, 1, iv, 0, ivLength);
            Array.Copy(keyBlock, 1 + ivLength, key, 0, key.Length);
            var cipherText = Convert.FromBase64String(parts[3]);
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.IV = iv;
            aes.Key = key;
            return Encoding.UTF8.GetString(aes.DecryptCfb(cipherText, aes.IV));
        }

        try
        {
            return Encoding.UTF8.GetString(_turing.Decrypt(ByteConverterGetBytes(toDecrypt), false));
        }
        catch (CryptographicException e)
        {
            throw new CryptographicException(
                "Could not decrypt an encrypted string, possibly you are trying to decrypt it after having changed the PrivateKey to a different one than at the time it was encrypted?",
                e);
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
        return value != null && !string.IsNullOrWhiteSpace(value) &&
               (value.StartsWith("$js1$") || value.Count(c => c == '-') >= 47);
    }
}