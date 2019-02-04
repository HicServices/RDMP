using System;

namespace MapsDirectlyToDatabaseTable.Attributes
{
    /// <summary>
    /// Indicates that the given parameter should be unique amongst other objects
    /// of the same type.  These should be enforced with a unique index/constraint 
    /// on the database level.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
    {
    }
}