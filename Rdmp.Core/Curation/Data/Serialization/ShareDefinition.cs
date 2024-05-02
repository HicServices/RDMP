// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

namespace Rdmp.Core.Curation.Data.Serialization;

/// <summary>
///     Describes a DatabaseEntity which has been made exportable from RDMP via <see cref="ObjectExport" />.  This class
///     includes the properties that are
///     directly recorded for the object e.g. Name, SelectSQL etc.  For Foreign Key columns (See
///     <see cref="RelationshipAttribute" />) e.g. <see cref="CatalogueItem.Catalogue_ID" /> the Guid
///     of another <see cref="ShareDefinition" /> is given (e.g. of the <see cref="Catalogue" />).  This means that a
///     <see cref="ShareDefinition" /> is only valid when all its dependencies are
///     also available (See Sharing.Dependency.Gathering.Gatherer for how to do this)
/// </summary>
[Serializable]
public class ShareDefinition
{
    /// <summary>
    ///     The unique number that identifies this shared object.  This is created when the object is first shared as an
    ///     <see cref="ObjectExport" /> and
    ///     persisted by all other systems that import the object as an <see cref="ObjectImport" />.
    /// </summary>
    public Guid SharingGuid { get; set; }

    /// <summary>
    ///     The <see cref="DatabaseEntity.ID" /> of the object in the original database the share was created from (this will
    ///     be different to the ID it has when imported elsewhere)
    /// </summary>
    [JsonIgnore]
    public int ID { get; set; }

    /// <summary>
    ///     The System.Type and therefore database table of the <see cref="DatabaseEntity" /> that is being shared
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    ///     The values of all public properties on the <see cref="DatabaseEntity" /> (except ID, Relationship and
    ///     NoMappingToDatabase properties).  This does not include
    ///     any foreign key ID properties e.g. <see cref="CatalogueItem.Catalogue_ID" /> which will instead be stored in
    ///     <see cref="RelationshipProperties" />
    /// </summary>
    public Dictionary<string, object> Properties { get; set; }

    /// <summary>
    ///     The values of all foreign key properties on the <see cref="DatabaseEntity" /> (e.g.
    ///     <see cref="CatalogueItem.Catalogue_ID" />).  This is the SharingGuid of the referenced object.
    ///     An object cannot be shared unless it is also shared with all such dependencies.
    /// </summary>
    public Dictionary<RelationshipAttribute, Guid> RelationshipProperties = new();

    /// <inheritdoc cref="ShareDefinition" />
    public ShareDefinition(Guid sharingGuid, int id, Type type, Dictionary<string, object> properties,
        Dictionary<RelationshipAttribute, Guid> relationshipProperties)
    {
        if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
            throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable", nameof(type));

        SharingGuid = sharingGuid;
        ID = id;
        Type = type;
        Properties = properties;

        RelationshipProperties = new Dictionary<RelationshipAttribute, Guid>();

        foreach (var kvp in relationshipProperties)
            RelationshipProperties.Add(kvp.Key, kvp.Value);
    }

    /// <summary>
    ///     Removes null entries and fixes problematic value types e.g. <see cref="FolderHelper" /> which is better imported as
    ///     a string
    /// </summary>
    public Dictionary<string, object> GetDictionaryForImport()
    {
        //Make a dictionary of the normal properties we are supposed to be importing
        var newDictionary = Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        //remove null arguments they won't help us here
        foreach (var key in newDictionary.Keys.ToArray())
            if (newDictionary[key] == null)
                newDictionary.Remove(key);

        return newDictionary;
    }
}