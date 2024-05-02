// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Stores a username and encrypted password the Password property of the entity will be a hex value formatted as
///     string which can be decrypted at runtime via
///     the methods of base class EncryptedPasswordHost which currently uses SimpleStringValueEncryption which is a wrapper
///     for RSACryptoServiceProvider.  The layout
///     of this hierarchy however allows for future plugin utility e.g. using different encryption keys for different
///     tables / user access rights etc.
/// </summary>
public class DataAccessCredentials : DatabaseEntity, IDataAccessCredentials, INamed, IHasDependencies
{
    private readonly EncryptedPasswordHost _encryptedPasswordHost;

    #region Database Properties

    private string _name;
    private string _username;

    /// <inheritdoc />
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc />
    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    /// <inheritdoc />
    public string Password
    {
        get => _encryptedPasswordHost.Password;
        set
        {
            _encryptedPasswordHost.Password = value;
            OnPropertyChanged(null, value);
        }
    }

    #endregion

    public DataAccessCredentials()
    {
        _encryptedPasswordHost = new EncryptedPasswordHost();
    }

    /// <summary>
    ///     Records a new (initially blank) set of credentials that can be used to access a <see cref="TableInfo" /> or other
    ///     object requiring authentication.
    ///     <para>A single <see cref="DataAccessCredentials" /> can be shared by multiple tables</para>
    ///     <para>You can also use <see cref="DataAccessCredentialsFactory" /> for easier credentials creation</para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public DataAccessCredentials(ICatalogueRepository repository, string name = null)
    {
        name ??= $"New Credentials {Guid.NewGuid()}";

        _encryptedPasswordHost = new EncryptedPasswordHost(repository);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name }
        });
    }

    internal DataAccessCredentials(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        _encryptedPasswordHost = new EncryptedPasswordHost(repository);

        Name = (string)r["Name"];
        Username = r["Username"].ToString();
        Password = r["Password"].ToString();
    }

    /// <inheritdoc />
    public override void DeleteInDatabase()
    {
        var users = CatalogueRepository.TableInfoCredentialsManager.GetAllTablesUsingCredentials(this);

        // if there are any contexts where there are any associated tables using this credentials
        if (users.Any(k => k.Value.Any()))
            throw new CredentialsInUseException(
                $"Cannot delete credentials {Name} because it is in use by one or more TableInfo objects({string.Join(",", users.SelectMany(u => u.Value).Distinct().Select(t => t.Name))})");

        try
        {
            base.DeleteInDatabase();
        }
        catch (Exception e)
        {
            if (e.Message.Contains("FK_DataAccessCredentials_TableInfo_DataAccessCredentials"))
                throw new CredentialsInUseException(
                    $"Cannot delete credentials {Name} because it is in use by one or more TableInfo objects({string.Join("", GetAllTableInfosThatUseThis().Values.Select(t => string.Join(",", t)))})",
                    e);

            throw;
        }
    }

    /// <summary>
    ///     Returns all the <see cref="TableInfo" /> that rely on the credentials to access the table(s).  This is split into
    ///     the contexts under which the
    ///     credentials are used e.g. <see cref="DataAccessContext.DataLoad" />
    /// </summary>
    /// <returns></returns>
    public Dictionary<DataAccessContext, List<ITableInfo>> GetAllTableInfosThatUseThis()
    {
        return CatalogueRepository.TableInfoCredentialsManager.GetAllTablesUsingCredentials(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <inheritdoc />
    public string GetDecryptedPassword()
    {
        return _encryptedPasswordHost == null
            ? throw new Exception(
                $"Passwords cannot be decrypted until {nameof(SetRepository)} has been called and decryption strategy is established")
            : _encryptedPasswordHost.GetDecryptedPassword() ?? "";
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return Array.Empty<IHasDependencies>();
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return GetAllTableInfosThatUseThis().SelectMany(kvp => kvp.Value).Cast<IHasDependencies>().ToArray();
    }

    public bool PasswordIs(string password)
    {
        var p = GetDecryptedPassword();

        return string.IsNullOrWhiteSpace(p) ? string.IsNullOrWhiteSpace(password) : p.Equals(password);
    }

    internal void SetRepository(ICatalogueRepository repository)
    {
        _encryptedPasswordHost.SetRepository(repository);
    }
}