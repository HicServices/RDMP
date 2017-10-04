using System;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Used by classes to indicate that a complex POCO property should have all its properties initialized from a ProcessTaskArgument
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class DemandsNestedInitializationAttribute : System.Attribute
    {
    }
}