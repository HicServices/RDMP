// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode.DataAccess;

/// <summary>
///     Encrypted Password string.  It is expected that GetDecryptedPassword method will throw Exception if the current
///     user doesn't have access
///     to the resources required to decrypt the Password (e.g. access to an RSA private key)
/// </summary>
public interface IEncryptedPasswordHost
{
    /// <summary>
    ///     The encrypted password stored in memory (and possibly in the database).  This property should never return a clear
    ///     text password.  Use <see cref="GetDecryptedPassword" />
    ///     to get the decrypted string.
    /// </summary>
    string Password { get; set; }

    /// <summary>
    ///     Decrypts the encrypted Password property.  This method will throw an Exception if the user doesn't have access to
    ///     the resources required
    ///     to decrypt the Password (e.g. access to an RSA private key).
    /// </summary>
    /// <returns></returns>
    string GetDecryptedPassword();
}