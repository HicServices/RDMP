using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public interface IRDMPSingleDatabaseObjectControl : IContainerControl, INamedTab
    {
        DatabaseEntity DatabaseObject { get; }

        void SetDatabaseObject(IActivateItems activator, DatabaseEntity databaseObject);
        Type GetTypeOfT();
    }
}