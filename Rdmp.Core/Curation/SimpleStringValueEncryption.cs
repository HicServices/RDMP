// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Core;

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
       @"<? xml version = ""1.0"" encoding=""utf-16""?><RSAParameters xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Modulus>5CBYA/4GKu57eepMM5dHguZR6QxPakujvUPq81YQlAs9XdAS5OugT9xYATXV0ZUVeQrtCOj1jjS6cSnekJKzXMD48H2IbT+ImRVqyjE19dgeeZtK1cGa8wDKTdNtjo+ur2iaMzItE3VChcMidWncQpiieieSUwQ81uoab7foVdaQm078TzlHLaWiSyAPCOOIeeO2q7HEjVPkbiqCGl2Lkrzvzct84SDMFkyQXzXVJCfdlFn5bX3/8OwC9gWVICPBVbVFZZQ3skUKFqK/aYcgJL/svDyhFsj89TK3xzz8YE1r8VwxVtvqLfRXrWqUCV1n2vEm4XUjuTwQi2nwclREuQ==</Modulus><Exponent>AQAB</Exponent><P>+zfBbR8e3gLc5TDOSoyjclKdPyl62BZvp0kdlmihI5rJ/Bk+CnymYr22vBbWe/wJugWL4bLEAMWiWsa0ri0mJig24aJZ+DMEJDh+wz4J70OBbsE7jydw9whkt6r/8dYCwE2L4aKlL5pUOL/DzgQJ6vxkQ3UAjYMvEzBPapd7Z7c=</P><Q>6HgO3YVz2umROVrHkQ0xa+4a+EMZVAEBhS+ZuJ5KhXBomxYptfAud0WmGN1zOM7TpBYnk8IBx2kuBKoAavFtjbjsoINVxlLlUvUDEJmeElVz5TqRRCNEChY0sfDlR9gVIopB/p7BU5SFRz5i7+qpsWzWdBU/BR93K2vJNsj2Vw8=</Q><DP>ecv4bY1vC7hbnIrjGWXCQMUpE9xqgKWwEGz0eV3U8kwzrZQXbkIs8SaFl/+Cka4KkTPrM8vWF4G6S0SXiPK+0jUhFpf+AsXJNj5lxwcnDeeusyHgXHGE5WAeZKX1XSyjPNTcAtM2PzQVrUXcCuAOZu1jNwlc8T8u7aC4gDddT1U=</DP><DQ>KNlP42Ub4o/AUQ++maJz2L9SReWkgbpbhgfDP0mxVplWCEpwseOuho7ajOv83zKYxfCOq8wfe+bjizZENIaP9aNVES+C1wKiAV3EWBpmSFpzrwgHlq2LuyoDwHDQGTvDGvqodhF3bzRd5xLzV60ofGDfni5NkJzi1+JszQ+rGck=</DQ><InverseQ>BC4M2rtw/lHKW8gDVcQSB1a1yWlgtLqtoX+krelqO59/6Np2ApsPc43SUoy4PY1f+Oxf+Erik1NM1+TRucVBGB8AP1q0SFuTsmWiLE9zv/1zjeJJLOoPbpGia+bQ/r7fP+ZhBK8ldae7FAOctcoSfQ0jAn2IBpDyvlAlcnwRvwQ=</InverseQ><D>q1e/w//gEg7dn0xjv7w4chEcJLaiT2xQp6+DoRFbklZ+2R+XkWmJF3KghwgweSJI5olWUALprM3d23FfQaduIJSwZbFj7upxZsm3U/ZyWRzihuQk6ThpcWt+h8Xt283/nrAqYZmmUZ8ZP+64ywef8EVEhAuE0+Wy7JkZEiBH2W/MEXUvbMV8w282/X6H8zpIkHgjMvy/rouDMFA+ZLR9OOCofw7aVV9VivOVCVIhWe+inrQzG3UCLEEmKNOy0FmqQYvZ4vtwJ+kAByo6xW2YO9cHtEJFiKrZ1O2A0P0xtziOqStDq6JqoeE/bty8y3oM3HPyXMZXG2ecLuwbP4usoQ==</D></RSAParameters>";

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