// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Newtonsoft.Json;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Serialization;

/// <summary>
///     Facilitates the use of <see cref="DatabaseEntityJsonConverter" /> and
///     <see cref="PickAnyConstructorJsonConverter" /> by configuring appropriate <see cref="JsonSerializerSettings" /> etc
/// </summary>
public static class JsonConvertExtensions
{
    /// <summary>
    ///     Serialize the given object resolving any properties which are <see cref="DatabaseEntity" /> into pointers using
    ///     <see cref="DatabaseEntityJsonConverter" />
    /// </summary>
    /// <param name="value"></param>
    /// <param name="repositoryLocator"></param>
    /// <returns></returns>
    public static string SerializeObject(object value, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        var databaseEntityJsonConverter = new DatabaseEntityJsonConverter(repositoryLocator);

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Converters = new JsonConverter[] { databaseEntityJsonConverter },
            ContractResolver = new DictionaryAsArrayResolver()
        };

        return JsonConvert.SerializeObject(value, settings);
    }

    /// <summary>
    ///     Deserializes a string created with <see cref="SerializeObject(object,IRDMPPlatformRepositoryServiceLocator)" />.
    ///     This involves two additional areas of functionality
    ///     beyond basic JSON:
    ///     <para>
    ///         1. Any database pointer (e.g. Catalogue 123 0xab1d) will be fetched and returned from the appropriate
    ///         platform database (referenced by <paramref name="repositoryLocator" />)
    ///     </para>
    ///     <para>
    ///         2. Objects do not need a default constructor, instead <see cref="PickAnyConstructorJsonConverter" /> will be
    ///         used with <paramref name="objectsForConstructingStuffWith" />
    ///     </para>
    ///     <para>
    ///         3. Any objects implementing <see cref="IPickAnyConstructorFinishedCallback" /> will have
    ///         <see cref="IPickAnyConstructorFinishedCallback.AfterConstruction" /> called
    ///     </para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="repositoryLocator"></param>
    /// <param name="objectsForConstructingStuffWith"></param>
    /// <returns></returns>
    public static object DeserializeObject(string value, Type type,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator, params object[] objectsForConstructingStuffWith)
    {
        var databaseEntityJsonConverter = new DatabaseEntityJsonConverter(repositoryLocator);
        var lazyJsonConverter =
            new PickAnyConstructorJsonConverter(new[] { repositoryLocator }.Union(objectsForConstructingStuffWith)
                .ToArray());

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Converters = new JsonConverter[] { databaseEntityJsonConverter, lazyJsonConverter },
            ContractResolver = new DictionaryAsArrayResolver()
        };

        return JsonConvert.DeserializeObject(value, type, settings);
    }
}