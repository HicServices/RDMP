using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Refreshing;

namespace CatalogueManager.SimpleControls
{
    [System.ComponentModel.DesignerCategory("")]
    public class ObjectSaverButton : Button, IRefreshBusSubscriber
    {
        private DatabaseEntity _o;
        private RefreshBus _refreshBus;
        
        public event Action AfterSave;

        /// <summary>
        /// Function to carry out some kind of proceedure before the object is saved.  Return true if you want the save to carry on and be applied or false to abandon the save attempt.
        /// </summary>
        public event Func<DatabaseEntity,bool> BeforeSave;

        public void SetupFor(DatabaseEntity o, RefreshBus refreshBus)
        {
            //already set up before
            if(_o != null)
                return;

            _o = o;
            _refreshBus = refreshBus;
            _refreshBus.Subscribe(this);
            o.PropertyChanged += PropertyChanged;
            Enabled = false;
            Text = "&Save";
        }
        
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Enabled = true;
        }

        protected override void OnClick(EventArgs e)
        {
            Save();
            base.OnClick(e);
            
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(_refreshBus != null)
                _refreshBus.Unsubscribe(this);
        }

        public void Save()
        {
            if(BeforeSave!= null)
                if (!BeforeSave(_o))
                    return;

            _o.SaveToDatabase();
            _refreshBus.Publish(this,new RefreshObjectEventArgs(_o));
            Enabled = false;
            
            if(AfterSave != null)
                AfterSave();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //pick up new instances of the object from the database
            if (e.Object.Equals(_o))
            {
                _o.PropertyChanged -= PropertyChanged;//unsubscribe from local property change events on stale object
                _o = e.Object;  //record the new fresh object
                _o.PropertyChanged += PropertyChanged;//and subscribe to it's events
            }
        }

        public void ForceDirty()
        {
            Enabled = true;
        }
    }
}
