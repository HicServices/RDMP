namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface that allows you to be supported as a [DemandsInitialization] property of a DLE / pipeline component (see Argument) despite having a horribly complex Type.
    /// If your property Type too complex to be handled by the existing supported Types of Argument (See PermissableTypes) then you will have to use the ICustomUI system and
    /// create your own interface for allowing the user to configure it and write the code to serialize/deserialize it into a string yourself.
    /// 
    /// If the complexity is due to subcomponents of the class e.g. 'RemoteDataFetcher' could have a Property 'Endpoint' of Type 'EndpointDefinition' you can instead 
    /// decorate the property 'Endpoint' with [DemandsNestedInitialization] and then decorate the properties of 'EndpointDefinition' with 'DemandsInitialization'.  Basically
    /// you don't want to use this interface if you can avoid it.
    /// </summary>
    public interface ICustomUIDrivenClass
    {
        void RestoreStateFrom(string value);
        string SaveStateToString();
    }
}
