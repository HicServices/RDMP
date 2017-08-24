using System;

namespace ReusableUIComponents
{
    /// <summary>
    /// Used to indicate when a property does not map to an underlying data table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TechnicalUI : Attribute
    {
    }
}