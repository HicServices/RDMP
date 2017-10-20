using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Collections.Providers.Copying
{
    public class DragDropProvider:SimpleDragSource
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandExecutionFactory _commandExecutionFactory;
        private readonly TreeListView _treeView;

        public DragDropProvider(ICommandFactory commandFactory, ICommandExecutionFactory commandExecutionFactory, TreeListView treeView)
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

        void DragDropProvider_CanDrop(object sender, OlvDropEventArgs e)
        {
            var dropTargetModel = e.DropTargetItem != null ? e.DropTargetItem.RowObject :null;
            var dataObject = e.DataObject as DataObject;

            if(dataObject == null)
                return;

            if(dataObject is OLVDataObject)
                return; //should be handled by ModelCanDrop

            var execution = GetExecutionCommandIfAnyForNonModelObjects(dataObject,dropTargetModel);

            if(execution != null)
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
               var dataObject = (DataObject) e.DataObject;

                if(dataObject is OLVDataObject)
                    return;  //should be handled by ModelDropped

               //is it a non model drop (in which case ModelDropped won't be called) e.g. it could be a file drop 
                var execution = GetExecutionCommandIfAnyForNonModelObjects(dataObject, e.DropTargetItem.RowObject);

               if(execution != null && !execution.IsImpossible)
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
                    ExceptionViewer.Show("ExecuteCommand " + execution.GetType().Name + " failed, See Exception for details", exception);
                }
        }

       /// <summary>
       /// Only applies to external files and other wierd types that are not Models in our tree views but that we might still want to allow drag and drop interactions for
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
               ICommand fileCommand = _commandFactory.Create(files);

               //if command factory supports generating file based commands
               if (fileCommand != null)
               {
                   //does the execution factory permit the combination of a file command with the drop target
                   ICommandExecution execution = _commandExecutionFactory.Create(fileCommand, dropTargetModel);
                   return execution;
               }
           }

           return null;
       }

        public override object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item)
        {
            var rowObject = item.RowObject;//get the dragged object
            
            if(olv.SelectedObjects.Count > 1)
                rowObject = olv.SelectedObjects.Cast<object>().ToArray();

            if(rowObject != null)
            {
                //get the drag operation data object olv does
                var toReturn = (OLVDataObject)base.StartDrag(olv, button, item);
                
                //can we process it into a command?
                ICommand command = _commandFactory.Create(toReturn);

                if (command == null)
                    return null;//it couldn't become a command so leave it as a model object

                //yes, lets hot swap the data object in the drag command, that couldn't possibly go wrong right?
                toReturn.ModelObjects.Clear();
                toReturn.ModelObjects.Add(command);
                return toReturn;
            }

            return base.StartDrag(olv, button, item);
        }

   

        private void DisplayFeedback(ICommandExecution execution, OlvDropEventArgs e)
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
        

        private InsertOption GetDropLocation(ModelDropEventArgs e)
        {
            if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                return InsertOption.InsertAbove;

            if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                return InsertOption.InsertBelow;

            return InsertOption.Default;
        }

    }
}
