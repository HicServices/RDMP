using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Revertable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueManager.SimpleControls
{
    public partial class ObjectSaverButton : UserControl,IRefreshBusSubscriber
    {
        public ObjectSaverButton()
        {
            InitializeComponent();
        }

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
            DisableAll();
            Text = "&Save";
        }

        private void DisableAll()
        {
            btnSave.Enabled = false;
            btnDiscard.Enabled = false;
            btnViewDifferences.Enabled = false;
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckForLocalChanges();
        }
        
        public void Save()
        {
            if(BeforeSave!= null)
                if (!BeforeSave(_o))
                    return;

            _o.SaveToDatabase();
            _refreshBus.Publish(this,new RefreshObjectEventArgs(_o));
            DisableAll();
            
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

            //anytime any publish event ever fires (not just to our object)
            CheckForLocalChanges();

        }

        private void CheckForLocalChanges()
        {
            var different = _o.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent;

            btnSave.Enabled = different;
            btnViewDifferences.Enabled = different;
            btnDiscard.Enabled = different;
        }

        public void ForceDirty()
        {
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnViewDifferences_Click(object sender, EventArgs e)
        {
            OfferChanceToSaveDialog.ShowIfRequired(_o);
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            _o.RevertToDatabaseState();
            _refreshBus.Publish(this, new RefreshObjectEventArgs(_o));
        }
    }
}
