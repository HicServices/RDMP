namespace ReusableLibraryCode
{
    /// <summary>
    /// Indicates that an object is part of a network of dependant objects (e.g. CatalogueItem depends on Catalogue).  Ideally you should try to 
    /// list all IHasDependencies in a network of objects such that when A says it DependsOn B then B should report that A is DependingOnThis (B)
    /// but if theres a few missing links it won't end the world.  The reason to do this is so that from any point we can find all related objects
    /// up and down the hierarchies.
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