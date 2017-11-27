using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;

namespace CatalogueManager.Refreshing
{
    public class SelfDestructProtocol<T> : IRefreshBusSubscriber where T : DatabaseEntity
    {
        private readonly IActivateItems _activator;
        public RDMPSingleDatabaseObjectControl<T> User { get; private set; }
        public T OriginalObject { get; set; }

        public SelfDestructProtocol(RDMPSingleDatabaseObjectControl<T> user,IActivateItems activator, T originalObject)
        {
            _activator = activator;
            User = user;
            OriginalObject = originalObject;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
           //implementation of the anoymous callback
            T o = e.Object as T;
                
            //don't respond to events raised by the user themself!
            if(sender == User)
                return;

            //if the original object does not exist anymore (could be a CASCADE event so we do have to check it every time regardless of what object type is refreshing)
            if (!OriginalObject.Exists())//object no longer exists!
            {
                var parent = User.ParentForm;
                if (parent != null && !parent.IsDisposed)
                    parent.Close();//self destruct because object was deleted

                return;
            }
            if (o != null && o.ID == OriginalObject.ID && o.GetType() == OriginalObject.GetType())//object was refreshed, probably an update to some fields in it
                User.SetDatabaseObject(_activator, o); //give it the new object
        }

    }
}