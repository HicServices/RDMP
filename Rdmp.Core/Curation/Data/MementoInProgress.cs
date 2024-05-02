// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Tracks changes made to a single object.
/// </summary>
public class MementoInProgress
{
    /// <summary>
    ///     Tracks the original serialized yaml of the object tracked .
    /// </summary>
    public string OldYaml { get; }

    /// <summary>
    ///     The last modification to the object within a <see cref="CommitInProgress" />.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     What is happening to the object, defaults to <see cref="MementoType.Modify" />
    /// </summary>
    public MementoType Type { get; set; } = MementoType.Modify;

    private readonly Dictionary<PropertyInfo, object> _props = new();

    public MementoInProgress(IMapsDirectlyToDatabaseTable o, string oldYaml)
    {
        OldYaml = oldYaml;
        foreach (var prop in TableRepository.GetPropertyInfos(o.GetType())) _props.Add(prop, prop.GetValue(o, null));
    }

    /// <summary>
    ///     Returns all properties on <paramref name="currentState" /> which are different from
    ///     when this class was constructed.
    /// </summary>
    /// <param name="currentState"></param>
    /// <returns></returns>
    public IEnumerable<PropertyInfo> GetDiffProperties(IMapsDirectlyToDatabaseTable currentState)
    {
        foreach (var prop in _props)
        {
            var newState = prop.Key.GetValue(currentState, null);

            // if new state is not null
            if (newState != null)
            {
                // any change is a difference
                if (!newState.Equals(prop.Value))
                    yield return prop.Key;
            }
            else
            {
                // value changed from null to not null
                if (prop.Value != null) yield return prop.Key;
            }
        }
    }
}