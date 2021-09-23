// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers.Filtering;


namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// This dialog prompts you to select a single RDMP object from a selection of objects.  This dialog is reused in many places throughout RDMP so you will need to rely on the context
    /// the dialog was launched in to determine what the effects of a given selection are.
    /// 
    /// <para>As an example you might be prompted to select a Catalogue (dataset) from a list of all active Catalogues.  Optionally you might be able to select Null (usually clears a currently 
    /// selected value).</para>
    /// 
    /// <para>If the dialog was launched in read/write mode then pressing Del will delete the currently selected entity (if possible... don't do this unless you really do want to delete it).</para>
    /// </summary>
    public partial class SelectDialog<T> : Form where T : class
    {
        private readonly IBasicActivateItems _activator;
        private readonly bool _allowDeleting;
        public T Selected;
                
        public const int MaxObjectsToShow = 1000;

        private bool _useCatalogueFilter = false;

        public SelectDialog(IBasicActivateItems activator,IEnumerable<T> toSelectFrom, bool allowSelectingNULL,bool allowDeleting)
        {
            _activator = activator;
            _allowDeleting = allowDeleting;
            InitializeComponent();

            //start at cancel so if they hit the X nothing is selected
            DialogResult = DialogResult.Cancel;

            olvID.AspectGetter = (m) => (m as IMapsDirectlyToDatabaseTable)?.ID??null;

            // don't add the ID column if we aren't talking about database objects
            if(!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(typeof(T)))
            {
                olvObjects.AllColumns.Remove(olvID);
            }

            olvName.AspectGetter = (m) => m.ToString();
            
            olvObjects.ListFilter = new TailFilter(MaxObjectsToShow);

            olvName.ImageGetter = GetImage;
            olvObjects.RowHeight = 19;
            

            if (toSelectFrom == null)
                return;
            
            if (!allowSelectingNULL)
            {
                //disable the option to select NULL
                btnSelectNULL.Visible = false;
            }

            //default to not allowing multi selection
            olvObjects.MultiSelect = false;
            btnSelect.Enabled = false;

            //Array them
            var o = toSelectFrom.ToArray();
            
            //Add them to the tree view
            olvObjects.AddObjects(o);

            if (o.Length>0 && IsBasicallyAllCatalogues(o))
            {
                splitContainer1.Panel2Collapsed = false;
                _useCatalogueFilter = true;
                catalogueCollectionFilterUI1.FiltersChanged += (s,e)=>ApplyFilter();
            }
            else
                splitContainer1.Panel2Collapsed = true;

            //If there were any
            if(o.Any())
            {
                //Set Width of the Form to accommodate all names no matter how long
                var pixelWidthofWidestText = o.Max(s =>TextRenderer.MeasureText( s.ToString(),olvObjects.Font).Width);

                //But don't make it too small (smaller than the form designer shows or larger than 1000 pixels)
                this.Width = Math.Min(Math.Max(Width,100 + pixelWidthofWidestText),1000);
            }

            AddUsefulPropertiesIfHomogeneousTypes(o);

            // Setup olvSelected but leave it removed for now (IsVisible is problematic - especially for first columns)
            olvSelected.CheckBoxes = true;
            olvSelected.AspectGetter += Selected_AspectGetter;
            olvSelected.AspectPutter += Selected_AspectPutter;
            olvObjects.AllColumns.Remove(olvSelected);

            olvObjects.RebuildColumns();
            
            MultiSelected = new HashSet<T>();

            olvSelected.GroupWithItemCountFormat = "{0} ({1} objects)";
            olvSelected.GroupWithItemCountSingularFormat = "{0} (1 objects)";
            olvSelected.GroupKeyGetter += GroupKeyGetter;
            
            ApplyFilter();
        }

        private bool IsBasicallyAllCatalogues(T[] o)
        {
            return o.All(c => c is ICatalogue || c is ExtractableDataSet || c is SelectedDataSets);
        }

        private object GroupKeyGetter(object rowObject)
        {
            if (MultiSelected == null)
                return false;

            return MultiSelected.Contains((T)rowObject)? "Selected": "Not Selected";
        }

        private bool buildGroupsRequired = false;

        private void Selected_AspectPutter(object rowobject, object newvalue)
        {
            var b = (bool) newvalue;
            if (b)
                MultiSelected.Add((T) rowobject);
            else
                MultiSelected.Remove((T) rowobject);
            
            //olvObjects.BuildGroups();
            buildGroupsRequired = true;

            UpdateButtonEnabledness();
        }

        
        private object Selected_AspectGetter(object rowObject)
        {
            if (!AllowMultiSelect)
                return null;

            return MultiSelected.Contains((T)rowObject);
        }

        private void AddUsefulPropertiesIfHomogeneousTypes(T[] mapsDirectlyToDatabaseTables)
        {
            var types = mapsDirectlyToDatabaseTables.Select(m => m.GetType()).Distinct().ToArray();

            //objects are not homogeneous
            if (types.Length == 1)
            {
                //all objects are the same Type

                //look for useful properties
                foreach (PropertyInfo propertyInfo in types[0].GetProperties())
                {
                    if (propertyInfo.GetCustomAttributes(typeof(UsefulPropertyAttribute), true).Any())
                    {
                        //add a column
                        var newCol = new OLVColumn(propertyInfo.Name, propertyInfo.Name);
                        olvObjects.AllColumns.Add(newCol);
                    }
                }
            }
            else
            {
                //they are all different types!

                // are they all database objects (e.g. Catalogue, Project etc)
                if(typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(typeof(T)))
                {
                    //yes, then tell the user what they are with this exciting new column
                    var newCol = new OLVColumn("Type", null);
                    newCol.AspectGetter += TypeAspectGetter;
                    olvObjects.AllColumns.Add(newCol);
                }
            }
        }

        private object TypeAspectGetter(object rowObject)
        {
            return rowObject.GetType().Name;
        }

        public HashSet<T> MultiSelected { get; private set; }

        public void SetInitialSelection(IEnumerable<T> toSelect)
        {
            MultiSelected = new HashSet<T>(toSelect);
            ApplyFilter();
        }

        public bool AllowMultiSelect
        {
            get { return olvObjects.MultiSelect; }
            set
            {
                olvObjects.MultiSelect = value;
                if(value)
                {
                    if(!olvObjects.AllColumns.Contains(olvSelected))
                    {
                        olvObjects.AllColumns.Add(olvSelected);
                    }
                }
                else
                {
                    olvObjects.AllColumns.Remove(olvSelected);
                }

                if (value)
                {
                    olvObjects.ShowGroups = true;
                    olvObjects.AlwaysGroupByColumn = olvSelected;
                    olvObjects.AlwaysGroupBySortOrder = SortOrder.Descending;
                    olvObjects.ShowItemCountOnGroups = true;   
                }
                else
                {

                    olvObjects.AlwaysGroupByColumn = null;
                    olvObjects.ShowGroups = false;
                }

                olvObjects.RebuildColumns();
            }
        }

        public Bitmap GetImage(object model)
        {
            var bmp = _activator.CoreIconProvider.GetImage(model);
            return bmp == _activator.CoreIconProvider.ImageUnknown ? null : bmp;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (!AllowMultiSelect)
                Selected = (T) olvObjects.SelectedObject;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSelectNULL_Click(object sender, EventArgs e)
        {
            Selected = default(T);
            MultiSelected = null;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Selected = default(T);
            MultiSelected = null;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabledness();
        }

        private void UpdateButtonEnabledness()
        {
            if (AllowMultiSelect)
                btnSelect.Enabled = MultiSelected.Any();
            else
                btnSelect.Enabled = olvObjects.SelectedObject != null;
        }

        private void SelectIMapsDirectlyToDatabaseTableDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            var deletable = olvObjects.SelectedObject as IDeleteable;
            if (e.KeyCode == Keys.Delete && _allowDeleting && deletable != null)
            {
                if(MessageBox.Show("Confirm deleting " + deletable,"Really delete?",MessageBoxButtons.YesNoCancel)== DialogResult.Yes)
                {
                    try
                    {
                        deletable.DeleteInDatabase();
                        olvObjects.RemoveObject(deletable);
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                    }
                }
            }

            if (e.KeyCode == Keys.Enter && olvObjects.SelectedObject != null)
            {
                DialogResult = DialogResult.OK;
                Selected =  olvObjects.SelectedObject is T s ? s : default(T);

                if (Selected == null)
                    return;

                MultiSelected = new HashSet<T>(new []{ Selected });
                this.Close();
            }

            //space flips the selectedness of the objects that are selected
            if (e.KeyCode == Keys.Space && AllowMultiSelect && olvObjects.SelectedObjects != null)
            {
                foreach (T o in olvObjects.SelectedObjects)
                {
                    if (MultiSelected.Contains(o))
                        MultiSelected.Remove(o);
                    else
                        MultiSelected.Add(o);
                }
                olvObjects.RebuildColumns();
                UpdateButtonEnabledness();
            }
        }

        public void SetInitialFilter(string text)
        {
            if (!string.IsNullOrEmpty(text))
                tbFilter.Text = text;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {            
            var modelFilter = new TextMatchFilterWithWhiteList(MultiSelected,olvObjects,tbFilter.Text,StringComparison.InvariantCultureIgnoreCase);
            olvObjects.ListFilter = new CherryPickingTailFilter(MaxObjectsToShow,modelFilter);

            olvObjects.ModelFilter = _useCatalogueFilter ? 
                (IModelFilter) new CompositeAllFilter(new List<IModelFilter>{modelFilter,new CatalogueCollectionFilter(_activator.CoreChildProvider)})
                : modelFilter;
            

        }
        
        private void tbFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                if (!olvObjects.Focused)
                {
                    olvObjects.Focus();
                    SendKeys.Send(e.KeyCode == Keys.Up ? "{UP}":"{DOWN}");
                }
        }
        
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        private void listBox1_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                Selected = olvObjects.SelectedObject as T;

                if (Selected == null)
                    return;

                //double clicking on a row when several others are selected should not make it the only selected item
                if (AllowMultiSelect)
                {
                    //instead it should just add it to the multi selection
                    MultiSelected.Add(Selected);
                    buildGroupsRequired = true;

                    UpdateButtonEnabledness();
                    return;
                }

                MultiSelected = new HashSet<T>(new[] { Selected });
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (buildGroupsRequired)
            {
                buildGroupsRequired = false;
                olvObjects.BuildGroups();
            }
        }
    }
}
