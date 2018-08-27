using System;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Records the user configured value of a property marked with [DemandsInitialization] declared on a data flow/dle component (including plugin components).
    ///  See Argument for full description.
    /// </summary>
    public interface IArgument:IMapsDirectlyToDatabaseTable,ISaveable
    {
        /// <summary>
        /// The name of the Property which this object stores the value of.  The Property should be decorated with [DemandsInitialization]
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Record of <see cref="DemandsInitializationAttribute.Description"/> as it was specified when the <see cref="IArgument"/> was created
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The string value for populating the class Property at runtime.  This is usually set to null or a default then adjusted by the user as needed to tailor
        /// the <see cref="IArgumentHost"/> class.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// The full Type name of the class Property this <see cref="IArgument"/> holds the runtime value for (See <see cref="IArgumentHost"/>)
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Change the current <see cref="Value"/> held by the <see cref="IArgument"/> to a new value (this must be a supported Type - See <see cref="Argument.PermissableTypes"/>)
        /// </summary>
        /// <param name="o"></param>
        void SetValue(object o);

        /// <summary>
        /// Parses the current <see cref="Value"/> into the <see cref="IArgument.Type"/> and returns it as a strongly typed object
        /// </summary>
        /// <returns></returns>
        object GetValueAsSystemType();
        
        /// <summary>
        /// Parses the current <see cref="Type"/> string into a <see cref="System.Type"/>
        /// </summary>
        /// <returns></returns>
        Type GetSystemType();

        /// <summary>
        /// Similar to <see cref="GetSystemType"/> except it will look for a non interface/abstract derrived class e.g. if <see cref="Type"/> is <see cref="ICatalogue"/> 
        /// it will return <see cref="Catalogue"/>
        /// </summary>
        /// <returns></returns>
        Type GetConcreteSystemType();

        /// <summary>
        /// Change the <see cref="Type"/> of the <see cref="IArgument"/> to the supplied <see cref="System.Type"/>.  This may make <see cref="Value"/> invalid.
        /// </summary>
        /// <param name="t"></param>
        void SetType(Type t);
    }
}