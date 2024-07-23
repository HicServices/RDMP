// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Version = System.Version;

namespace Rdmp.Core.Repositories;

/// <summary>
/// Reads/Writes <see cref="Version"/> as a simple string value
/// </summary>
internal sealed class VersionYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Version);

    public object ReadYaml(IParser parser, Type _1, ObjectDeserializer _2)
    {
        var s = parser.Consume<Scalar>();
        return new Version(s.Value);
    }

    public void WriteYaml(IEmitter emitter, object value, Type _1, ObjectSerializer _2)
    {
        if (value is not Version v) throw new ArgumentException("Non-Version argument", nameof(value));

        emitter.Emit(new Scalar(v.ToString()));
    }
}