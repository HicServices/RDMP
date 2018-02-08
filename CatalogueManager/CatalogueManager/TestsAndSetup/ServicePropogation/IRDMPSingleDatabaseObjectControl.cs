using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public interface IRDMPSingleDatabaseObjectControl : IContainerControl, INamedTab, IConsultableBeforeClosing
    {
        DatabaseEntity DatabaseObject { get; }

        void SetDatabaseObject(IActivateItems activator, DatabaseEntity databaseObject);
        Type GetTypeOfT();
    }
}