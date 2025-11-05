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
        <RSAKeyValue>
            <Modulus>xZ7V6twqlj+3L1hsjB+BwhzQEgcs8tpqzgSjwgjPwzfisLFBN9HkwX2u+ZrgrKhKmZgSOVBYRhZiGJeF1hY5gLz3/Zo05fcvtgL0ylEhi6wnH9y6CPu8+4xCOJ26eywtNS02v2CC/0HUWnw6kNHBqdyyNjwuC12Ll5YJ3wZ1kQZ2mce3DMjpgRDvYqnm8ldwLWpgreK7I5vggQgons3v39x+Cx501Eo0qd+iHzXf6/8pUhYqb+xhDdb9gnHcitipXVFG3Ts27OUiuO0uLAO4Es400ApyWpvHdCCNrBJ/EQWZGEO/qVKmfP0CWBmokVL+IUawX9lPl/Luo9W5AFpaIQ==</Modulus>
            <Exponent>AQAB</Exponent>
            <P>xvQflEOfLYAXiMFrG/alxcgUbimbY8Qc1/3Jm21CFVrkQL1rNQ/PEXavXYyNP3jRwUrqKbJrtcdz5MPgKpvA/iWdqjp4Qx+V7N2DkdzKL7liViKoCHf1eXfyeWkCErHOgInTerOSlCaeABqkqpr+eaAtzy2j4df5U/1+j8g7I48=</P>
            <Q>/kjapfiD7aC1yQX8BYiJ29oTlvZx0sptfXA0Bx9qXvd1UFTqjDbVoIF7tXk+DsottRL97G52ImpYf4w4wJFb1F+d5sQCaCrEzIdQ/4mJdpn7FLYxErYXpoZl0e2knIoqQie7+vaw2oV9YSx3iJHoEVfDM+0NAW+aCUUJGNorD08=</Q>
            <DP>mAYms0ZQtZXxZcBWNhHsbgsLAXqtkDhkye7VRPzhyCuhyo5zAyLHWVLVgahKrjuGHCtAbwg1Ibv8pMu/2Q8XE5xus4rmJnRWPZ6uUKDjpkAEEkl9GKuBWYX8NCW3Pc28O6AVhub8lFRF21KAjRTOauWo22zGk2ZS0IkdUoTwG6U=</DP>
            <DQ>nQsrllNMT0bw3k0G3/f6hEBD1vkvROrmAhF44GlDjZEw78Lx9FStTOqLF4HglMvCvNEU55808H5TV7qnFi7v0tKWt32YqvK3BkYP/THZJtlkWt9GoXK6WosoeSVWg6NFBAR8MTuH7/1/eLM4w6yw8X0NPpWJcbiWHmF3g9TBwTs=</DQ>
            <InverseQ>fbI4APSSbOOd07goBWzZYjdrEVEZDiboK4jFKYoMI19PrbQBTvpRmLsJxhg96fgjiFfmw05tsPIsLSQPczsav6JDq25sQyOaz7VE33Y8CEV4rVvI+l7vlWqvn/EfThPqYdR4UOHa6G5wcBTI48O4+SmKMhLQl1GK/ZA+XEmlHx8=</InverseQ>
            <D>QC+Uv1F/K4nKT8BikShylr+Q/SoDeWVjp0Juhcki4f82y7jmu+CachYGTN/29V07zaNM1/y2jx0aA27Dc4OIbb3ythXt9HtSrcVMCKJNSPZDRuAENIK/INyvbYAdX4A7trfWvlX0dj/FXxZWV08pnagm4eKt+dcKTdPXpO6OJOnnSYMcdRBFdZLAj/sh8oMNXMHSxsCA7YHjL+4NGbpyTMYnfa6CUxFc36cLi2/Hm6JhV1nYEswTI5T1qRqmWpF1+H0ksxLKx6I5D61mzfaf7RQPJRCLhddXoLPIh8IaoNIomPcZyZQBxwBhtgj8pf9rcBN50aqNc5V2vKoEOt/gGQ==</D>
        </RSAKeyValue>";

    private readonly string _parameters;

    public SimpleStringValueEncryption(string parameters)
    {
        _parameters=parameters;
    }

    /// <summary>
    /// Encrypt the payload using a new AES key and IV.  The AES key is then encrypted using the RSA public key and the IV is prepended to the encrypted payload.
    /// A prefix '$js1$' is added to the encrypted string to indicate that it is encrypted using the new (2023) RDMP default encryption algorithm.
    /// </summary>
    /// <returns></returns>
    public string Encrypt(string toEncrypt)
    {
        _turing.FromXmlString(_parameters ?? Key);
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
        _turing.FromXmlString(_parameters ?? Key);
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