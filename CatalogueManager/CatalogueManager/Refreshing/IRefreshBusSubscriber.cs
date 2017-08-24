namespace CatalogueManager.Refreshing
{
    public interface IRefreshBusSubscriber
    {
        void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e);
    }
}