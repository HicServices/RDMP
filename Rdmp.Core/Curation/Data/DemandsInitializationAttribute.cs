// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Used to model Design Time initialization of IDataFlowComponents and DLE ProcessTasks (IAttacher etc).  Decorate
///     public properties of IDataFlowComponents
///     with this attribute to allow the user  to define values for the Pipeline when they build it.  Each Demand will be
///     serialised as a
///     PipelineComponentArgument/ProcessTaskArgument.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class DemandsInitializationAttribute : Attribute
{
    /// <summary>
    ///     User readable description of what they are supposed to supply as values for the decorated property, allowable
    ///     values etc
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    ///     Any special subcategory e.g. String might be <see cref="Data.DemandType.SQL" /> or it might just be a regular
    ///     string value.  If in doubt
    ///     use <see cref="Data.DemandType.Unspecified" /> (the default)
    /// </summary>
    public DemandType DemandType { get; set; }

    /// <summary>
    ///     The value to use if the user hasn't picked a value yet (created an <see cref="IArgument" />).  This will also be
    ///     the default value set
    ///     on any argument when it is created.
    /// </summary>
    public object DefaultValue { get; set; }


    /// <summary>
    ///     A optional text value that some controls use to add more context to the input. I.e. DemandType.SQL uses this to
    ///     display the clause text next to the input
    /// </summary>
    public string ContextText { get; set; }

    /// <summary>
    ///     True if the public property must have a value supplied by the user.  This is compatible with
    ///     <see cref="DefaultValue" />.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    ///     If the property being decorated is System.Type e.g. <code>public Type OperationType {get;set;}</code>.  Then this
    ///     specifies which Types the user can
    ///     select (anything derrived from this Type).  This lets you have the user pick a strategy for your plugin as long as
    ///     the strategies have blank/compatible
    ///     constructors.  You will have to decide how best to instantiate this Type yourself at runtime.
    /// </summary>
    public Type TypeOf { get; set; }

    /// <summary>
    ///     Marks a public property on an RDMP plugin class as editable by the user.  The user can pick a value at design time
    ///     for use with the plugin e.g. in
    ///     a <see cref="Pipeline" /> then when the pipeline is run your class will be instantiated and all properties will be
    ///     hydrated from the corresponding <see cref="IArgument" />s.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="demandType"></param>
    /// <param name="defaultValue"></param>
    /// <param name="typeOf"></param>
    /// <param name="mandatory"></param>
    public DemandsInitializationAttribute(string description, DemandType demandType = DemandType.Unspecified,
        object defaultValue = null, Type typeOf = null, bool mandatory = false)
    {
        Description = description;
        DemandType = demandType;
        DefaultValue = defaultValue;
        TypeOf = typeOf;
        Mandatory = mandatory;
    }
}