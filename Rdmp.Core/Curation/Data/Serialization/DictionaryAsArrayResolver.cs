// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Rdmp.Core.Curation.Data.Serialization;

/// <summary>
///     JSON Contract Resolver which serializes <see cref="IDictionary" /> as two arrays (keys and values).  This
///     allows serialization of keys which are complex types (by default json only supports string keys).
/// </summary>
public class DictionaryAsArrayResolver : DefaultContractResolver
{
    protected override JsonContract CreateContract(Type objectType)
    {
        return objectType.GetInterfaces().Any(i => i == typeof(IDictionary) ||
                                                   (i.IsGenericType && i.GetGenericTypeDefinition() ==
                                                       typeof(IDictionary<,>)))
            ? base.CreateArrayContract(objectType)
            : base.CreateContract(objectType);
    }
}