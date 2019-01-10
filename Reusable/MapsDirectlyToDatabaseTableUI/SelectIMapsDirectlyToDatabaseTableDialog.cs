using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableUIComponents;
using ReusableUIComponents.Settings;


namespace MapsDirectlyToDatabaseTableUI
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
    public partial class SelectIMapsDirectlyToDatabaseTableDialog : Form
    {
        private readonly bool _allowDeleting;
        public IMapsDirectlyToDatabaseTable Selected;

        public static Func<object, Image> ImageGetter;

        public SelectIMapsDirectlyToDatabaseTableDialog(IEnumerable<IMapsDirectlyToDatabaseTable> toSelectFrom, bool allowSelectingNULL,bool allowDeleting)
        {
            _allowDeleting = allowDeleting;
            InitializeComponent();
            
            if (ImageGetter != null)
            {
                olvName.ImageGetter = (model) => ImageGetter(model);
                olvObjects.RowHeight = 19;
            }

            if (toSelectFrom == null)
                return;
            
            if (!allowSelectingNULL)
            {
                //disable the option to select NULL
                btnSelectNULL.Visible = false;

                //move this button down so it doesn't look weird
                btnSelect.Location = new Point(btnSelectNULL.Location.X,btnSelectNULL.Location.Y);
            }

            //default to not allowing multi selection
            olvObjects.MultiSelect = false;
            btnSelect.Enabled = false;

            //Array them
            var o = toSelectFrom.ToArray();
            
            //Add them to the tree view
            olvObjects.AddObjects(o);


            //If there were any
            if(o.Any())
            {
                //Set Width of the Form to accommodate all names no matter how long
                var pixelWidthofWidestText = o.Max(s =>TextRenderer.MeasureText( s.ToString(),olvObjects.Font).Width);

                //But don't make it too small (smaller than the form designer shows or larger than 1000 pixels)
                this.Width = Math.Min(Math.Max(Width,100 + pixelWidthofWidestText),1000);
            }

            AddUsefulPropertiesIfHomogeneousTypes(o);

            olvSelected.CheckBoxes = true;
            olvSelected.AspectGetter += Selected_AspectGetter;
            olvSelected.AspectPutter += Selected_AspectPutter;
            olvSelected.IsVisible = false;

            olvObjects.RebuildColumns();
            
            MultiSelected = new HashSet<IMapsDirectlyToDatabaseTable>();

            olvSelected.GroupWithItemCountFormat = "{0} ({1} objects)";
            olvSelected.GroupWithItemCountSingularFormat = "{0} (1 objects)";
            olvSelected.GroupKeyGetter += GroupKeyGetter;
        }

        private object GroupKeyGetter(object rowObject)
        {
            if (MultiSelected == null)
                return false;

            return MultiSelected.Contains(rowObject)? "Selected": "Not Selected";
        }

        private bool buildGroupsRequired = false;

        private void Selected_AspectPutter(object rowobject, object newvalue)
        {
            var b = (bool) newvalue;
            if (b)
                MultiSelected.Add((IMapsDirectlyToDatabaseTable) rowobject);
            else
                MultiSelected.Remove((IMapsDirectlyToDatabaseTable) rowobject);
            
            //olvObjects.BuildGroups();
            buildGroupsRequired = true;

            UpdateButtonEnabledness();
        }

        
        private object Selected_AspectGetter(object rowObject)
        {
            if (!AllowMultiSelect)
                return null;

            return MultiSelected.Contains(rowObject);
        }

        private void AddUsefulPropertiesIfHomogeneousTypes(IMapsDirectlyToDatabaseTable[] mapsDirectlyToDatabaseTables)
        {
            var types = mapsDirectlyToDatabaseTables.Select(m => m.GetType()).Distinct().ToArray();

            bool addedColumns = false;

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
                        addedColumns = true;
                    }
                }
            }
            else
            {
                //they are all different types!
                var newCol = new OLVColumn( "Type",null);
                newCol.AspectGetter += TypeAspectGetter;
                olvObjects.AllColumns.Add(newCol);
                addedColumns = true;
            }

            if (addedColumns)
                olvObjects.RebuildColumns();
        }

        private object TypeAspectGetter(object rowObject)
        {
            return rowObject.GetType().Name;
        }

        public HashSet<IMapsDirectlyToDatabaseTable> MultiSelected { get; private set; }

        public bool AllowMultiSelect
        {
            get { return olvObjects.MultiSelect; }
            set
            {
                olvObjects.MultiSelect = value;
                olvSelected.IsVisible = value;

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

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (!AllowMultiSelect)
                Selected = (IMapsDirectlyToDatabaseTable) olvObjects.SelectedObject;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSelectNULL_Click(object sender, EventArgs e)
        {
            Selected = null;
            MultiSelected = null;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Selected = null;
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
                Selected =  olvObjects.SelectedObject as IMapsDirectlyToDatabaseTable;

                if (Selected == null)
                    return;

                MultiSelected = new HashSet<IMapsDirectlyToDatabaseTable>(new []{ Selected });
                this.Close();
            }

            //space flips the selectedness of the objects that are selected
            if (e.KeyCode == Keys.Space && AllowMultiSelect && olvObjects.SelectedObjects != null)
            {
                foreach (IMapsDirectlyToDatabaseTable o in olvObjects.SelectedObjects)
                {
                    if (MultiSelected.Contains(o))
                        MultiSelected.Remove(o);
                    else
                        MultiSelected.Add(o);
                }
                olvObjects.RebuildColumns();
            }
        }

        public void SetInitialFilter(string text)
        {
            if (!string.IsNullOrEmpty(text))
                tbFilter.Text = text;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvObjects.ModelFilter = new TextMatchFilterWithWhiteList(MultiSelected,olvObjects,tbFilter.Text,StringComparison.InvariantCultureIgnoreCase);
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
                Selected = olvObjects.SelectedObject as IMapsDirectlyToDatabaseTable;

                if (Selected == null)
                    return;

                MultiSelected = new HashSet<IMapsDirectlyToDatabaseTable>(new[] { Selected });
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
