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
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Revertable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;

namespace CatalogueManager.SimpleControls
{
    public partial class ObjectSaverButton : UserControl,IRefreshBusSubscriber
    {
        private Bitmap _undoImage;
        private Bitmap _redoImage;

        public ObjectSaverButton()
        {
            InitializeComponent();
            _undoImage = FamFamFamIcons.Undo;
            _redoImage = FamFamFamIcons.Redo;

            btnUndoRedo.Image = _undoImage;
        }

        private DatabaseEntity _o;
        private RefreshBus _refreshBus;
        
        public event Action AfterSave;

        /// <summary>
        /// Function to carry out some kind of proceedure before the object is saved.  Return true if you want the save to carry on and be applied or false to abandon the save attempt.
        /// </summary>
        public event Func<DatabaseEntity,bool> BeforeSave;

        private bool _isEnabled;
        private bool _undo = true;
        

        public void SetupFor(DatabaseEntity o, RefreshBus refreshBus)
        {
            //already set up before
            if(_o != null)
                return;

            _o = o;
            _refreshBus = refreshBus;
            _refreshBus.Subscribe(this);
            o.PropertyChanged += PropertyChanged;

            if (ParentForm == null)
                throw new NotSupportedException("Cannot call SetupFor before the control has been added to it's parent form");

            ParentForm.Enter += ParentForm_Enter;
            ParentForm.Leave += ParentFormOnLeave;
            Enable(false);
            Text = "&Save";
        }

        private void ParentFormOnLeave(object sender, EventArgs eventArgs)
        {
            CheckForUnsavedChangesAnOfferToSave();
        }

        void ParentForm_Enter(object sender, EventArgs e)
        {
            CheckForOutOfDateObjectAndOfferToFix();
        }
        
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Enable(true);
        }

        public void Enable(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => Enable(b)));
                return;
            }

            btnSave.Enabled = b;
            btnUndoRedo.Enabled = b;

            _isEnabled = b;
        }
        
        
        public void Save()
        {
            if(BeforeSave!= null)
                if (!BeforeSave(_o))
                    return;

            _o.SaveToDatabase();
            _refreshBus.Publish(this,new RefreshObjectEventArgs(_o));
            Enable(false);

            SetReadyToUndo();

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
            var changes = _o.HasLocalChanges();
            var isDifferent = changes.Evaluation == ChangeDescription.DatabaseCopyDifferent;

            btnSave.Enabled = isDifferent;
            btnUndoRedo.Enabled = isDifferent;
            
            _isEnabled = isDifferent;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private RevertableObjectReport _undoneChanges;

        private void btnUndoRedo_Click(object sender, EventArgs e)
        {
            if(_undo)
            {
                var changes = _o.HasLocalChanges();

                //no changes anyway user must have made a change and then unapplyed it
                if (changes.Evaluation != ChangeDescription.DatabaseCopyDifferent)
                    return;

                //reset to the database state
                _o.RevertToDatabaseState();

                //publish that the object has changed
                _refreshBus.Publish(this, new RefreshObjectEventArgs(_o));

                //show the redo image
                SetReadyToRedo(changes);

                //publish probably disabled us
                Enable(true);

            }
            else
            {
                if(_undoneChanges != null && _undoneChanges.Evaluation == ChangeDescription.DatabaseCopyDifferent)
                {
                    foreach (var difference in _undoneChanges.Differences)
                        difference.Property.SetValue(_o, difference.LocalValue);

                    _refreshBus.Publish(this, new RefreshObjectEventArgs(_o));
                    
                    SetReadyToUndo();
                }
            }
        }

        private void SetReadyToRedo(RevertableObjectReport changes)
        {
            //record the changes prior to the revert
            _undoneChanges = changes; 

            _undo = false;
            btnUndoRedo.Image = _redoImage;
        }
        private void SetReadyToUndo()
        {
            _undo = true;
            btnUndoRedo.Image = _undoImage;
            _undoneChanges = null;
        }

        public void CheckForOutOfDateObjectAndOfferToFix()
        {
            if(_o == null)
                return;

            var differences = _o.HasLocalChanges();

            if (differences.Evaluation == ChangeDescription.DatabaseCopyDifferent)
                if(MessageBox.Show(_o + " is out of date with database, would you like to reload a fresh copy?","Object Changed",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _o.RevertToDatabaseState();
                    _refreshBus.Publish(this, new RefreshObjectEventArgs(_o));
                }
        }

        public void CheckForUnsavedChangesAnOfferToSave()
        {
            if (_o == null)
                return;

            if (_isEnabled)
                if (MessageBox.Show("Save Changes To '" + _o + "'?", "Save Changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Save();
                else
                    _o.RevertToDatabaseState();
        }

    }
}
