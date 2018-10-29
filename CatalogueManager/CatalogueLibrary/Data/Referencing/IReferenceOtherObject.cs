using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Referencing
{
    /// <summary>
    /// Interface for all objects which reference a single other object e.g. <see cref="Favourite"/>
    /// </summary>
    public interface IReferenceOtherObject
    {
        /// <summary>
        /// Returns true if the object being referenced is of Type <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsReferenceTo(Type type);

        /// <summary>
        /// Returns true if the object being referenced is <paramref name="o"/>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool IsReferenceTo(IMapsDirectlyToDatabaseTable o);
    }
}
