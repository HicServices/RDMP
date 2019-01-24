using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all <see cref="PermissionWindow"/> objects.  These are windows of time in which operations are permitted / forbidden.
    /// </summary>
    public class AllPermissionWindowsNode:SingletonNode,IOrderable
    {
        public AllPermissionWindowsNode() : base("Permission Windows")
        {
        }

        public int Order { get { return 0; } set{} }
    }
}
