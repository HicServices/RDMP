// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataLoad.Modules.DataProvider;

/// <summary>
///     Describes a website / webservice endpoint which can be accessed with an optional username / password.  Use this
///     Type when you need a [DemandsInitialization]
///     property on a component (e.g. IAttacher) which is a remote website/webservice.
/// </summary>
public class WebServiceConfiguration : EncryptedPasswordHost, ICustomUIDrivenClass
{
    private readonly ICatalogueRepository _repository;

    /// <summary>
    ///     For XML Serialization
    /// </summary>
    private WebServiceConfiguration()
    {
    }

    public WebServiceConfiguration(ICatalogueRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public string Endpoint { get; set; }
    public string Username { get; set; }

    //[Obsolete]
    //public string EndpointName { get; set; }

    public int MaxBufferSize { get; set; }
    public int MaxReceivedMessageSize { get; set; }

    public void RestoreStateFrom(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var deserializer = new XmlSerializer(typeof(WebServiceConfiguration));
        try
        {
            var deserialized = (WebServiceConfiguration)deserializer.Deserialize(new StringReader(value));
            deserialized.SetRepository(_repository);

            Endpoint = deserialized.Endpoint;
            Username = deserialized.Username;
            Password = deserialized.Password;
            MaxBufferSize = deserialized.MaxBufferSize;
            MaxReceivedMessageSize = deserialized.MaxReceivedMessageSize;
        }
        catch (Exception e)
        {
            throw new Exception($"Deserialisation failed: {value}", e);
        }
    }

    public string SaveStateToString()
    {
        var sb = new StringBuilder();

        var serializer = new XmlSerializer(GetType());

        using (var sw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
        {
            serializer.Serialize(sw, this);
        }

        return sb.ToString();
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint) &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password);
    }
}