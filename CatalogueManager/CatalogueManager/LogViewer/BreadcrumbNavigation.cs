using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogueManager.LogViewer
{
    /// <summary>
    /// Appears on the top of LogViewerForm and tells you which log table you are looking at (e.g. the run, the tables that were loaded, the progress messages etc).  Also tells you what
    /// current filters are on the view in the LogViewerForm (e.g. 'you are looking at all tables loaded as part of data load run ID=5).  Breadcrumb elements are hyperlinks that will 
    /// change what you are looking at in LogViewerForm if you click on them (e.g. Clicking 'All Tasks' will take you back to seeing all the tasks).
    /// </summary>
    public partial class BreadcrumbNavigation : UserControl
    {
        private LinkLabel _llAllTasks;
        private LinkLabel _llSpecificTask;
        private LinkLabel _llAllRuns;
        private LinkLabel _llSpecificRun;
        private LinkLabel _llAllTableLoads;
        private LinkLabel _llSpecificTableLoad;

        private LogViewerFilterCollection _filter;
        private LinkLabel _llAllProgressMessages;
        private LinkLabel _llAllFatalErrors;
        private LinkLabel _llAllDataSources;

        public event LogViewerNavigationRequestHandler Navigate = delegate { };

        public BreadcrumbNavigation()
        {
            InitializeComponent();

            _llAllTasks = new LinkLabel();
            _llAllTasks.Text = "All Tasks";
            _llAllTasks.LinkClicked += (s, e) =>
            {
                _filter.Clear();
                Navigate(this, LogViewerNavigationTarget.DataLoadTasks, null);
            };

            _llSpecificTask = new LinkLabel();
            _llSpecificTask.Text = "Task (x)";
            _llSpecificTask.LinkClicked += (s, e) => Navigate(this, LogViewerNavigationTarget.DataLoadTasks, null);

            _llAllRuns = new LinkLabel();
            _llAllRuns.Text = "All Runs";
            _llAllRuns.LinkClicked += (s, e) =>
            {
                _filter.Run = null;
                _filter.Table = null;
                Navigate(this, LogViewerNavigationTarget.DataLoadRuns, null);
            };

            _llSpecificRun = new LinkLabel();
            _llSpecificRun.Text = "Run (x)";
            _llSpecificRun.LinkClicked += (s, e) =>
            {
                Navigate(this, LogViewerNavigationTarget.DataLoadRuns, null);
            };

            _llAllTableLoads = new LinkLabel();
            _llAllTableLoads.Text = "All TableLoads";
            _llAllTableLoads.LinkClicked+= (s,e)=>
            {
                _filter.Table = null;
                Navigate(this, LogViewerNavigationTarget.TableLoadRuns, null);
            };

            _llSpecificTableLoad = new LinkLabel();
            _llSpecificTableLoad.Text = "TableLoad (x)";
            _llSpecificTableLoad.LinkClicked += (s, e) => Navigate(this, LogViewerNavigationTarget.TableLoadRuns, null);


            _llAllProgressMessages = new LinkLabel();
            _llAllProgressMessages.Text = "All Progress Messages";
            
            _llAllFatalErrors = new LinkLabel();
            _llAllFatalErrors.Text = "All Fatal Errors";

            _llAllDataSources = new LinkLabel();
            _llAllDataSources.Text = "All Data Sources";

        }

        public void SetupFor(LogViewerFilterCollection filter,LogViewerNavigationTarget currentlySelectedTab)
        {
            _filter = filter;
            flp.Controls.Clear();
            flp.Controls.Add(_llAllTasks);

            if (filter.Task != null)
            {
                AddSeparator();
                _llSpecificTask.Text = "Task (ID=" + filter.Task + ")";
                _llSpecificTask.Tag = filter.Task;
                flp.Controls.Add(_llSpecificTask);
            }

            if (currentlySelectedTab >= LogViewerNavigationTarget.DataLoadRuns)
            {
                AddSeparator();
                flp.Controls.Add(_llAllRuns);

                if (filter.Run != null)
                {
                    AddSeparator();
                    _llSpecificRun.Text = "Run (ID=" + filter.Run + ")";
                    _llSpecificRun.Tag = filter.Run;
                    flp.Controls.Add(_llSpecificRun);
                }
            }

            if (currentlySelectedTab == LogViewerNavigationTarget.FatalErrors)
            {
                AddSeparator();
                flp.Controls.Add(_llAllFatalErrors);
            }

            if (currentlySelectedTab == LogViewerNavigationTarget.ProgressMessages)
            {
                AddSeparator();
                flp.Controls.Add(_llAllProgressMessages);
            }

            if (currentlySelectedTab >= LogViewerNavigationTarget.TableLoadRuns)
            {
                AddSeparator();
                flp.Controls.Add(_llAllTableLoads);

                if (filter.Table != null)
                {
                    AddSeparator();
                    _llSpecificTableLoad.Text = "TableLoad (ID=" + filter.Table + ")";
                    _llSpecificTableLoad.Tag = filter.Table;
                    flp.Controls.Add(_llSpecificTableLoad);
                }
            }
            
            if (currentlySelectedTab == LogViewerNavigationTarget.DataSources)
            {
                AddSeparator();
                flp.Controls.Add(_llAllDataSources);
            }

            foreach (Control c in flp.Controls)
                c.AutoSize = true;

            //decide where to put the arrow
            switch (currentlySelectedTab)
            {
                case LogViewerNavigationTarget.DataLoadTasks:
                    CenterArrowOn(filter.Task == null ? _llAllTasks : _llSpecificTask);
                    break;
                case LogViewerNavigationTarget.DataLoadRuns:
                    CenterArrowOn(filter.Run == null ? _llAllRuns : _llSpecificRun);
                    break;
                case LogViewerNavigationTarget.ProgressMessages:
                    CenterArrowOn(_llAllProgressMessages);
                    break;
                case LogViewerNavigationTarget.FatalErrors:
                    CenterArrowOn(_llAllFatalErrors);
                    break;
                case LogViewerNavigationTarget.TableLoadRuns:
                    CenterArrowOn(filter.Table  == null? _llAllTableLoads:_llSpecificTableLoad);
                    break;
                case LogViewerNavigationTarget.DataSources:
                    CenterArrowOn(_llAllDataSources);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("currentlySelectedTab");
            }
        }

        private void CenterArrowOn(LinkLabel label)
        {
            pbArrow.Left = label.Left + (label.Width/2) + 3;
        }

        private void AddSeparator()
        {
            Label l = new Label();
            l.Text = ">";
            flp.Controls.Add(l);
        }
    }
}
