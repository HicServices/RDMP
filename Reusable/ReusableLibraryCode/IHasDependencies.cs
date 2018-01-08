namespace ReusableLibraryCode
{
    /// <summary>
    /// Indicates that an object is part of a network of dependant objects (e.g. CatalogueItem depends on Catalogue).  Ideally you should try to 
    /// list all IHasDependencies in a network of objects such that when A lists B as a DependsOn then B should also include A as DependingOn but if
    /// theres a few missing links it won't end the world.
    /// </summary>
    public interface IHasDependencies
    {
        /// <summary>
        /// Objects which this class instance cannot exist without (things further up the dependency hierarchy)
        /// </summary>
        /// <returns></returns>
        IHasDependencies[] GetObjectsThisDependsOn();

        /// <summary>
        /// Objects which this class knows depend on it (things further down the dependency hierarchy).
        /// </summary>
        /// <returns></returns>
        IHasDependencies[] GetObjectsDependingOnThis();
    }

}