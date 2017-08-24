namespace CatalogueLibrary.Data.DataLoad
{
    public interface ICustomUIDrivenClass
    {
        void RestoreStateFrom(string value);
        string SaveStateToString();
    }
}
