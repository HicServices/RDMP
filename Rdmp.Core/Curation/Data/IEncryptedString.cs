// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     A string which can be decrypted.  GetDecryptedValue might throw if the user doesn't have access  to the decryption
///     algorithm / artifacts. Allows transmission of a
///     string without nessesarily having access to the decrypted value.
/// </summary>
public interface IEncryptedString
{
    /// <inheritdoc cref="Rdmp.Core.ReusableLibraryCode.DataAccess.IEncryptedPasswordHost.Password" />
    string Value { get; set; }

    /// <inheritdoc cref="ReusableLibraryCode.DataAccess.IEncryptedPasswordHost.GetDecryptedPassword" />
    string GetDecryptedValue();

    /// <summary>
    ///     Returns true if the <paramref name="value" /> looks like it is encrypted
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool IsStringEncrypted(string value);
}