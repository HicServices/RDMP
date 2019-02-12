using System;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Used to indicate when a property that should not be overwritten when importing descriptions from a share source (e.g. Dublin Core, ShareDefinition etc)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DoNotImportDescriptionsAttribute : Attribute
    {
        /// <summary>
        /// Changes behaviour of import:
        /// <para>True - Overwrite if the current value is null</para>
        /// <para>False - Never overwrite in any circumstances</para>
        /// </summary>
        public bool AllowOverwriteIfBlank { get; set; }
    }
}