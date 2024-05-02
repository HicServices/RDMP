// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Ticketing;

/// <summary>
///     All implementations of ITicketingSystem will be given this parameter as a constructor argument.  It includes the
///     RDMP configured credentials for
///     the ticketing system.  Credentials.Password is encrypted, use GetDecryptedPassword() if you want to use the
///     Credentials property (this method can
///     fail if the user does not have access to the password decryption key (see PasswordEncryptionKeyLocationUI).
/// </summary>
public class TicketingSystemConstructorParameters
{
    public string Url { get; set; }
    public IDataAccessCredentials Credentials { get; set; }

    public TicketingSystemConstructorParameters(string url, IDataAccessCredentials credentials)
    {
        Url = url;
        Credentials = credentials;
    }
}