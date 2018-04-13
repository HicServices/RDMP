using System;
using System.Runtime.CompilerServices;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Used to indicate when an ID column contains the ID of another RDMP object.  Decorate the foreign key object. This can be involve going 
    /// between databases or even servers e.g. between DataExport and Catalogue libraries or between Catalogue and plugin databases
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class RelationshipAttribute : Attribute
    {
        /// <summary>
        /// The other class whose ID is stored in decorated property
        /// </summary>
        public Type Cref { get; set; }

        /// <summary>
        /// The decorated property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Declares that the decorated property contains the ID of the specified Type of object
        /// </summary>
        /// <param name="cref"></param>
        /// <param name="propertyName"></param>
        public RelationshipAttribute(Type cref,[CallerMemberName] string propertyName=null)
        {
            Cref = cref;
            PropertyName = propertyName;
        }
    }
}