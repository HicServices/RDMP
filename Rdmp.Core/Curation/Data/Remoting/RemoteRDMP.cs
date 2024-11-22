// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data.Remoting;

/// <summary>
/// This represent a Remote Installation of RDMP which can accept connections at multiple endpoints
/// 
/// <para>Endpoints are usual REST endpoints with a URL and a type which they can accept.
/// The endpoint format is {Url}/api/{typename}
/// The typename is used to create the URL for the endpoint.
/// If you are sending a collection, append this to the URI: ?asarray=true</para>
/// </summary>
public class RemoteRDMP : DatabaseEntity, INamed, IEncryptedPasswordHost
{
    #region Database Properties

    private string _uRL;
    private string _name;
    private string _username;

    private EncryptedPasswordHost _encryptedPasswordHost;
    private string _tempPassword;

    #endregion

    /// <summary>
    /// Web service URL for communicating with the remote RDMP instance
    /// </summary>
    public string URL
    {
        get => _uRL;
        set => SetField(ref _uRL, value);
    }

    /// <inheritdoc/>
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Username to specify when connecting to the remote webservice
    /// </summary>
    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    /// <inheritdoc/>
    public string Password
    {
        get => _encryptedPasswordHost.Password;
        set
        {
            // if we are being deserialized (using blank constructor)
            if (_encryptedPasswordHost == null)
            {
                // store the encrypted value from the database in a temp variable
                // until we get told how to decrypt (see SetRepository)
                _tempPassword = value;
                return;
            }

            _encryptedPasswordHost.Password = value;
            OnPropertyChanged(null, value);
        }
    }

    /// <inheritdoc/>
    public string GetDecryptedPassword() =>
        _encryptedPasswordHost == null
            ? throw new Exception(
                $"Passwords cannot be decrypted until {nameof(SetRepository)} has been called and decryption strategy is established")
            : _encryptedPasswordHost.GetDecryptedPassword() ?? "";

    public RemoteRDMP()
    {
    }

    /// <inheritdoc/>
    public RemoteRDMP(ICatalogueRepository repository)
    {
        // need a new copy of the catalogue repository so a new DB connection can be made to use with the encrypted host.
        _encryptedPasswordHost = new EncryptedPasswordHost(repository);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", "Unnamed remote" },
            { "URL", "https://example.com" }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    /// <inheritdoc/>
    public RemoteRDMP(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        // need a new copy of the catalogue repository so a new DB connection can be made to use with the encrypted host.
        _encryptedPasswordHost = new EncryptedPasswordHost((ICatalogueRepository)repository);

        URL = r["URL"].ToString();
        Name = r["Name"].ToString();
        Username = r["Username"] as string;
        Password = r["Password"] as string;
    }

    /// <inheritdoc cref="Name"/>
    public override string ToString() => Name;


    /// <summary>
    /// Gets the web service sub url for interacting with the object T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isarray"></param>
    /// <returns></returns>
    public string GetUrlFor<T>(bool isarray = false)
    {
        var baseUri = new UriBuilder(new Uri(URL));
        if (isarray)
            baseUri.Query = "isarray=true";

        baseUri.Path += $"/api/{typeof(T).Name}";

        return baseUri.ToString();
    }

    /// <summary>
    /// Gets the web service sub url for performing a data release
    /// </summary>
    /// <returns></returns>
    public string GetUrlForRelease()
    {
        var baseUri = new UriBuilder(new Uri(URL));

        baseUri.Path += "/api/Release/";

        return baseUri.ToString();
    }

    /// <summary>
    /// Gets the web service sub url for value checking?
    /// </summary>
    /// <returns></returns>
    public string GetCheckingUrl()
    {
        var baseUri = new UriBuilder(new Uri(URL));

        baseUri.Path += "/api/values/";

        return baseUri.ToString();
    }


    public void SetRepository(ICatalogueRepository repository)
    {
        _encryptedPasswordHost = new EncryptedPasswordHost(repository)
        {
            Password = _tempPassword
        };
    }
}