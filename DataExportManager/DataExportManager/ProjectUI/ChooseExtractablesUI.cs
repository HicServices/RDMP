// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExtractionTime.Commands;
using CatalogueLibrary.ExtractionTime.UserPicks;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.UserPicks;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace DataExportManager.ProjectUI
{
    public delegate void ExtractCommandSelectedHandler(object sender, IExtractCommand command);
    
    /// <summary>
    /// Part of ExecuteExtractionUI.  This control allows you to select which datasets, extractable attachments (See SupportingDocumentUI)  and Lookups (See LookupConfiguration) you want
    /// to execute on this pass.  You can only select datasets etc which are part of the currently executing project configuration (See ExecuteExtractionUI).
    /// 
    /// <para>All artifacts are extracted in parallel into the data extraction directory of the project (in a subfolder for the configuration).  This execution will override any files it finds
    /// for previous executions of the same dataset but will not delete unticked datasets.  This allows you to run an extraction overnight of half the datasets and then do the other half
    /// the next night.</para>
    /// </summary>
    public partial class ChooseExtractablesUI : RDMPUserControl
    {
        private const string Bundles = "Datasets";
        
        public event ExtractCommandSelectedHandler CommandSelected;

        private GlobalsBundle _lastDispatchedGlobalsBundle = null;

        private Dictionary<string, List<object>> Categories = new Dictionary<string, List<object>>();
        
        public ChooseExtractablesUI()
        {
            InitializeComponent();

            tlvDatasets.RowFormatter += RowFormatter;
        }

        private void RowFormatter(OLVListItem olvItem)
        {
            var eds = olvItem.RowObject as ExtractableDataSet;
            var command = olvItem.RowObject as IExtractDatasetCommand;

            if (eds != null && eds.DisableExtraction)
                olvItem.ForeColor = Color.Red;

            if (command != null && command.DatasetBundle.DataSet.DisableExtraction)
                olvItem.ForeColor = Color.Red;

        }
        
        public IExtractCommand[] GetFinalExtractCommands()
        {
            var uncheckedObjects = tlvDatasets.Objects.Cast<object>().Except(tlvDatasets.CheckedObjects.Cast<object>());
            foreach (var uncheckObject in uncheckedObjects)
            {
                var p = tlvDatasets.GetParent(uncheckObject);
                var dsCommandParent = p as IExtractDatasetCommand;
                if (dsCommandParent != null)
                    dsCommandParent.DatasetBundle.DropContent(uncheckObject);

            }
            return tlvDatasets.CheckedObjects.OfType<IExtractCommand>().ToArray();
        }
        
        public void Setup(ExtractionConfiguration configuration)
        {
            olvColumn1.ImageGetter += ImageGetter;
            
            Categories.Clear();
            Categories.Add(ExtractionDirectory.GLOBALS_DATA_NAME, new List<object>());
            Categories.Add(Bundles, new List<object>());
            
            var factory = new ExtractCommandCollectionFactory();
            var collection = factory.Create(Activator.RepositoryLocator, configuration);

            //find all the things that are available for extraction
            
            //add globals to the globals category
            Categories[ExtractionDirectory.GLOBALS_DATA_NAME].AddRange(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<SupportingDocument>().Where(d => d.IsGlobal && d.Extractable));

            //add global SQLs to globals category
            Categories[ExtractionDirectory.GLOBALS_DATA_NAME].AddRange(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<SupportingSQLTable>().Where(s => s.IsGlobal && s.Extractable));

            //add the bundle
            Categories[Bundles].AddRange(collection.Datasets);

            tlvDatasets.SetObjects(Categories.Keys);
            
            //Can expand dataset bundles and also can expand strings for which the dictionary has a key for that string (i.e. the strings "Custom Tables" etc)
            tlvDatasets.CanExpandGetter = x => x is ExtractDatasetCommand || (x is string && Categories.ContainsKey((string) x));
            tlvDatasets.ChildrenGetter += ChildrenGetter;

            tlvDatasets.CheckAll();
     
            tlvDatasets.ExpandAll();

            foreach (IExtractDatasetCommand eds in Categories[Bundles])
                if (eds.DatasetBundle.DataSet.DisableExtraction)
                    tlvDatasets.UncheckObject(eds);
        }

        private IEnumerable ChildrenGetter(object model)
        {
            if(model is string)
                foreach (object o in Categories[(string) model])
                    yield return o;

            IExtractDatasetCommand ds = model as IExtractDatasetCommand;
            if (ds != null)
            {
                yield return ds.DatasetBundle.DataSet;

                foreach (SupportingDocument o in ds.DatasetBundle.Documents)
                    yield return o;

                foreach (SupportingSQLTable o in ds.DatasetBundle.SupportingSQL)
                    yield return o;

                foreach (IBundledLookupTable o in ds.DatasetBundle.LookupTables)
                    yield return o;
            }
        }

        private object ImageGetter(object rowObject)
        {
            //if we have dispatched the globals
            if (_lastDispatchedGlobalsBundle != null)
                if (_lastDispatchedGlobalsBundle.States.ContainsKey(rowObject)) //and this object was in the global bundle dispatched
                    return GetStringForState(_lastDispatchedGlobalsBundle.States[rowObject]); //return an appropriate icon that reflects it's status
                        
            //it is not a global or globals haven't been dispatched yet

            //see if it is from a non global bundle
            var objectIsFromAKnownBundle = GetBundleIfAnyForObject(rowObject);
                
            //it IS from a non global bundle
            if (objectIsFromAKnownBundle != null)
            {
                //see if we have a state icon for this 
                string icon = GetStringForState(objectIsFromAKnownBundle.States[rowObject]);
                if (icon != null)
                    return icon;
            }
            
            //It was not a known state, probably means that it has not been started or is in NotLaunched state, so give a better icon

            if (rowObject is string)
                return "folder.bmp";
            
            if (rowObject.GetType() == typeof (SupportingDocument))
                return "supportingdocument.bmp";

            if(rowObject.GetType() == typeof(SupportingSQLTable) 
                ||
                rowObject.GetType() == typeof(BundledLookupTable))
                return "sql.bmp";

            if (rowObject.GetType() == typeof(ExtractableDataSet))
            {
                var command = Categories[Bundles].Cast<IExtractDatasetCommand>().Single(d => d.DatasetBundle.DataSet.Equals(rowObject));
                return GetStringForState(command.State) ?? "extractabledataset.bmp";
            }
            
            if (rowObject is IExtractCommand)
                return GetStringForState(((IExtractCommand)rowObject).State) ?? "bundle.bmp";

            return null;
        }

        private string GetStringForState(ExtractCommandState state)
        {
            //and we know the state
            //get an appropriate icon for the state it is in
            switch (state)
            {
                case ExtractCommandState.NotLaunched:return null;
                case ExtractCommandState.WaitingToExecute: return "sleeping.bmp";
                case ExtractCommandState.WaitingForSQLServer: return "talkingtoSQL.bmp";
                case ExtractCommandState.WritingToFile: return "writing.bmp";
                case ExtractCommandState.Crashed: return "failed.bmp";
                case ExtractCommandState.UserAborted: return "stopped.bmp";
                case ExtractCommandState.Completed: return "tick.bmp";
                case ExtractCommandState.Warning: return "warning.bmp";
                case ExtractCommandState.WritingMetadata: return "word.bmp";
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }

        private void lvDatasets_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            var item = (OLVListItem)tlvDatasets.HitTest(e.Location).Item;


            if (item == null)
                return;

            var doc = item.RowObject as SupportingDocument;
            if (doc != null)
                try
                {
                    UsefulStuff.GetInstance().ShowFileInWindowsExplorer(doc.GetFileName());
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show(ex);
                }

            var sql = item.RowObject as SupportingSQLTable;
            if (sql != null)
                try
                {

                    if (sql.ExternalDatabaseServer_ID == null)
                        throw new NotSupportedException("There is no known server for SupportingSql " + sql);

                    var server = sql.ExternalDatabaseServer;
                    DataTableViewerUI dtv = new DataTableViewerUI(server, sql.SQL, sql.Name);
                    dtv.Show();
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show(ex);
                }


            //User double clicks a lookup table
            var lookup = item.RowObject as BundledLookupTable;

            if (lookup != null)
            {
                ViewSQLAndResultsWithDataGridUI f = new ViewSQLAndResultsWithDataGridUI();
                f.SetCollection(null, new ViewTableInfoExtractUICollection(lookup.TableInfo,ViewType.TOP_100));
                f.Show();
            }
        }
        
        public GlobalsBundle GetGlobalsBundle()
        {
            //get the checked globals
            var docs = Categories[ExtractionDirectory.GLOBALS_DATA_NAME].OfType<SupportingDocument>().Where(g => tlvDatasets.CheckedObjects.Contains(g)).ToArray();

            //and the checked global supporting sql
            var sqls = Categories[ExtractionDirectory.GLOBALS_DATA_NAME].OfType<SupportingSQLTable>().Where(g => tlvDatasets.CheckedObjects.Contains(g)).ToArray();
            
            //record it so we can decide what icon to use
            _lastDispatchedGlobalsBundle = new GlobalsBundle(docs, sqls);
            return _lastDispatchedGlobalsBundle;
        }
        

        private void tlvDatasets_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                var item =(OLVListItem) e.Item;
                var bundle = item.RowObject as IExtractCommand;

                if (bundle != null && CommandSelected != null)
                    CommandSelected(this, bundle);
            }
        }


        private void tlvDatasets_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var obj = ((OLVListItem) e.Item).RowObject;

            //if it is a dataset
            var eds = obj as ExtractableDataSet;
            //if is it bundled object (get the parent bundle)
            var bundle = GetBundleIfAnyForObject(obj);

            if(eds != null && eds.DisableExtraction )
                tlvDatasets.UncheckObject(eds);

            if(bundle != null && bundle.DataSet.DisableExtraction)
                tlvDatasets.UncheckObject(bundle);
            
            //if user is trying to uncheck a dataset
            if (eds != null && !IsChecked(obj))
            {
                //also uncheck the bundle
                tlvDatasets.UncheckObject(
                    Categories[Bundles].Cast<IExtractDatasetCommand>().Single(b => b.DatasetBundle.DataSet == obj));

                return;
            }

            //if it is from a bundle then it could be lookup or attachment or something
            if (bundle != null && IsChecked(obj))
                tlvDatasets.CheckObject(bundle.DataSet); //so also tick the dataset
        }


        //Helper methods
        private IExtractableDatasetBundle GetBundleIfAnyForObject(object rowObject)
        {
            var matchingCommand = Categories[Bundles].Cast<IExtractDatasetCommand>().SingleOrDefault(b => b.DatasetBundle.States.ContainsKey(rowObject));

            if(matchingCommand == null)
                return null;

            return matchingCommand.DatasetBundle;
        }

        private bool IsChecked(object obj)
        {
            return tlvDatasets.CheckedObjects.Contains(obj);
        }

        public IExtractableDataSet[] GetCheckedDatasets()
        {
            return tlvDatasets.CheckedObjects.OfType<ExtractDatasetCommand>().Select(c=>c.DatasetBundle.DataSet).ToArray();
        }

        ExtractCommandStateMonitor monitor = new ExtractCommandStateMonitor();
        

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (VisualStudioDesignMode)
                return;
            try
            {
                List<object> toRefresh = new List<object>();

                if (_lastDispatchedGlobalsBundle != null)
                {
                    toRefresh.AddRange(monitor.GetAllChangedObjects(_lastDispatchedGlobalsBundle));
                    monitor.SaveState(_lastDispatchedGlobalsBundle);
                }
                
                if (!Categories.Any())
                   return;

                foreach (IExtractDatasetCommand cmd in Categories[Bundles].ToArray())
                {
                    if (!monitor.Contains(cmd))
                        monitor.Add(cmd);
                    
                    toRefresh.AddRange(monitor.GetAllChangedObjects(cmd));
                    monitor.SaveState(cmd);
                }

                if(toRefresh.Any())
                    tlvDatasets.RefreshObjects(toRefresh);
                
            }
            catch (Exception)
            {
                timer1.Stop();
                throw;
            }
        }
    }
}
