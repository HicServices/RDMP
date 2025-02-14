// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Annotations;

/// <summary>
/// Indicates that the value of the marked element could be <c>null</c> sometimes,
/// so the check for <c>null</c> is necessary before its usage
/// </summary>
/// <example><code>
/// [CanBeNull] public object Test() { return null; }
/// public void UseTest() {
///   var p = Test();
///   var s = p.ToString(); // Warning: Possible 'System.NullReferenceException'
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Parameter |
    AttributeTargets.Property | AttributeTargets.Delegate |
    AttributeTargets.Field)]
public sealed class CanBeNullAttribute : Attribute;

/// <summary>
/// Indicates that the value of the marked element could never be <c>null</c>
/// </summary>
/// <example><code>
/// [NotNull] public object Foo() {
///   return null; // Warning: Possible 'null' assignment
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Parameter |
    AttributeTargets.Property | AttributeTargets.Delegate |
    AttributeTargets.Field)]
public sealed class NotNullAttribute : Attribute;

/// <summary>
/// Indicates that the method is contained in a type that implements
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> interface
/// and this method is used to notify that some property value changed
/// </summary>
/// <remarks>
/// The method should be non-static and conform to one of the supported signatures:
/// <list>
/// <item><c>NotifyChanged(string)</c></item>
/// <item><c>NotifyChanged(params string[])</c></item>
/// <item><c>NotifyChanged{T}(Expression{Func{T}})</c></item>
/// <item><c>NotifyChanged{T,U}(Expression{Func{T,U}})</c></item>
/// <item><c>SetProperty{T}(ref T, T, string)</c></item>
/// </list>
/// </remarks>
/// <example><code>
/// public class Foo : INotifyPropertyChanged {
///   public event PropertyChangedEventHandler PropertyChanged;
///   [NotifyPropertyChangedInvocator]
///   protected virtual void NotifyChanged(string propertyName) { ... }
///
///   private string _name;
///   public string Name {
///     get { return _name; }
///     set { _name = value; NotifyChanged("LastName"); /* Warning */ }
///   }
/// }
/// </code>
/// Examples of generated notifications:
/// <list>
/// <item><c>NotifyChanged("Property")</c></item>
/// <item><c>NotifyChanged(() =&gt; Property)</c></item>
/// <item><c>NotifyChanged((VM x) =&gt; x.Property)</c></item>
/// <item><c>SetProperty(ref myField, value, "Property")</c></item>
/// </list>
/// </example>
[AttributeUsage(AttributeTargets.Method)]
public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
{
    public NotifyPropertyChangedInvocatorAttribute()
    {
    }

    public NotifyPropertyChangedInvocatorAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; private set; }
}

/// <summary>
/// Indicates that the marked symbol is used implicitly
/// (e.g. via reflection, in external library), so this symbol
/// will not be marked as unused (as well as by other usage inspections)
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class UsedImplicitlyAttribute : Attribute
{
    public UsedImplicitlyAttribute(
        ImplicitUseKindFlags useKindFlags = ImplicitUseKindFlags.Default,
        ImplicitUseTargetFlags targetFlags = ImplicitUseTargetFlags.Default)
    {
        UseKindFlags = useKindFlags;
        TargetFlags = targetFlags;
    }

    public ImplicitUseKindFlags UseKindFlags { get; }
    public ImplicitUseTargetFlags TargetFlags { get; }
}

[Flags]
public enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

    /// <summary>Only entity marked with attribute considered used</summary>
    Access = 1,

    /// <summary>Indicates implicit assignment to a member</summary>
    Assign = 2,

    /// <summary>
    /// Indicates implicit instantiation of a type with fixed constructor signature.
    /// That means any unused constructor parameters won't be reported as such.
    /// </summary>
    InstantiatedWithFixedConstructorSignature = 4,

    /// <summary>Indicates implicit instantiation of a type</summary>
    InstantiatedNoFixedConstructorSignature = 8
}

/// <summary>
/// Specify what is considered used implicitly
/// </summary>
[Flags]
public enum ImplicitUseTargetFlags
{
    Itself = 1,
    Default = Itself,

    /// <summary>Members of entity marked with attribute are considered used</summary>
    Members = 2,

    /// <summary>Entity marked with attribute and all its members considered used</summary>
    WithMembers = Itself | Members
}