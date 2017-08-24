using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;


namespace MapsDirectlyToDatabaseTableUI
{
    /// <summary>
    /// This dialog prompts you to select a single RDMP object from a selection of objects.  This dialog is reused in many places throughout RDMP so you will need to rely on the context
    /// the dialog was launched in to determine what the effects of a given selection are.
    /// 
    /// As an example you might be prompted to select a Catalogue (dataset) from a list of all active Catalogues.  Optionally you might be able to select Null (usually clears a currently 
    /// selected value).
    /// 
    /// If the dialog was launched in read/write mode then pressing Del will delete the currently selected entity (if possible... don't do this unless you really do want to delete it).
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
                listBox1.RowHeight = 19;
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
            listBox1.MultiSelect = false;

            btnSelect.Enabled = false;

            //Array them
            var o = toSelectFrom.ToArray();
            
            //Add them to the tree view
            listBox1.AddObjects(o);


            //If there were any
            if(o.Any())
            {
                //Set Width of the Form to accommodate all names no matter how long
                var pixelWidthofWidestText = o.Max(s =>TextRenderer.MeasureText( s.ToString(),listBox1.Font).Width);

                //But don't make it too small (smaller than the form designer shows or larger than 1000 pixels)
                this.Width = Math.Min(Math.Max(Width,100 + pixelWidthofWidestText),1000);
            }

            listBox1.AfterSorting += listBox1_AfterSorting;
            try
            {

                var previousSetting = RecentHistoryOfControls.GetInstance().GetList("SelectIMapsSortOrder");

                if (previousSetting != null && previousSetting.Count == 1)
                {
                    string[] split = previousSetting[0].Split('|');

                    if(split.Length != 2)
                        return;

                    if(string.IsNullOrWhiteSpace(split[0]))
                        return;

                    if (string.IsNullOrWhiteSpace(split[1]))
                        return;

                    OLVColumn sortCol = listBox1.GetColumn(split[0]);
                    SortOrder sortOrder;
                        
                    if(!SortOrder.TryParse(split[1],out sortOrder))
                        return;
                
                    listBox1.Sort(sortCol,sortOrder);
                }
            }
            catch (Exception e)
            {
                //Previous value extraction failed, ah well nevermind eh
                Console.WriteLine(e);
            }

            
        }

        void listBox1_AfterSorting(object sender, AfterSortingEventArgs e)
        {
            RecentHistoryOfControls.GetInstance().Clear("SelectIMapsSortOrder");

            if (listBox1.LastSortColumn != null)
                RecentHistoryOfControls.GetInstance().AddResults("SelectIMapsSortOrder", listBox1.LastSortColumn.Text + "|" + listBox1.LastSortOrder);
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> MultiSelected { get; set; }

        public bool AllowMultiSelect
        {
            get { return listBox1.MultiSelect; }
            set { listBox1.MultiSelect = value; }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (AllowMultiSelect)
                MultiSelected = listBox1.SelectedObjects.Cast<IMapsDirectlyToDatabaseTable>();
            else
                Selected = (IMapsDirectlyToDatabaseTable) listBox1.SelectedObject;

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
            if (AllowMultiSelect)
                btnSelect.Enabled = listBox1.SelectedObjects.Cast<IMapsDirectlyToDatabaseTable>().Any();
            else
                btnSelect.Enabled = listBox1.SelectedObject != null;
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
            var deletable = listBox1.SelectedObject as IDeleteable;
            if (e.KeyCode == Keys.Delete && _allowDeleting && deletable != null)
            {
                if(MessageBox.Show("Confirm deleting " + deletable,"Really delete?",MessageBoxButtons.YesNoCancel)== DialogResult.Yes)
                {
                    try
                    {
                        deletable.DeleteInDatabase();
                        listBox1.RemoveObject(deletable);
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                    }
                }
            }

            if (e.KeyCode == Keys.Enter && listBox1.SelectedObject != null)
            {
                DialogResult = DialogResult.OK;
                Selected =  listBox1.SelectedObject as IMapsDirectlyToDatabaseTable;

                if (Selected == null)
                    return;

                MultiSelected = new[] { Selected };
                this.Close();
            }

        }


        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            listBox1.ModelFilter = new TextMatchFilter(listBox1,tbFilter.Text,StringComparison.InvariantCultureIgnoreCase);

        }


        private void tbFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                if (!listBox1.Focused)
                {
                    listBox1.Focus();
                    SendKeys.Send(e.KeyCode == Keys.Up ? "{UP}":"{DOWN}");
                }

        
        }

        private void listBox1_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                Selected = listBox1.SelectedObject as IMapsDirectlyToDatabaseTable;

                if (Selected == null)
                    return;

                MultiSelected = new[] { Selected };
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

    }
}
