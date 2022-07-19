// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleControls
{
    /// <summary>
    /// Allows saving of any DatabaseEntity.  When public properties are changed on the object the control automatically lights up and tracks the changes.  Clicking
    /// the Undo button will reset the DatabaseEntity to the state before the changes were made / when it was last saved.  Undo is a toggle from last saved state to 
    /// current user edit state and back again (i.e. not a tracked history).  In order to use an object saver button you should add it to an RDMPSingleDatabaseObjectControl
    /// and call SetupFor on the DatabaseObject.  You should also mark your control as ISaveableUI and implement the single method on that interface so that shortcuts
    /// are correctly routed to this control.
    /// </summary>
    public partial class ObjectSaverButton
    {
        private Bitmap _undoImage;
        private Bitmap _redoImage;

        private ToolStripButton btnSave  = new ToolStripButton("Save",FamFamFamIcons.disk);
        private ToolStripButton btnUndoRedo = new ToolStripButton("Undo", FamFamFamIcons.Undo);
        
        private RevertableObjectReport _undoneChanges;
        private IRDMPControl _parent;
        private IActivateItems _activator;

        public ObjectSaverButton()
        {
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnUndoRedo.Click += new System.EventHandler(this.btnUndoRedo_Click);
            
            _undoImage = FamFamFamIcons.Undo;
            _redoImage = FamFamFamIcons.Redo;

            btnUndoRedo.Image = _undoImage;
        }

        private DatabaseEntity _o;
        
        public event Action AfterSave;

        /// <summary>
        /// Function to carry out some kind of proceedure before the object is saved.  Return true if you want the save to carry on and be applied or false to abandon the save attempt.
        /// </summary>
        public event Func<DatabaseEntity,bool> BeforeSave;

        private bool _isEnabled;
        private bool _undo = true;
        
        public void SetupFor(IRDMPControl control, DatabaseEntity o, IActivateItems activator)
        {
            control.CommonFunctionality.Add(btnSave);
            control.CommonFunctionality.Add(btnUndoRedo);
            
            Form f = control as Form ?? ((Control)control).FindForm();

            if (f == null)
                throw new NotSupportedException("Cannot call SetupFor before the control has been added to its parent form");

            _parent = control;

            Enable(false);

            //if it is a fresh instance
            if(!ReferenceEquals(_o,o))
            {
                //subscribe to property change events
                if(_o != null)
                    _o.PropertyChanged -= PropertyChanged;
                _o = o;
                _o.PropertyChanged += PropertyChanged;
            }
            
            //already set up before
            if (_activator != null)
                return;
            
            _activator = activator;

            f.Enter += ParentForm_Enter;
            
            //the first time it is set up it could still be out of date!
            CheckForOutOfDateObjectAndOfferToFix();
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
            var c = (Control)_parent;
            if (c.InvokeRequired)
            {
               c.Invoke(new MethodInvoker(() => Enable(b)));
                return;
            }

            _parent.SetUnSavedChanges(b);
            
            btnSave.Enabled = b;
            btnUndoRedo.Enabled = b;

            _isEnabled = b;
        }
        
        
        public void Save()
        {
            if(_o == null)
                throw new Exception("Cannot Save because ObjectSaverButton has not been set up yet, call SetupFor first (e.g. in your SetDatabaseObject method) ");
            
            if(BeforeSave != null)
                if (!BeforeSave(_o))
                    return;

            _o.SaveToDatabase();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_o));
            Enable(false);

            SetReadyToUndo();

            if(AfterSave != null)
                AfterSave();
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        

        private void btnUndoRedo_Click(object sender, EventArgs e)
        {
            if (_undo)
                Undo();
            else
                Redo();
        }

        public void Redo()
        {
            if (_undoneChanges != null && _undoneChanges.Evaluation == ChangeDescription.DatabaseCopyDifferent)
            {
                foreach (var difference in _undoneChanges.Differences)
                    difference.Property.SetValue(_o, difference.LocalValue);
                
                SetReadyToUndo();
            }
        }

        public void Undo()
        {
            var changes = _o.HasLocalChanges();

            //no changes anyway user must have made a change and then unapplyed it
            if (changes.Evaluation != ChangeDescription.DatabaseCopyDifferent)
                return;

            //reset to the database state
            _o.RevertToDatabaseState();

            //publish that the object has changed
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_o));

            //show the redo image
            SetReadyToRedo(changes);

            //publish probably disabled us
            Enable(true);
        }

        private void SetReadyToRedo(RevertableObjectReport changes)
        {
            //record the changes prior to the revert
            _undoneChanges = changes; 

            _undo = false;
            btnUndoRedo.Image = _redoImage;
            btnUndoRedo.Text = "Redo";
        }
        private void SetReadyToUndo()
        {
            _undo = true;
            btnUndoRedo.Image = _undoImage;
            _undoneChanges = null;
            btnUndoRedo.Text = "Undo";
        }

        public void CheckForOutOfDateObjectAndOfferToFix()
        {
            if (IsDifferent())
                if(
                    //if we didn't think there were changes
                    !_isEnabled &&
                    
                    //but there are!
                    _activator.ShouldReloadFreshCopy(_o))
                {
                    _o.RevertToDatabaseState();
                    
                    //if we are not in the middle of a publish already
                    if (!_activator.RefreshBus.PublishInProgress)
                        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_o));
                }
        }

        private bool IsDifferent()
        {
            if (_o == null)
                return false;

            var changes = _o.HasLocalChanges();
            //are there changes
            if (changes.Evaluation == ChangeDescription.DatabaseCopyDifferent)
                return changes.Differences.Any(); //are the changes to properties

            return false;
        }

        public void CheckForUnsavedChangesAnOfferToSave()
        {
            // If there is no object or it does not exist don't try to save it
            if (_o == null || !_o.Exists())
                return;
            
            if (_isEnabled)
                if (_activator.YesNo("Save Changes To '" + _o + "'?", "Save Changes"))
                    Save();
                else
                    _o.RevertToDatabaseState();
        }

    }
}
