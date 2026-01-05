// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.Collections.Providers.Copying;

/// <summary>
/// Provides UI code for drag and drop in <see cref="TreeListView"/>.  The code for what is/isn't draggable onto what is determined
/// by the <see cref="ICommandExecutionFactory"/>.
/// </summary>
public class DragDropProvider : SimpleDragSource
{
    private readonly ICombineableFactory _commandFactory;
    private readonly ICommandExecutionFactory _commandExecutionFactory;
    private readonly TreeListView _treeView;

    public DragDropProvider(ICombineableFactory commandFactory, ICommandExecutionFactory commandExecutionFactory,
        TreeListView treeView)
    {
        _commandFactory = commandFactory;
        _commandExecutionFactory = commandExecutionFactory;
        _treeView = treeView;

        _treeView.IsSimpleDragSource = false;
        _treeView.DragSource = this;

        _treeView.IsSimpleDropSink = true;

        ((SimpleDropSink)_treeView.DropSink).CanDrop += DragDropProvider_CanDrop;
        ((SimpleDropSink)_treeView.DropSink).Dropped += DragDropProvider_Dropped;

        _treeView.ModelCanDrop += ModelCanDrop;
        _treeView.ModelDropped += ModelDropped;
    }

    private void DragDropProvider_CanDrop(object sender, OlvDropEventArgs e)
    {
        var dropTargetModel = e.DropTargetItem?.RowObject;

        if (e.DataObject is not DataObject dataObject)
            return;

        if (dataObject is OLVDataObject)
            return; //should be handled by ModelCanDrop

        var execution = GetExecutionCommandIfAnyForNonModelObjects(dataObject, dropTargetModel);

        if (execution != null)
            DisplayFeedback(execution, e);
    }

    private void ModelCanDrop(object sender, ModelDropEventArgs e)
    {
        ((SimpleDropSink)_treeView.DropSink).CanDropBetween = e.TargetModel is IOrderable;

        e.Handled = true;
        e.Effect = DragDropEffects.None;

        if (e.TargetModel == null)
            return;

        var cmd = _commandFactory.Create(e);

        if (cmd == null)
            return;

        var execution = _commandExecutionFactory.Create(cmd, e.TargetModel, GetDropLocation(e));

        DisplayFeedback(execution, e);
    }

    private void DragDropProvider_Dropped(object sender, OlvDropEventArgs e)
    {
        try
        {
            var dataObject = (DataObject)e.DataObject;

            if (dataObject is OLVDataObject)
                return; //should be handled by ModelDropped

            //is it a non model drop (in which case ModelDropped won't be called) e.g. it could be a file drop
            var execution = GetExecutionCommandIfAnyForNonModelObjects(dataObject, e.DropTargetItem.RowObject);

            if (execution is { IsImpossible: false })
                execution.Execute();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void ModelDropped(object sender, ModelDropEventArgs e)
    {
        var cmd = _commandFactory.Create(e);

        //no valid source command
        if (cmd == null)
            return;

        //otherwise get command compatible with the target
        var execution = _commandExecutionFactory.Create(cmd, e.TargetModel, GetDropLocation(e));

        //it is a valid combination
        if (execution != null)
            try
            {
                execution.Execute();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show($"ExecuteCommand {execution.GetType().Name} failed, See Exception for details",
                    exception);
            }
    }

    /// <summary>
    /// Only applies to external files and other weird types that are not Models in our tree views but that we might still want to allow drag and drop interactions for
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="dropTargetModel"></param>
    /// <returns></returns>
    private ICommandExecution GetExecutionCommandIfAnyForNonModelObjects(DataObject dataObject, object dropTargetModel)
    {
        //if user is dragging in files
        if (dataObject.ContainsFileDropList())
        {
            //get file list
            var files = dataObject.GetFileDropList().Cast<string>().Select(s => new FileInfo(s)).ToArray();
            var fileCommand = _commandFactory.Create(files);

            //if command factory supports generating file based commands
            if (fileCommand != null)
            {
                //does the execution factory permit the combination of a file command with the drop target
                var execution = _commandExecutionFactory.Create(fileCommand, dropTargetModel);
                return execution;
            }
        }

        return null;
    }

    public override object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item)
    {
        var rowObject = item.RowObject; //get the dragged object

        if (olv.SelectedObjects.Count > 1)
            rowObject = olv.SelectedObjects.Cast<object>().ToArray();

        if (rowObject != null)
        {
            //get the drag operation data object olv does
            var toReturn = (OLVDataObject)base.StartDrag(olv, button, item);

            //can we process it into a command?
            var command = _commandFactory.Create(toReturn);

            if (command == null)
                return null; //it couldn't become a command so leave it as a model object

            //yes, let's hot swap the data object in the drag command, that couldn't possibly go wrong right?
            toReturn.ModelObjects.Clear();
            toReturn.ModelObjects.Add(command);
            return toReturn;
        }

        return base.StartDrag(olv, button, item);
    }


    private static void DisplayFeedback(ICommandExecution execution, OlvDropEventArgs e)
    {
        //no command is even remotely possible
        if (execution == null)
        {
            e.DropSink.FeedbackColor = Color.IndianRed;
            return;
        }

        //valid combination of objects but not possible due to object states
        if (execution.IsImpossible)
        {
            e.DropSink.FeedbackColor = Color.IndianRed;
            e.DropSink.Billboard.BackColor = Color.IndianRed;
            e.InfoMessage = execution.ReasonCommandImpossible;
            return;
        }

        e.DropSink.Billboard.BackColor = Color.GreenYellow;
        e.DropSink.FeedbackColor = Color.GreenYellow;
        e.InfoMessage = execution.GetCommandName();
        e.Handled = true;
        e.Effect = DragDropEffects.Move;
    }



    private static InsertOption GetDropLocation(ModelDropEventArgs e)
    {
        if (e.DropTargetLocation == DropTargetLocation.AboveItem)
            return InsertOption.InsertAbove;

        return e.DropTargetLocation == DropTargetLocation.BelowItem ? InsertOption.InsertBelow : InsertOption.Default;
    }
}