namespace CatalogueLibrary.QueryBuilding.Parameters
{
    /// <summary>
    /// Describes the stage at which the ParameterManager has reached.  This starts on construction at which time you can set the Globals, once parameter discovery has
    /// started you cannot add more Globals because they are considered when deciding whether or not to add a given ISqlParameter found at a lower level (e.g. if you 
    /// have a global @hb then finding @hb on an IFilter can be ignored since there is the global defined).
    /// 
    /// <para>Finalized occurs when you resolve the parameters collected into a single difinitive set (merging duplicates / overrides etc).</para>
    /// </summary>
    public enum ParameterManagerLifecycleState
    {
        /// <summary>
        /// More global parameters can be added
        /// </summary>
        AllowingGlobals,

        /// <summary>
        /// Parameter discovery has started, no more globals can be added
        /// </summary>
        ParameterDiscovery,

        /// <summary>
        /// The final parameter list has been generated and no more parameters can be added
        /// </summary>
        Finalized

    }
}
