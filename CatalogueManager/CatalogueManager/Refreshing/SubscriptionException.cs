using System;

namespace CatalogueManager.Refreshing
{
    public class SubscriptionException : Exception
    {
        public SubscriptionException(string message): base(message)
        {
            
        }
    }
}