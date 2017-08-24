using System;
using System.Security.Cryptography.X509Certificates;

namespace ReusableLibraryCode
{
    public interface ILifelineable
    {
        DateTime? Lifeline { get; set; }
        void TickLifeline();
        void RefreshLifelinePropertyFromDatabase();
    }
}