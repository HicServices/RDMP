using ReusableLibraryCode.Annotations;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface that allows you to be supported as a [DemandsInitialization] property of a DLE / pipeline component (see Argument) despite having a horribly complex Type.
    /// If your property Type too complex to be handled by the existing supported Types of Argument (See PermissableTypes) then you will have to use the ICustomUI system and
    /// create your own interface for allowing the user to configure it and write the code to serialize/deserialize it into a string yourself.
    /// 
    /// <para>If the complexity is due to subcomponents of the class e.g. 'RemoteDataFetcher' could have a Property 'Endpoint' of Type 'EndpointDefinition' you can instead 
    /// decorate the property 'Endpoint' with [DemandsNestedInitialization] and then decorate the properties of 'EndpointDefinition' with 'DemandsInitialization'.  Basically
    /// you don't want to use this interface if you can avoid it.</para>
    /// </summary>
    public interface ICustomUIDrivenClass
    {
        /// <summary>
        /// Hydrate the <see cref="ICustomUIDrivenClass"/> by deserializing the supplied string.  If there is no <see cref="IArgument"/> value configured yet
        /// then <see cref="value"/> may be null
        /// </summary>
        /// <param name="value"></param>
        void RestoreStateFrom([CanBeNull]string value);

        /// <summary>
        /// Persist the current state of the <see cref="ICustomUIDrivenClass"/> as a string.  This must be compatible with <see cref="RestoreStateFrom"/>
        /// </summary>
        /// <returns></returns>
        string SaveStateToString();
    }
}
