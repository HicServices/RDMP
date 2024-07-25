// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.Theme;

namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

/// <summary>
/// TECHNICAL: base abstract class for all Controls which are concerned with a single root DatabaseEntity e.g. AggregateGraph is concerned only with an AggregateConfiguration
/// and its children.  The reason this class exists is to streamline lifetime publish subscriptions (ensuring multiple tabs editting one anothers database objects happens
/// in a seamless a way as possible).
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
[TechnicalUI]
public abstract class RDMPSingleDatabaseObjectControl<T> : RDMPUserControl, IRDMPSingleDatabaseObjectControl
    where T : DatabaseEntity
{
    /// <summary>
    /// True to track changes made to the <see cref="DatabaseObject"/> hosted by this control
    /// and create <see cref="Commit"/> when changes are saved.  Using this field requires
    /// declaring yourself <see cref="ISaveableUI"/>
    /// </summary>
    public bool UseCommitSystem { get; set; } = false;

    /// <summary>
    /// Tracks changes to <see cref="DatabaseObject"/> since last save.  Note that this is null
    /// before <see cref="SetDatabaseObject(IActivateItems, DatabaseEntity)"/> has been called
    /// or if <see cref="UseCommitSystem"/> is false.
    /// </summary>
    protected CommitInProgress CurrentCommit;

    private Control _colorIndicator;
    private Label _readonlyIndicator;

    private BinderWithErrorProviderFactory _binder;

    protected ObjectSaverButton ObjectSaverButton1 = new();

    private ToolStripMenuItem _refresh;

    private IActivateItems _activator;


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
        _activator = activator;
        Activator.RefreshBus.EstablishSelfDestructProtocol(this, activator, databaseObject);
        DatabaseObject = databaseObject;

        CommonFunctionality.ClearToolStrip();

        if (_colorIndicator == null && AssociatedCollection != RDMPCollection.None)
        {
            _colorIndicator = new Control
            {
                Dock = DockStyle.Top,
                Location = new Point(0, 0),
                Size = new Size(150, BackColorProvider.IndicatorBarSuggestedHeight),
                TabIndex = 0,
                BackColor = BackColorProvider.GetColor(AssociatedCollection)
            };
            Controls.Add(_colorIndicator);
        }

        _readonlyIndicator ??= new Label
        {
            Dock = DockStyle.Top,
            Location = new Point(0, 0),
            Size = new Size(150, 20),
            TabIndex = 0,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = SystemColors.HotTrack,
            Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, (byte)0),
            ForeColor = Color.Moccasin
        };

        if (databaseObject is IMightBeReadOnly ro)
        {
            if (ro.ShouldBeReadOnly(out var reason))
            {
                _readonlyIndicator.Text = reason;
                Controls.Add(_readonlyIndicator);
                ReadOnly = true;
            }
            else
            {
                //removing it allows us to handle refreshes (where something becomes unfrozen for example)
                Controls.Remove(_readonlyIndicator);
                ReadOnly = false;
            }
        }

        _binder ??= new BinderWithErrorProviderFactory(activator);

        SetBindings(_binder, databaseObject);

        if (this is ISaveableUI)
        {
            if (UseCommitSystem && CurrentCommit == null && Activator.UseCommits())
            {
                CurrentCommit = new CommitInProgress(activator.RepositoryLocator,
                    new CommitInProgressSettings(databaseObject));
                ObjectSaverButton1.BeforeSave += BeforeSave_FinishCommitInProgressIfAny;
                ObjectSaverButton1.AfterSave += AfterSave_BeginNewCommitIfApplicable;
            }
            if (this.GetType() == typeof(ExtractionFilterUI) && UserSettings.PromptRenameOnCohortFilterChange)
            {
                ObjectSaverButton1.BeforeSave -= BeforeSave_PromptRenameOfExtractionFilterContainer;
                ObjectSaverButton1.BeforeSave += BeforeSave_PromptRenameOfExtractionFilterContainer;
            }

            ObjectSaverButton1.SetupFor(this, databaseObject, activator);
        }


        var gotoFactory = new GoToCommandFactory(activator);
        foreach (var cmd in gotoFactory.GetCommands(databaseObject).OfType<ExecuteCommandShow>())
        {
            cmd.SuggestedCategory = AtomicCommandFactory.GoTo;
            CommonFunctionality.AddToMenu(cmd, null, null, AtomicCommandFactory.GoTo);
        }


        //add refresh
        _refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right,
            ToolTipText= "Refresh Object"
        };
        _refresh.Click += Refresh;
        CommonFunctionality.Add(_refresh);
    }

    private void Refresh(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandRefreshObject(_activator, DatabaseObject);
        cmd.Execute();
    }

    protected bool BeforeSave_PromptRenameOfExtractionFilterContainer(DatabaseEntity _)
    {
        if (!ObjectSaverButton1.IsEnabled) return true;
        AggregateFilter af;
        try
        {
            af = (AggregateFilter)_;
        }
        catch (Exception)
        {
            //DatabaseEntity was not an aggregateFilter
            return true;
        }
        AggregateFilterContainer afc = af.CatalogueRepository.GetAllObjectsWhere<AggregateFilterContainer>("ID", af.FilterContainer_ID).FirstOrDefault();
        if (afc != null)
        {
            AggregateConfiguration ac = afc.GetAggregate();
            if (ac != null)
            {
                var rename = new ExecuteCommandRename(Activator, ac);
                rename.Execute();
            }
        }
        return true;
    }


    protected bool BeforeSave_FinishCommitInProgressIfAny(DatabaseEntity _)
    {
        // control doesn't require commits (most controls don't)
        if (!UseCommitSystem)
            return true; // go through with the save

        // user has opted out via user settings or backing repository doesn't support commits
        if (!Activator.UseCommits())
            return true;

        if (CurrentCommit != null)
            if (CurrentCommit.TryFinish(Activator) == null)
                // No changes were actually made or user cancelled
                return false;

        // before starting a new commit cleanup old one
        CurrentCommit?.Dispose();

        // setting to null means a new one will be created in AfterSave
        CurrentCommit = null;

        return true;
    }

    private void AfterSave_BeginNewCommitIfApplicable()
    {
        if (CurrentCommit == null && UseCommitSystem && Activator.UseCommits())
            // start a new commit for the next changes the user commits
            CurrentCommit =
                new CommitInProgress(Activator.RepositoryLocator, new CommitInProgressSettings(DatabaseObject));
    }

    private void CommonFunctionality_ToolStripAddedToHost(object sender, EventArgs e)
    {
        _colorIndicator?.SendToBack();
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
    protected void Bind(Control c, string propertyName, string dataMember, Func<T, object> getter,
        bool formattingEnabled = true, DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
    {
        //workaround for only comitting lists on loose focus
        if (c is ComboBox { DropDownStyle: ComboBoxStyle.DropDownList } box && propertyName.Equals("SelectedItem"))
            box.SelectionChangeCommitted += (s, e) => box.DataBindings["SelectedItem"].WriteValue();

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

            var dateTime = DateTime.Parse(tb.Text);
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

            var u = new Uri(tb.Text);
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
        SetDatabaseObject(activator, (T)databaseObject);
    }

    public Type GetTypeOfT() => typeof(T);

    public virtual string GetTabName() =>
        DatabaseObject is INamed named ? named.Name : DatabaseObject?.ToString() ?? "Unnamed Tab";

    public virtual string GetTabToolTip() => null;

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

    public virtual void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
    }

    public virtual ObjectSaverButton GetObjectSaverButton() => ObjectSaverButton1;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        CurrentCommit?.Dispose();

        if (ObjectSaverButton1 != null)
        {
            ObjectSaverButton1.BeforeSave -= BeforeSave_FinishCommitInProgressIfAny;
            ObjectSaverButton1.BeforeSave -= BeforeSave_PromptRenameOfExtractionFilterContainer;
            ObjectSaverButton1.AfterSave -= AfterSave_BeginNewCommitIfApplicable;
        }
    }
}