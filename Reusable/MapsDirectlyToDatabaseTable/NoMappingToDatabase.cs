using System;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Used to indicate when a property does not map to an underlying data table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class NoMappingToDatabase : Attribute
    {
    }
}