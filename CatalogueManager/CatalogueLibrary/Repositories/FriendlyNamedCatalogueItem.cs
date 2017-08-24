namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// This class exists for performance reasons, it is constructed by CatalogueRepository.GetFullNameOfAllCatalogueItems() and is used to generate a list of ID +
    ///  'cataloguename'.'catalogueitemname' this could be thousands of records (every catalogueitem) so rather than assembling the dictionary from the objects we are
    /// running a bespoke SQL query against the catalogue database to join up all the names in the database as a result set. 
    /// </summary>
    public class FriendlyNamedCatalogueItem
    {
        public FriendlyNamedCatalogueItem(int id, string friendlyName)
        {
            ID = id;
            FriendlyName = friendlyName;
        }

        public string FriendlyName { get; internal set; }
        public int ID { get; internal set; }

        public override string ToString()
        {
            return FriendlyName;
        }
    }
}