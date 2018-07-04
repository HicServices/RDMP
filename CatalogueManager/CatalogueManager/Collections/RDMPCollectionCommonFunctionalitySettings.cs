namespace CatalogueManager.Collections
{
    public class RDMPCollectionCommonFunctionalitySettings
    {
        public bool AddFavouriteColumn { get; set; }
        public bool AllowPinning { get; set; }
        public bool AddIDColumn { get; set; }
        public bool SuppressChildrenAdder { get; set; }

        public RDMPCollectionCommonFunctionalitySettings()
        {
            AddFavouriteColumn = true;
            AllowPinning = true;
            AddIDColumn = true;
            SuppressChildrenAdder = false;
        }
    }
}