using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;

namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    /// <summary>
    /// Part of JoinConfiguration and LookupConfiguration, allows you to drop a ColumnInfo into it to declare it as a key in a relationship being built (either a Lookup or a JoinInfo). Clicking
    /// the garbage can will clear the control.
    /// </summary>
    public partial class KeyDropLocationUI : UserControl
    {
        private JoinKeyType _keyType;
        public ColumnInfo SelectedColumn { get; private set; }
        
        public JoinKeyType KeyType
        {
            get { return _keyType; }
            set
            {
                _keyType = value;
                switch (KeyType)
                {
                    case JoinKeyType.PrimaryKey:
                        label.Text = "(Primary Key)";
                        break;
                    case JoinKeyType.ForeignKey:
                        label.Text = "(Foreign Key)";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Set this to allow dragging only certain items onto the control.  Return true to allow drop and false to prevent it.
        /// </summary>
        public Func<ColumnInfo, bool> IsValidGetter { get; set; }

        public event Action SelectedColumnChanged;

        public KeyDropLocationUI()
        {
            InitializeComponent();
            btnClear.Image = FamFamFamIcons.delete;
            btnClear.Enabled = false;
        }
        
        private void tbPk1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            var col = GetColumnInfoOrNullFromDrag(e);

            if(IsValidGetter != null && !IsValidGetter(col))
                return;
            
            if(col != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void tbPk1_DragDrop(object sender, DragEventArgs e)
        {
            SelectedColumn = GetColumnInfoOrNullFromDrag(e);
            tbPk1.Text = SelectedColumn.ToString();
            btnClear.Enabled = true;

            if (SelectedColumnChanged != null)
                SelectedColumnChanged();
        }

        private ColumnInfo GetColumnInfoOrNullFromDrag(DragEventArgs e)
        {
            var data = e.Data as OLVDataObject;

            if (data == null)
                return null;

            if (data.ModelObjects.Count != 1)
                return null;

            var ei = data.ModelObjects[0] as ExtractionInformation;

            if (ei != null)
                return ei.ColumnInfo;

            return data.ModelObjects[0] as ColumnInfo;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        public void Clear()
        {
            tbPk1.Text = "";
            SelectedColumn = null;
            btnClear.Enabled = false;

            if (SelectedColumnChanged != null)
                SelectedColumnChanged();
        }
    }
    public enum JoinKeyType
    {
        PrimaryKey,
        ForeignKey
    }
}
