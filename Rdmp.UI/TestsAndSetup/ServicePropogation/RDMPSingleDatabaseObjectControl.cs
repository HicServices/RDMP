// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.Theme;


namespace Rdmp.UI.TestsAndSetup.ServicePropogation
{
    /// <summary>
    /// TECHNICAL: base abstract class for all Controls which are concerned with a single root DatabaseEntity e.g. AggregateGraph is concerned only with an AggregateConfiguration
    /// and it's children.  The reason this class exists is to streamline lifetime publish subscriptions (ensuring multiple tabs editting one anothers database objects happens 
    /// in a seamless a way as possible). 
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TechnicalUI]
    public abstract class RDMPSingleDatabaseObjectControl<T> : RDMPUserControl, IRDMPSingleDatabaseObjectControl where T : DatabaseEntity
    {
        private Control _colorIndicator;
        private Label _readonlyIndicator;

        private BinderWithErrorProviderFactory _binder;

        protected ObjectSaverButton ObjectSaverButton1 = new ObjectSaverButton();

        public DatabaseEntity DatabaseObject { get; private set; }
        protected RDMPCollection AssociatedCollection = RDMPCollection.None;

        /// <summary>
        /// True if the hosted <see cref="DatabaseObject"/> <see cref="IMightBeReadOnly.ShouldBeReadOnly"/>.  This property is detected and update during SetDatabaseObject so use it only after this call has been made
        /// </summary>
        public bool ReadOnly { get; set; }

        protected RDMPSingleDatabaseObjectControl()
        {
            CommonFunctionality.ToolStripAddedToHost += CommonFunctionality_ToolStripAddedToHost;
        }

        public virtual void SetDatabaseObject(IActivateItems activator, T databaseObject)
        {
            SetItemActivator(activator);
            Activator.RefreshBus.EstablishSelfDestructProtocol(this,activator,databaseObject);
            DatabaseObject = databaseObject;

            CommonFunctionality.ClearToolStrip();

            if(_colorIndicator == null && AssociatedCollection != RDMPCollection.None)
            {
                var colorProvider = new BackColorProvider();
                _colorIndicator = new Control();
                _colorIndicator.Dock = DockStyle.Top;
                _colorIndicator.Location = new Point(0, 0);
                _colorIndicator.Size = new Size(150, BackColorProvider.IndiciatorBarSuggestedHeight);
                _colorIndicator.TabIndex = 0;
                _colorIndicator.BackColor = colorProvider.GetColor(AssociatedCollection);
                this.Controls.Add(this._colorIndicator);
            }

            if (_readonlyIndicator == null)
            {
                _readonlyIndicator = new Label();
                _readonlyIndicator.Dock = DockStyle.Top;
                _readonlyIndicator.Location = new Point(0, 0);
                _readonlyIndicator.Size = new Size(150, 20);
                _readonlyIndicator.TabIndex = 0;
                _readonlyIndicator.TextAlign = ContentAlignment.MiddleLeft;
                _readonlyIndicator.BackColor = System.Drawing.SystemColors.HotTrack;
                _readonlyIndicator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _readonlyIndicator.ForeColor = System.Drawing.Color.Moccasin;

            }

            if (databaseObject is IMightBeReadOnly ro)
            {
                if (ro.ShouldBeReadOnly(out string reason))
                {
                    _readonlyIndicator.Text = reason;
                    this.Controls.Add(this._readonlyIndicator);
                    ReadOnly = true;
                }
                else
                {
                    //removing it allows us to handle refreshes (where something becomes unfrozen for example)
                    this.Controls.Remove(this._readonlyIndicator);
                    ReadOnly = false;
                }
            }

            if (_binder == null)
                _binder = new BinderWithErrorProviderFactory(activator);

            SetBindings(_binder, databaseObject);
            
            if(this is ISaveableUI)
                ObjectSaverButton1.SetupFor(this, databaseObject, activator);
            
            var gotoFactory = new GoToCommandFactory(activator);
            foreach (var cmd in gotoFactory.GetCommands(databaseObject).OfType<ExecuteCommandShow>())
            {
                cmd.UseIconAndTypeName = true;
                cmd.FetchDestinationObjects();
                CommonFunctionality.Add(cmd);
            }
        }

        void CommonFunctionality_ToolStripAddedToHost(object sender, EventArgs e)
        {
            if (_colorIndicator != null)
                _colorIndicator.SendToBack();
        }

        protected virtual void SetBindings(BinderWithErrorProviderFactory rules, T databaseObject)
        {
            
        }

        /// <summary>
        /// Performs data binding using default parameters (OnPropertyChanged), no formatting etc.  Getter must be a
        /// property of <see cref="DatabaseObject"/>
        /// </summary>
        /// <param name="c"></param>
        /// <param name="propertyName"></param>
        /// <param name="dataMember"></param>
        /// <param name="getter"></param>
        /// <param name="formattingEnabled"></param>
        /// <param name="updateMode"></param>
        protected void Bind(Control c, string propertyName, string dataMember, Func<T, object> getter, bool formattingEnabled = true,DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            var box = c as ComboBox;

            //workaround for only comitting lists on loose focus
            if (box != null && box.DropDownStyle == ComboBoxStyle.DropDownList && propertyName.Equals("SelectedItem"))
            {
                box.SelectionChangeCommitted += (s,e)=>box.DataBindings["SelectedItem"].WriteValue();
            }
            
            _binder.Bind(c, propertyName, (T)DatabaseObject, dataMember, formattingEnabled, updateMode, getter);
        }


        /// <summary>
        /// Parses the datetime out of the <paramref name="tb"/> with blank being null.  If the string doesn't parse
        /// then the text will turn red.
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="action">Method to call if a valid DateTime is entered into the text box.  Called with null if text box is blank</param>
        protected void SetDate(TextBox tb, Action<DateTime?> action)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    action(null);
                    return;
                }

                DateTime dateTime = DateTime.Parse(tb.Text);
                action(dateTime);

                tb.ForeColor = Color.Black;

            }
            catch (Exception)
            {
                tb.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Parses the Uri out of the <paramref name="tb"/> with blank being null.  If the string doesn't parse
        /// then the text will turn red.
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="action">Method to call if a valid Uri is entered into the text box.  Called with null if text box is blank</param>
        protected void SetUrl(TextBox tb, Action<Uri> action)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    action(null);
                    return;
                }

                Uri u = new Uri(tb.Text);
                action(u);
                tb.ForeColor = Color.Black;

            }
            catch (UriFormatException)
            {
                tb.ForeColor = Color.Red;
            }
        }

        public void SetDatabaseObject(IActivateItems activator, DatabaseEntity databaseObject)
        {
            SetDatabaseObject(activator,(T)databaseObject);
        }

        public Type GetTypeOfT()
        {
            return typeof (T);
        }

        public virtual string GetTabName()
        {
            var named = DatabaseObject as INamed;

            if (named != null)
                return named.Name;


            if (DatabaseObject != null)
                return DatabaseObject.ToString();

            return "Unamed Tab";
        }

        public virtual string GetTabToolTip()
        {
            return null;
        }

        /// <summary>
        /// Triggers an application refresh because a change has been made to <paramref name="e"/>
        /// </summary>
        public void Publish(IMapsDirectlyToDatabaseTable e)
        {
            Activator.Publish(e);
        }

        /// <summary>
        /// Triggers an application refresh because a change has been made to the forms main <see cref="DatabaseObject"/>
        /// </summary>
        public void Publish()
        {
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(DatabaseObject));
        }

        /// <summary>
        /// Triggers a refresh only of this form (calls <see cref="SetDatabaseObject(Rdmp.UI.ItemActivation.IActivateItems,T)"/>)
        /// </summary>
        protected void PublishToSelfOnly()
        {
            SetDatabaseObject(Activator, DatabaseObject);
        }
        public virtual void ConsultAboutClosing(object sender, FormClosingEventArgs e) {}

        public virtual ObjectSaverButton GetObjectSaverButton()
        {
            return ObjectSaverButton1;
        }
    }
}
