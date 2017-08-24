using BrightIdeasSoftware;

namespace CatalogueManager.SimpleDialogs.Automation
{
    partial class LockableOverviewUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LockableOverviewUI));
            this.olvLockables = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLockedBy = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvUnlock = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checksUIIconOnly1 = new ReusableUIComponents.ChecksUI.ChecksUIIconOnly();
            ((System.ComponentModel.ISupportInitialize)(this.olvLockables)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvLockables
            // 
            this.olvLockables.AllColumns.Add(this.olvName);
            this.olvLockables.AllColumns.Add(this.olvType);
            this.olvLockables.AllColumns.Add(this.olvID);
            this.olvLockables.AllColumns.Add(this.olvLockedBy);
            this.olvLockables.AllColumns.Add(this.olvUnlock);
            this.olvLockables.CellEditUseWholeCell = false;
            this.olvLockables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvID,
            this.olvLockedBy,
            this.olvUnlock});
            this.olvLockables.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvLockables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvLockables.Location = new System.Drawing.Point(3, 16);
            this.olvLockables.Name = "olvLockables";
            this.olvLockables.Size = new System.Drawing.Size(425, 243);
            this.olvLockables.SmallImageList = this.imageList1;
            this.olvLockables.TabIndex = 0;
            this.olvLockables.UseCompatibleStateImageBehavior = false;
            this.olvLockables.View = System.Windows.Forms.View.Details;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            // 
            // olvType
            // 
            this.olvType.AspectName = "";
            this.olvType.DisplayIndex = 1;
            this.olvType.IsVisible = false;
            this.olvType.Text = "Type";
            // 
            // olvID
            // 
            this.olvID.AspectName = "ID";
            this.olvID.Groupable = false;
            this.olvID.Text = "ID";
            // 
            // olvLockedBy
            // 
            this.olvLockedBy.AspectName = "LockHeldBy";
            this.olvLockedBy.Text = "Locked By";
            this.olvLockedBy.Width = 100;
            // 
            // olvUnlock
            // 
            this.olvUnlock.Groupable = false;
            this.olvUnlock.IsButton = true;
            this.olvUnlock.Text = "Unlock";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "padlockSquare.png");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checksUIIconOnly1);
            this.groupBox1.Controls.Add(this.olvLockables);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 262);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Locked Objects";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checksUIIconOnly1
            // 
            this.checksUIIconOnly1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUIIconOnly1.Location = new System.Drawing.Point(405, 236);
            this.checksUIIconOnly1.Name = "checksUIIconOnly1";
            this.checksUIIconOnly1.Size = new System.Drawing.Size(20, 20);
            this.checksUIIconOnly1.TabIndex = 2;
            // 
            // LockableOverviewUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "LockableOverviewUI";
            this.Size = new System.Drawing.Size(431, 262);
            ((System.ComponentModel.ISupportInitialize)(this.olvLockables)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView olvLockables;
        private System.Windows.Forms.GroupBox groupBox1;
        private OLVColumn olvName;
        private OLVColumn olvType;
        private OLVColumn olvID;
        private OLVColumn olvUnlock;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Timer timer1;
        private OLVColumn olvLockedBy;
        private ReusableUIComponents.ChecksUI.ChecksUIIconOnly checksUIIconOnly1;
    }
}
