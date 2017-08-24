using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Reports;
using ReusableUIComponents;

namespace CatalogueManager.DashboardTabs
{
    /// <summary>
    /// TECHNICAL: wrapper class for a hosted IDashboardableControl.  Is responsible for rendering the close box and the border of the control.
    /// </summary>
    [TechnicalUI]
    public partial class DashboardableControlHostPanel : UserControl
    {
        private readonly IActivateItems _activator;
        private readonly DashboardControl _databaseRecord;
        private bool _editMode;
        public IDashboardableControl HostedControl { get; private set; }

        private const float BorderWidth = 5;

        public DashboardableControlHostPanel(IActivateItems activator, DashboardControl databaseRecord, IDashboardableControl hostedControl)
        {
            
            _activator = activator;
            _databaseRecord = databaseRecord;
            HostedControl = hostedControl;
            InitializeComponent();
            
            pbDelete.Image = FamFamFamIcons.delete;

            Margin = Padding.Empty;

            pbDelete.Visible = false;
            
            this.Controls.Add((Control)HostedControl);
            
            AdjustControlLocation();
        }

        private void AdjustControlLocation()
        {
            var control = ((Control)HostedControl);

            //center it on us with a gap of BorderWidth
            if (_editMode)
            {

                control.Location = new Point((int) BorderWidth, (int) BorderWidth);
                control.Width = (int) (Width - (BorderWidth*2));
                control.Height = (int) (Height - (BorderWidth*2));
            }
            else
            {
                control.Location = new Point(0,0);
                control.Width = Width;
                control.Height = Height;
            }

            //anchor to all
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_editMode)
                e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);
        }
        
        public void NotifyEditModeChange(bool isEditModeOn)
        {
            _editMode = isEditModeOn;
            AdjustControlLocation();
            HostedControl.NotifyEditModeChange(isEditModeOn);

            pbDelete.Visible = isEditModeOn;
            
            if(isEditModeOn)
                pbDelete.BringToFront();

            Invalidate();
        }


        private void pbDelete_Click(object sender, EventArgs e)
        {
            if(_editMode)
                _activator.DeleteControlFromDashboardWithConfirmation(this, _databaseRecord);
        }
    }
}
