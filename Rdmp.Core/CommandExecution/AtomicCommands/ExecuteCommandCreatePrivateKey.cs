// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a new private key for encrypting credentials in RDMP.  Old passwords will not decrypt using the new key and
///     will be lost (unless you have the original key)
/// </summary>
public class ExecuteCommandCreatePrivateKey : BasicCommandExecution
{
    private readonly FileInfo _keyFileToCreate;
    private readonly PasswordEncryptionKeyLocation _encryption;

    public ExecuteCommandCreatePrivateKey(IBasicActivateItems activator, FileInfo keyFileToCreate) : base(activator)
    {
        _keyFileToCreate = keyFileToCreate;
        _encryption =
            activator.RepositoryLocator.CatalogueRepository.EncryptionManager as PasswordEncryptionKeyLocation;

        if (_encryption == null)
        {
            SetImpossible("Current Encryption manager is not based on key files");
            return;
        }

        var existing = _encryption.GetKeyFileLocation();
        if (existing != null)
            SetImpossible($"There is already a key file at '{existing}'");
    }

    public override void Execute()
    {
        base.Execute();

        _encryption.CreateNewKeyFile(_keyFileToCreate.FullName);
    }
}