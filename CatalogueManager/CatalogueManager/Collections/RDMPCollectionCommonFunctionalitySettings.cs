namespace CatalogueManager.Collections
{
    public class RDMPCollectionCommonFunctionalitySettings
    {
        /// <summary>
        /// True to add an extra column to the tree view which shows if / allows changing the favourite objects status of objects.
        ///  <para>Defaults to true</para>
        /// </summary>
        public bool AddFavouriteColumn { get; set; }

        /// <summary>
        /// True if the collection should allow the user to hide all objects but the pinned one (and it's parental hierarchy).  False
        /// to prevent the user doing so.  Set this to false if you have a collection that only really shows one object hierarchy and
        /// hiding stuff doesn't make sense.
        ///  <para>Defaults to true</para>
        /// </summary>
        public bool AllowPinning { get; set; }

        /// <summary>
        /// True to add an extra column (not visible by default) to the tree view which the ID property of objects that are <see cref="IMapsDirectlyToDatabaseTable"/>
        ///  <para>Defaults to true</para>
        /// </summary>
        public bool AddIDColumn { get; set; }

        /// <summary>
        /// False to automatically set up tree hierarchy children based on the <see cref="CatalogueLibrary.Providers.ICoreChildProvider"/> in the
        /// <see cref="CatalogueManager.ItemActivation.IActivateItems"/> at construction time.  True if you plan to handle object children yourself
        ///  <para>Defaults to false</para>
        /// </summary>
        public bool SuppressChildrenAdder { get; set; }

        /// <summary>
        /// False to perform the default object activation behaviour on double click.  True if you plan to handle it yourself with a custom action.
        /// 
        /// <para>Defaults to false</para>
        /// </summary>
        public bool SuppressActivate { get; set; }

        public RDMPCollectionCommonFunctionalitySettings()
        {
            AddFavouriteColumn = true;
            AllowPinning = true;
            AddIDColumn = true;
            SuppressChildrenAdder = false;
            SuppressActivate = false;
        }
    }
}