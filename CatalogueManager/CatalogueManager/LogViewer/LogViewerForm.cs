using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using HIC.Logging;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;


namespace CatalogueManager.LogViewer
{
    /// <summary>
    /// Displays all the activity going on within the RDMP that has been recorded in the logging database.  This includes data extractions, data loads, data quality runs etc.  This 
    /// information is stored in a relational database format including:
    /// 
    /// <para>Task - The overarching type of task e.g. 'Data Extraction', 'Loading Biochemistry' etc
    /// Run - Each time data has flown from one set of locations to another, this encapsulates one execution e.g. An attempt to load 3 Biochemistry files on 2016-02-05 at 5AM
    /// Table Loads - Each run will have 0 or more Table Loads, these are destinations for the data being handled and may include flat file locations such as during data export to csv
    /// Data Sources - Each table can have an explicit source which might be a flat file being loaded or an SQL query in the case of data extraction.
    /// Fatal Errors - Any crash that happened during a run should appear in this view
    /// Progress Messages - A log of every progress message generated during the run will appear here</para>
    /// 
    /// <para>The LogViewerNavigationPane on the right and the BreadcrumbNavigation controls allow you to rapidly zip around the logging database to see what has been going on / going wrong</para>
    /// </summary>
    public partial class LogViewerForm : Form
    {
        private readonly ExternalDatabaseServer _loggingServer;
        private LogViewerFilterCollection _filter;
        private LogManager _logManager;
        private DiscoveredServer _server;

        public LogViewerForm(ExternalDatabaseServer loggingServer)
        {
            _loggingServer = loggingServer;
            InitializeComponent();

            logViewerNavigationPane.Navigate += OnNavigate;
            breadcrumbNavigation.Navigate += OnNavigate;

            loggingTasks.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;
            loggingRunsTab1.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;
            loggingFatalErrors.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;
            loggingProgressMessagesTab1.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;
            loggingTableLoadsTab1.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;
            loggingDataSources.NavigationPaneGoto += doubleClicks_NavigationPaneGoto;

        }

        void doubleClicks_NavigationPaneGoto(object sender, NavigatePaneToEntityArgs args)
        {
            logViewerNavigationPane.SelectEntity(args);
        }

        void OnNavigate(object sender, LogViewerNavigationTarget target, int? alsoSelectRowID)
        {
            switch (target)
            {
                case LogViewerNavigationTarget.DataLoadTasks:
                    tabControl1.SelectTab(tpTasks);
                    
                    if (alsoSelectRowID != null)
                        loggingTasks.SelectRowWithID((int)alsoSelectRowID);
                    break;

                case LogViewerNavigationTarget.DataLoadRuns:
                    tabControl1.SelectTab(tpRuns);
                    
                    if (alsoSelectRowID != null)
                        loggingRunsTab1.SelectRowWithID((int)alsoSelectRowID);

                    break;
                case LogViewerNavigationTarget.ProgressMessages:
                    tabControl1.SelectTab(tpProgressMessages);

                     break;
                case LogViewerNavigationTarget.FatalErrors:
                    tabControl1.SelectTab(tpFatalErrors);
                    
                    break;
                case LogViewerNavigationTarget.TableLoadRuns:
                    tabControl1.SelectTab(tpTableLoads);
                    
                    if(alsoSelectRowID != null)
                        loggingTableLoadsTab1.SelectRowWithID((int)alsoSelectRowID);

                    break;
                case LogViewerNavigationTarget.DataSources:
                       tabControl1.SelectTab(tpDataSources);
                     
                    break;
                default:
                    throw new ArgumentOutOfRangeException("target");
            }
        }
        void RefreshTabs()
        {
            tbCurrentFilter.Text = _filter.ToString();
            loggingTasks.SetStateTo(_logManager,_filter);
            loggingRunsTab1.SetStateTo(_logManager, _filter);
            loggingTableLoadsTab1.SetStateTo(_logManager,_filter);
            loggingDataSources.SetStateTo(_logManager, _filter);
            loggingFatalErrors.SetStateTo(_logManager,_filter);
            loggingProgressMessagesTab1.SetStateTo(_logManager,_filter);

            breadcrumbNavigation.SetupFor(_filter, GetCurrentTab());
        }

        private LogViewerNavigationTarget GetCurrentTab()
        {
            if(tabControl1.SelectedTab == tpTasks)
                return LogViewerNavigationTarget.DataLoadTasks;

            if (tabControl1.SelectedTab == tpTableLoads)
                return LogViewerNavigationTarget.TableLoadRuns;
            if (tabControl1.SelectedTab == tpRuns)
                return LogViewerNavigationTarget.DataLoadRuns;
            if (tabControl1.SelectedTab == tpProgressMessages)
                return LogViewerNavigationTarget.ProgressMessages;
            if (tabControl1.SelectedTab == tpFatalErrors)
                return LogViewerNavigationTarget.FatalErrors;
            if (tabControl1.SelectedTab == tpDataSources)
                return LogViewerNavigationTarget.DataSources;

            throw new NotSupportedException("User had no tabs selected or a freaky one?");
        }

        private void LogViewerForm_Load(object sender, EventArgs e)
        {
            if (_loggingServer == null)
                return;

            _server = DataAccessPortal.GetInstance().ExpectServer(_loggingServer, DataAccessContext.Logging);
            _logManager = new LogManager(_server);
            _filter = new LogViewerFilterCollection();
            _filter.FilterChanged += RefreshTabs;
            
            var tasks = loggingTasks.FetchDataTasks(_logManager, _filter);
            logViewerNavigationPane.InitializeWithTaskList(tasks, _server,_filter);

            
            RefreshTabs();

        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            _filter.Clear();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            breadcrumbNavigation.SetupFor(_filter,GetCurrentTab());
        }
    }
}
