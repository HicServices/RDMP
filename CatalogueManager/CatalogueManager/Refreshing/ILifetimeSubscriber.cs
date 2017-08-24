using System.Windows.Forms;

namespace CatalogueManager.Refreshing
{
    public interface ILifetimeSubscriber:IContainerControl,IRefreshBusSubscriber
    {
    }
}