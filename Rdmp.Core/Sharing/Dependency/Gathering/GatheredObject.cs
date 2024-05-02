// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Sharing.Dependency.Gathering;

/// <summary>
///     The described Object is only tenously related to the original object and you shouldn't worry too much if during
///     refactoring you don't find any references.
///     An example of this would be all Filters in a Catalogue where a single ColumnInfo is being renamed.  Any filter in
///     the catalogue could contain a reference to
///     the ColumnInfo but most won't.
///     <para>
///         Describes an RDMP object that is related to another e.g. a ColumnInfo can have 0+ CatalogueItems associated
///         with it.  This differs from IHasDependencies by the fact that
///         it is a more constrained set rather than just spider webbing out everywhere.
///     </para>
/// </summary>
public class GatheredObject : IHasDependencies, IMasqueradeAs
{
    public IMapsDirectlyToDatabaseTable Object { get; }
    public List<GatheredObject> Children { get; }

    public GatheredObject(IMapsDirectlyToDatabaseTable o)
    {
        Object = o;
        Children = new List<GatheredObject>();
    }

    /// <summary>
    ///     True if the gathered object is a data export object (e.g. it is an ExtractableColumn or DeployedExtractionFilter)
    ///     and it is part of a frozen (released)
    ///     ExtractionConfiguration
    /// </summary>
    public bool IsReleased { get; set; }

    /// <summary>
    ///     Creates a sharing export (<see cref="ObjectExport" />) for the current <see cref="GatheredObject.Object" /> and
    ///     then serializes it as a <see cref="ShareDefinition" />.
    ///     This includes mapping any [<see cref="RelationshipAttribute" />] properties on the
    ///     <see cref="GatheredObject.Object" /> to the relevant Share Guid (which must
    ///     exist in branchParents).
    ///     <para>ToShareDefinitionWithChildren if you want a full list of shares for the whole tree</para>
    /// </summary>
    /// <param name="shareManager"></param>
    /// <param name="branchParents"></param>
    /// <returns></returns>
    public ShareDefinition ToShareDefinition(ShareManager shareManager, List<ShareDefinition> branchParents)
    {
        var export = shareManager.GetNewOrExistingExportFor(Object);

        var properties = new Dictionary<string, object>();
        var relationshipProperties = new Dictionary<RelationshipAttribute, Guid>();

        var relationshipFinder = new AttributePropertyFinder<RelationshipAttribute>(Object);
        var noMappingFinder = new AttributePropertyFinder<NoMappingToDatabase>(Object);


        //for each property in the Object class
        foreach (var property in Object.GetType().GetProperties())
        {
            //if it's the ID column skip it
            if (property.Name == "ID")
                continue;

            //skip [NoMapping] columns
            if (noMappingFinder.GetAttribute(property) != null)
                continue;

            //skip IRepositories (these tell you where the object came from)
            if (typeof(IRepository).IsAssignableFrom(property.PropertyType))
                continue;

            var attribute = relationshipFinder.GetAttribute(property);

            //if it's a relationship to a shared object
            if (attribute is { Type: RelationshipType.SharedObject or RelationshipType.OptionalSharedObject })
            {
                var idOfParent = property.GetValue(Object);
                var typeOfParent = attribute.Cref;

                var parent = branchParents.SingleOrDefault(d => d.Type == typeOfParent && d.ID.Equals(idOfParent));

                //if the parent is not being shared along with us
                if (parent == null)
                {
                    //if a reference is required (i.e. not optional)
                    if (attribute.Type != RelationshipType.OptionalSharedObject)
                        throw new SharingException(
                            $"Property {property} on Type {Object.GetType()} is marked [Relationship] but we found no ShareDefinition amongst the current objects parents to satisfy this property");
                }
                else
                {
                    relationshipProperties.Add(attribute, parent.SharingGuid);
                }
            }
            else
            {
                properties.Add(property.Name, property.GetValue(Object));
            }
        }

        return new ShareDefinition(export.SharingUIDAsGuid, Object.ID, Object.GetType(), properties,
            relationshipProperties);
    }

    /// <summary>
    ///     Creates sharing exports (<see cref="ObjectExport" />) for the current <see cref="GatheredObject.Object" /> and all
    ///     <see cref="GatheredObject.Children" /> and
    ///     then serializes them as <see cref="ShareDefinition" />
    /// </summary>
    /// <param name="shareManager"></param>
    /// <returns></returns>
    public List<ShareDefinition> ToShareDefinitionWithChildren(ShareManager shareManager)
    {
        return ToShareDefinitionWithChildren(shareManager, new List<ShareDefinition>());
    }

    private List<ShareDefinition> ToShareDefinitionWithChildren(ShareManager shareManager,
        List<ShareDefinition> branchParents)
    {
        var me = ToShareDefinition(shareManager, branchParents);

        var toReturn = new List<ShareDefinition>();
        var parents = new List<ShareDefinition>(branchParents) { me };
        toReturn.Add(me);

        foreach (var child in Children)
            toReturn.AddRange(child.ToShareDefinitionWithChildren(shareManager, parents));

        return toReturn;
    }

    #region Equality

    protected bool Equals(GatheredObject other)
    {
        return Equals(Object, other.Object);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GatheredObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Object);
    }

    public object MasqueradingAs()
    {
        return Object;
    }

    public static bool operator ==(GatheredObject left, GatheredObject right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(GatheredObject left, GatheredObject right)
    {
        return !Equals(left, right);
    }

    #endregion

    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return Array.Empty<IHasDependencies>();
    }

    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return Children.ToArray();
    }
}