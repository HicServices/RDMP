// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Enum = System.Enum;

namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;

/// <summary>
/// Filters, Aggregates, Extractions etc can all make use of SQL parameters (e.g. @drugName).  This dialog appears any time you are viewing/editing the parameters associated with a
/// given parameter use case.  If you do not understand what SQL parameters (aka variables) are then you should read up on this before using this control.
/// 
/// <para>The following help instructions will relate to the context of editing a Filter and configuring some parameters but is equally applicable to configuring global parameters on an
/// extraction or in a cohort identification configuration etc.</para>
/// 
/// <para>The first time you use a parameter in your filter (e.g. @drugName), a template SQL Parameter will be created (probably as a varchar(50)).  You should adjust the Declare SQL such
/// that it is the correct data type for your filter and give it a default value which illustrates how it should be used when deployed as part of cohort identification or data extraction.
/// Finally provide a description of what the parameter is supposed to do in the Comment section.  </para>
/// 
/// <para>Parameters exist at multiple levels within the system and it is possible that you are editing parameters that are not at the top level.  For example, you are configuring an extract
/// in which the global parameter @drugCodesOfInterest is declared for all datasets in the project and you have just opened a Filter.  In this context you will see some greyed out
/// 'Overriding' parameters, these are available for use at lower levels but cannot be changed (because the new Value would be applied to all users of the global i.e. all datasets in the
/// extraction, not just the one you are editing).</para>
/// 
/// <para>So to return to the above example, if you create a filter 'Prescriptions collected after date X' with the SQL 'dateCollected > @dateOfCollection'.  When you save the Filter the
/// parameter @dateOfCollection will be created (unless there is already a global with the same name/type).</para>
/// 
/// <para>Sometimes the Globals are explicit fixed value parameters for example the @ProjectNumber in a data extraction, in this case the Parameter cannot be modified period.</para>
/// </summary>
public partial class ParameterCollectionUI : RDMPUserControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ParameterCollectionUIOptions Options { get; private set; }

    private ToolStripMenuItem miAddNewParameter = new("New Parameter...");
    private ToolStripMenuItem miOverrideParameter = new("Override Parameter");

    public ParameterCollectionUI()
    {
        InitializeComponent();
        olvParameterName.GroupKeyGetter += GroupKeyGetter;
        olvParameters.AboutToCreateGroups += olvParameters_AboutToCreateGroups;
        olvParameters.AddDecoration(new EditingCellBorderDecoration { UseLightbox = true });

        olvParameterName.ImageGetter += ImageGetter;
        olvParameterName.AspectGetter += ParameterName_AspectGetter;

        olvOwner.AspectGetter += OwnerAspectGetter;
        olvParameters.CellToolTipShowing += olvParameters_CellToolTipShowing;

        parameterEditorScintillaControl1.ParameterSelected += (s, e) => olvParameters.SelectObject(e);
        parameterEditorScintillaControl1.ParameterChanged += (s, e) => olvParameters.RefreshObject(e);
        parameterEditorScintillaControl1.ProblemObjectsFound += RefreshProblemObjects;

        olvParameters.ContextMenuStrip = new ContextMenuStrip();
        olvParameters.ContextMenuStrip.Items.Add(miAddNewParameter);
        olvParameters.ContextMenuStrip.Items.Add(miOverrideParameter);

        miAddNewParameter.Click += miAddParameter_Click;
        miOverrideParameter.Click += miOverride_Click;

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvParameters, olvComment,
            new Guid("8058e9c5-236e-443a-8586-6a21bf1a4562"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvParameters, olvOwner,
            new Guid("e2097029-e35f-4d56-a000-e8cca717b9ad"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvParameters, olvValue,
            new Guid("cfbab41b-8af1-480c-a7a0-0bbf42ee8cb9"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvParameters, olvParameterName,
            new Guid("eadfd8e8-db88-4632-badc-0c6dbec2f877"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvParameters, olvParameterSQL,
            new Guid("a9216170-621d-45c6-9d32-9046a054fc0f"));
    }

    private object ParameterName_AspectGetter(object rowObject)
    {
        if (rowObject is not ISqlParameter p)
            return null;

        try
        {
            return p.ParameterName;
        }
        catch (Exception)
        {
            return "Unknown";
        }
    }


    private void olvParameters_AboutToCreateGroups(object sender, CreateGroupsEventArgs e)
    {
        var order = new Dictionary<string, int>
        {
            { ParameterLevel.Global.ToString(), 0 },
            { ParameterLevel.CompositeQueryLevel.ToString(), 1 },
            { ParameterLevel.QueryLevel.ToString(), 2 },
            { ParameterLevel.TableInfo.ToString(), 3 }
        };

        foreach (var g in e.Groups)
            g.SortValue = order[g.Header];

        var currentGroup = e.Groups.SingleOrDefault(g => g.Header.Equals(Options.CurrentLevel.ToString()));
        if (currentGroup != null)
            currentGroup.Header = $"{currentGroup.Header} (current)";

        e.Groups = e.Groups.OrderBy(g => g.SortValue).ToList();
    }

    public void Clear()
    {
        olvParameters.ClearObjects();
    }

    public void RefreshParametersFromDatabase()
    {
        miAddNewParameter.Enabled = Options.CanNewParameters();

        olvParameters.ClearObjects();

        //add all parameters from all levels
        foreach (ParameterLevel level in Enum.GetValues(typeof(ParameterLevel)))
        {
            var parametersFoundAtThisLevel = Options.ParameterManager.ParametersFoundSoFarInQueryGeneration[level];

            //add them to the collection
            if (parametersFoundAtThisLevel.Any())
                olvParameters.AddObjects(parametersFoundAtThisLevel);
        }

        DisableRelevantObjects();
        parameterEditorScintillaControl1.RegenerateSQL();

        UpdateTabVisibility();
    }

    private void UpdateTabVisibility()
    {
        if (parameterEditorScintillaControl1.IsBroken)
            tabControl1.TabPages.Remove(tpSql);
        else if (!tabControl1.TabPages.Contains(tpSql))
            tabControl1.TabPages.Add(tpSql);
    }

    private void DisableRelevantObjects()
    {
        if (olvParameters.Objects == null) //there are no parameters
            return;

        var parameters = olvParameters.Objects.Cast<ISqlParameter>().ToArray();
        var toDisable = parameters.Where(Options.ShouldBeDisabled);

        //These parameters are super special and in fact only have a Value (other properties are inherited from a parent ExtractionFilterParameter) so only let the user edit the Value textbox if there are any of these in the UI
        if (parameters.Any(p => p is ExtractionFilterParameterSetValue))
        {
            olvComment.IsEditable = false;
            olvParameterSQL.IsEditable = false;
        }
        else
        {
            olvComment.IsEditable = true;
            olvParameterSQL.IsEditable = true;
        }

        if (Options.ReadOnly)
        {
            olvParameters.DisableObjects(parameters);
            return;
        }

        //enable all
        olvParameters.EnableObjects(parameters);
        //disable all rows which are overridden or are from a higher level than the one the user is currently editing
        olvParameters.DisableObjects(toDisable);
        //refresh everyones icons and values
        olvParameters.RefreshObjects(parameters);
    }


    public static Form ShowAsDialog(IActivateItems activator, ParameterCollectionUIOptions options, bool modal = false)
    {
        var f = new Form
        {
            Text = $"Parameters For:{options.Collector}"
        };
        var ui = new ParameterCollectionUI();
        f.Width = ui.Width;
        f.Height = ui.Height;

        ui.SetUp(options, activator);
        ui.Dock = DockStyle.Fill;
        f.Controls.Add(ui);

        if (modal)
            f.ShowDialog();
        else
            f.Show();

        return f;
    }

    public void SetUp(ParameterCollectionUIOptions options, IActivateItems activator)
    {
        Options = options;

        SetItemActivator(activator);

        hiParameters.SetHelpText("Use Case", options.UseCase);

        parameterEditorScintillaControl1.Options = options;
        RefreshParametersFromDatabase();
    }

    private void miAddParameter_Click(object sender, EventArgs e)
    {
        var r = new Random();
        using var dialog = new TypeTextOrCancelDialog("Parameter Name", "Name", 100, $"@MyParam{r.Next()}");
        if (dialog.ShowDialog() != DialogResult.OK) return;

        var newParameter = Options.CreateNewParameter(dialog.ResultText.Trim());

        Options.ParameterManager.ParametersFoundSoFarInQueryGeneration[Options.CurrentLevel].Add(newParameter);

        RefreshParametersFromDatabase();
    }

    private void olvParameters_KeyDown(object sender, KeyEventArgs e)
    {
        // If the key isn't Delete, don't go building a list of things we'd delete then throwing it away!
        if (e.KeyCode != Keys.Delete) return;

        var deletables = olvParameters.SelectedObjects.OfType<IDeleteable>().ToArray();

        if (!deletables.Any() || e.KeyCode != Keys.Delete || !Activator.YesNo(
                deletables.Length == 1
                    ? $"Confirm deleting {deletables[0]}"
                    : $"Confirm deleting {deletables.Length} Parameters?", "Confirm delete")) return;
        foreach (IDeleteable d in olvParameters.SelectedObjects)
        {
            d.DeleteInDatabase();
            olvParameters.RemoveObject(d);
            Options.ParameterManager.RemoveParameter((ISqlParameter)d);

            DisableRelevantObjects();
            parameterEditorScintillaControl1.RegenerateSQL();
            UpdateTabVisibility();
        }

        // Rebuild once after deleting all objects, instead of after each deletion
        DisableRelevantObjects();
        parameterEditorScintillaControl1.RegenerateSQL();
        UpdateTabVisibility();
    }

    private void olvParameters_CellEditFinishing(object sender, CellEditEventArgs e)
    {
        var parameter = e.RowObject as ISqlParameter;
        string oldParameterName = null;
        string newParameterName;

        if (parameter != null)
            try
            {
                oldParameterName = parameter.ParameterName;
            }
            catch (Exception)
            {
                oldParameterName = null;
            }

        if (e.Column.Index == olvParameterSQL.Index)
            if (!string.IsNullOrWhiteSpace((string)e.NewValue))
                e.NewValue =
                    e.NewValue.ToString()
                        .TrimEnd(';'); //auto trim semicolons we automatically put those in ourselves we don't need user slapping them in too thanks

        e.Column.PutAspectByName(e.RowObject, e.NewValue);

        try
        {
            newParameterName = parameter?.ParameterName;
        }
        catch (Exception)
        {
            return;
        }

        if (e.RowObject is not IRevertable revertable)
            throw new NotSupportedException("Why is user editing something that isn't IRevertable?");

        var changes = revertable.HasLocalChanges();

        if (changes.Evaluation == ChangeDescription.DatabaseCopyDifferent)
            revertable.SaveToDatabase();

        //if the name has changed handle renaming
        if (oldParameterName != null && Options.Refactorer.HandleRename(parameter, oldParameterName, newParameterName))
        {
            var owner = parameter.GetOwnerIfAny();

            if ((owner ?? (object)parameter) is DatabaseEntity toRefresh)
                Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(toRefresh));
        }


        //anything that was a problem before
        var problemsBefore = parameterEditorScintillaControl1.ProblemObjects.Keys;
        DisableRelevantObjects();
        parameterEditorScintillaControl1.RegenerateSQL();
        UpdateTabVisibility();

        //might not be a problem any more so refresh the icons on them (and everything else)
        olvParameters.RefreshObjects(problemsBefore.ToList());
    }

    private object GroupKeyGetter(object rowObject)
    {
        var sqlParameter = (ISqlParameter)rowObject;
        return Options.ParameterManager.GetLevelForParameter(sqlParameter);
    }


    private void RefreshProblemObjects()
    {
        olvParameters.RefreshObjects(parameterEditorScintillaControl1.ProblemObjects.Select(kvp => kvp.Key).ToList());
    }

    private void olvParameters_CellToolTipShowing(object sender, ToolTipShowingEventArgs e)
    {
        var sqlParameter = (ISqlParameter)e.Model;

        e.IsBalloon = true;
        e.AutoPopDelay = 32767;
        e.StandardIcon = ToolTipControl.StandardIcons.Info;

        //if it is a problem parameter
        if (parameterEditorScintillaControl1.ProblemObjects.TryGetValue(sqlParameter, out var exception))
        {
            e.StandardIcon = ToolTipControl.StandardIcons.Warning;
            e.Title = "Problem Detected With Parameter";
            e.Text = exception.Message;
            return;
        }

        //if it is locked
        if (Options.IsOverridden(sqlParameter))
        {
            e.Title = "Parameter is overridden by a higher level";
            e.Text =
                "You have defined a higher level parameter with the same name/datatype which will override this parameters value during query generation";
            return;
        }

        //if it is locked
        if (Options.IsHigherLevel(sqlParameter))
        {
            e.Title = "Parameter is declared at a higher level";
            e.Text =
                $"This parameter is declared at {Options.ParameterManager.GetLevelForParameter(sqlParameter)} level which is higher than the level you are editing ({Options.CurrentLevel}).  You cannot change higher level parameters from here, look at the 'Owner' column to see which object the global belongs to";
        }
    }

    private string ImageGetter(object rowObject)
    {
        try
        {
            var sqlParameter = (ISqlParameter)rowObject;

            if (parameterEditorScintillaControl1.ProblemObjects.ContainsKey(sqlParameter))
                return "Warning.png";

            if (Options.IsOverridden(sqlParameter))
                return "Overridden.png";

            if (Options.IsHigherLevel(sqlParameter))
                return "Locked.png";
        }
        catch (Exception)
        {
            return "Warning.png"; //if it crashed trying to get an image then ... bad times return warning
        }

        return null;
    }

    private static object OwnerAspectGetter(object rowobject)
    {
        var parameter = (ISqlParameter)rowobject;
        var owner = parameter.GetOwnerIfAny();

        return owner == null ? null : $"{owner.GetType().Name}:{owner}";
    }

    private void olvParameters_SelectedIndexChanged(object sender, EventArgs e)
    {
        var parameter = olvParameters.SelectedObject as ISqlParameter;
        miOverrideParameter.Enabled = CanOverride(parameter);
    }

    private bool CanOverride(ISqlParameter sqlParameter)
    {
        if (sqlParameter != null)
            //if it is not already overridden
            if (!Options.IsOverridden(sqlParameter) &&
                //and it exists at a lower level
                Options.ParameterManager.GetLevelForParameter(sqlParameter) < Options.CurrentLevel)
                return true;

        return false;
    }

    private void miOverride_Click(object sender, EventArgs e)
    {
        if (olvParameters.SelectedObject is not ISqlParameter param || !CanOverride(param))
            return;

        var newParameter = Options.CreateNewParameter(param.ParameterName);

        newParameter.ParameterSQL = param.ParameterSQL;
        newParameter.Value = param.Value;
        newParameter.Comment = param.Comment;
        newParameter.SaveToDatabase();

        Options.ParameterManager.ParametersFoundSoFarInQueryGeneration[Options.CurrentLevel].Add(newParameter);
        RefreshParametersFromDatabase();
    }

    private void olvParameters_CellEditStarting(object sender, CellEditEventArgs e)
    {
        //cancel cell editing if it is read only
        if (e.RowObject is ISqlParameter p && Options.ShouldBeReadOnly(p))
            e.Cancel = true;
    }
}