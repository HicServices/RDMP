using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using HIC.Logging;
using HIC.Logging.PastEvents;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueManager.LogViewer
{
    /// <summary>
    /// Provides an expandable Tree view of the data presented in the main LogViewerForm.  This can be useful for exploration where you know the task and want to explore it hierarchically
    /// rather than as a DataTable (in the main view).  Selecting an entity in the tree will synchronise both the LogViewerForm and the BreadcrumbNavigation to highlight the selected item.
    /// </summary>
    public partial class LogViewerNavigationPane : UserControl
    {
        private Dictionary<string, int> _dataTasks;
        private DiscoveredServer _loggingServer;
        private LogViewerFilterCollection _filter;

        public event LogViewerNavigationRequestHandler Navigate = delegate { };

        public LogViewerNavigationPane()
        {
            InitializeComponent();

            treeView1.ChildrenGetter+= ChildrenGetter;
            treeView1.CanExpandGetter+= CanExpandGetter;
            treeView1.HideSelection = false;
            olvColumn1.FillsFreeSpace = true;
            treeView1.FormatRow += treeView1_FormatRow;
        }

        private bool FilterObjects(object obj)
        {
            //the only models of type string are DataLoadTasks (see _dataTask dictionary)
            var dataLoadTask = obj as string;
            if (dataLoadTask != null)
                return MatchesFilter(dataLoadTask);

            return true;
        }

        void treeView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            var dli = e.Model as ArchivalDataLoadInfo;
            
            //if it is a data load info thing
            if (dli != null)
                if (dli.Errors.Any())
                    e.Item.ForeColor = dli.HasUnresolvedErrors ? Color.Red : Color.DarkOrange;
                else if (dli.EndTime == null) //did not end
                    e.Item.ForeColor = Color.Purple;
                else
                    e.Item.ForeColor = Color.Green; //was fine
        }

        private bool CanExpandGetter(object model)
        {
            if (model is string)
                return true;

            if (model is ArchivalDataLoadInfo)
                return true;

            if (model is LogViewerNavigateToCollection)
                return ((LogViewerNavigateToCollection) model).NavigationPaneChildren.Any();

            if (model is ArchivalTableLoadInfo)
                return true;

            return false;
        }

        private IEnumerable ChildrenGetter(object model)
        {
            var s = model as string;
            var dli = model as ArchivalDataLoadInfo;
            var navigate = model as LogViewerNavigateToCollection;
            var tli = model as ArchivalTableLoadInfo;

            if (s != null)
            {
                var children = ArchivalDataLoadInfo.GetLoadHistoryForTask(s, _loggingServer).OrderByDescending(c=>c).ToArray();
                return children;
            }

            if(dli != null)
            {
                List<LogViewerNavigateToCollection> collections = new List<LogViewerNavigateToCollection>();
                
                collections.Add(
                    new LogViewerNavigateToCollection(
                        "Progress Messages (" + dli.Progress.Count + ")",
                        LogViewerNavigationTarget.ProgressMessages,
                        dli.ID,
                        dli.Progress.Select(p => p.ID).ToArray()
                        ));


                collections.Add(
                    new LogViewerNavigateToCollection(
                        "Fatal Errors (" + dli.Errors.Count + ")",
                        LogViewerNavigationTarget.FatalErrors,
                        dli.ID, dli.Errors.Select(e => e.ID).ToArray()
                        ));

                collections.Add(
                    new LogViewerNavigateToCollection(
                        "Load Targets (" + dli.TableLoadInfos.Count + ")",
                    LogViewerNavigationTarget.TableLoadRuns,
                    dli.ID,
                    dli.TableLoadInfos.Select(t => t.ID).ToArray(),
                    dli.TableLoadInfos.ToArray()
                    ));

                return collections;
            }

            if(tli != null)
                return new object[] { new LogViewerNavigateToCollection("Data Sources (" + tli.DataSources.Count + ")",LogViewerNavigationTarget.DataSources,tli.ID,tli.DataSources.Select(ds=>ds.ID).ToArray())};
            
            if (navigate != null)
                return navigate.NavigationPaneChildren;

            return null;
        }

        public void InitializeWithTaskList(Dictionary<string,int> dataTasks, DiscoveredServer loggingServer,LogViewerFilterCollection filter)
        {
            _filter = filter;
            _dataTasks = dataTasks;
            _loggingServer = loggingServer;
            treeView1.AddObjects(dataTasks.Keys);
        }

        private void treeView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            object o = treeView1.SelectedObject;
            var task = o as string;
            var navigationLink = o as LogViewerNavigateToCollection;
            var dli = o as ArchivalDataLoadInfo;
            var tli = o as ArchivalTableLoadInfo;

            if (task != null)
            {
                _filter.Task = _dataTasks[task];
                Navigate(this, LogViewerNavigationTarget.DataLoadRuns, null);
            }

            if(navigationLink != null)
            {
                if (navigationLink.Target == LogViewerNavigationTarget.DataSources)
                {
                    _filter.Table = navigationLink.ParentID;
                    var t = (ArchivalTableLoadInfo) treeView1.GetParent(o);
                    _filter.Run = t.Parent.ID;
                    _filter.Task = t.Parent.DataLoadTaskID;
                }
                else
                {
                    _filter.Run = navigationLink.ParentID;
                    var d = (ArchivalDataLoadInfo) treeView1.GetParent(o);
                    _filter.Task = d.DataLoadTaskID;
                    _filter.Table = null;
                }

                Navigate(this, navigationLink.Target,null);
            }

            if (dli != null)
            {
                _filter.Task = dli.DataLoadTaskID;
                Navigate(this, LogViewerNavigationTarget.DataLoadRuns,dli.ID);
            }

            if (tli != null)
            {
                _filter.Run = tli.Parent.ID;
                Navigate(this, LogViewerNavigationTarget.TableLoadRuns, tli.ID);
            }
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            treeView1.UseFiltering = true;
            treeView1.ModelFilter = new ModelFilter(FilterObjects);
            
        }

        public void SelectEntity(NavigatePaneToEntityArgs args)
        {
            LogManager lm = new LogManager(_loggingServer);

            int task, run=-1, table = -1;

            //work out what IDs we are trying to load based on the type of the object passed e.g. DataSource requires us to lookup IDs of Task, Run AND Table so we can select it in the tree view
            switch (args.EntityTarget)
            {
                case LogViewerNavigationTarget.DataLoadTasks:
                    task = args.EntityID;
                    break;

                case LogViewerNavigationTarget.DataLoadRuns:
                    run = args.EntityID;
                    lm.GetRunIDs(args.EntityID, out task);
                    break;

                case LogViewerNavigationTarget.ProgressMessages:
                    lm.GetProgressMessageIDs(args.EntityID, out task,out run);
                    break;

                case LogViewerNavigationTarget.FatalErrors:
                    lm.GetErrorIDs(args.EntityID,out task, out run);
                    break;

                case LogViewerNavigationTarget.TableLoadRuns:
                    table = args.EntityID;
                    lm.GetTableIDs(args.EntityID, out task, out run);
                    break;

                case LogViewerNavigationTarget.DataSources:
                    lm.GetDataSourceIDs(args.EntityID, out task, out run, out table);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
           
                
            /*
             * 
             * -- Archival Task []<- root objects
             *      -- Archival Run[]
             *          -- Message Collection
             *          -- Table Collection
             *              -- Archival Table[]
             *                  -- Datasource Collection
             * 
             * */

            //always expand the task (so we can find children - treelistview only loads children dynamically as they are expanded)
            string taskNode = ExpandTask(task);

            //if they double clicked a task
            if (args.EntityTarget == LogViewerNavigationTarget.DataLoadTasks)
                if (taskNode != null)
                    treeView1.SelectObject(taskNode);//select it
                else
                {
                    //they double clicked a task but there is a filter hidding the node
                    return;
                }
            else
            if (taskNode != null)
            {
                //they doubleclicked something under a task (run/message/error/table/datasource

                //it is a subtarget of run or run itself though garuanteed!

                //Expand the run 
                var runNode = ExpandRun(taskNode, run);

                
                if (args.EntityTarget == LogViewerNavigationTarget.DataLoadRuns)//if it is a run
                    treeView1.SelectObject(runNode, true);//select the run
                else
                if (args.EntityTarget >= LogViewerNavigationTarget.TableLoadRuns)//if its table or source
                {
                    //we will have to expand table collection too
                    var tableCollectionNode = ExpandCollection(runNode, LogViewerNavigationTarget.TableLoadRuns, table);    
                        
                    //select table
                    if(args.EntityTarget == LogViewerNavigationTarget.TableLoadRuns)//if its a table they want to select
                        SelectObjectWithIDFromNavigationCollection(tableCollectionNode, table); //select the table
                    else
                    {
                        //it is a datasource so expand the table data sources too!
                        ArchivalTableLoadInfo tli = (ArchivalTableLoadInfo)ExpandObjectWithIDFromNavigationCollection(tableCollectionNode, table);

                        //and finally find the right datasource and select it
                        var toSelect = treeView1.GetChildren(tli).OfType<LogViewerNavigateToCollection>().Single(e => e.CollectionIDs.Contains(args.EntityID));
                        treeView1.SelectObject(toSelect,true);
                    }
                }
                else
                {
                    var toSelect = treeView1.GetChildren(runNode).OfType<LogViewerNavigateToCollection>().Single(c => c.Target == args.EntityTarget);
                    treeView1.SelectObject(toSelect,true);
                }
                

            }
            
                   
            
        }

        private object ExpandObjectWithIDFromNavigationCollection(LogViewerNavigateToCollection parentNode, int id)
        {
            var toExpand = treeView1.GetChildren(parentNode).OfType<IArchivalLoggingRecordOfPastEvent>().Single(e => e.ID == id);
            treeView1.Expand(toExpand);
            return toExpand;
        }

        private void SelectObjectWithIDFromNavigationCollection(LogViewerNavigateToCollection parentNode, int id)
        {
            var toSelect = treeView1.GetChildren(parentNode).OfType<IArchivalLoggingRecordOfPastEvent>().Single(e => e.ID == id);
            
            treeView1.SelectObject(toSelect,true);
        }

        private LogViewerNavigateToCollection ExpandCollection(object parentNode, LogViewerNavigationTarget collectionType, int idOfOneOfTheObjectsHeldByTheCollection)
        {
            var toSelect = treeView1.GetChildren(parentNode).OfType<LogViewerNavigateToCollection>().Single(collection => collection.Target == collectionType && collection.CollectionIDs.Contains(idOfOneOfTheObjectsHeldByTheCollection));

            if (!treeView1.IsExpanded(toSelect))
                treeView1.Expand(toSelect);

            return toSelect;
        }


        private ArchivalDataLoadInfo ExpandRun(object taskNode, int run)
        {
            var toSelect = treeView1.GetChildren(taskNode).OfType<ArchivalDataLoadInfo>().Single(dli => dli.ID == run);
            if (!treeView1.IsExpanded(toSelect))
                treeView1.Expand(toSelect);

            return toSelect;
        }

        private string ExpandTask(int task)
        {
            var toSelectIfAny = treeView1.Objects.OfType<string>().SingleOrDefault(o => _dataTasks[o] == task);

            if(!MatchesFilter(toSelectIfAny))
            {
                MessageBox.Show("The current filter hides data task with ID " + task + " try clearing the current filter in the NavigationPane");
                return null;
            }

            if (!treeView1.IsExpanded(toSelectIfAny))
                treeView1.Expand(toSelectIfAny);

            return toSelectIfAny;

        }

        private bool MatchesFilter(string task)
        {
            return task.ToLower().Contains(tbFilter.Text.ToLower());
        }
    }
}

