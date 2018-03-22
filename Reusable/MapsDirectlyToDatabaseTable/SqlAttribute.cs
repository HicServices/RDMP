using System;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Used to indicate a property that contains sql e.g. Where logic, Select logic etc.  Using this property makes the Attribute
    /// 'find and replaceable' through the FindAndReplaceUI
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SqlAttribute : Attribute
    {
    }
}