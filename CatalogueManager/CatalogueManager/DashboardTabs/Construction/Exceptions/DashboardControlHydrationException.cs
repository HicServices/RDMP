using System;

namespace CatalogueManager.DashboardTabs.Construction.Exceptions
{
    public class DashboardControlHydrationException : Exception
    {
        public DashboardControlHydrationException(string s, Exception exception):base(s,exception)
        {
        }
    }
}