using BrightIdeasSoftware;

namespace CatalogueManager.SimpleDialogs.Automation
{
    partial class AutomationServiceSlotManagement
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.automationServiceSlots = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.automationServiceSlotUI1 = new CatalogueManager.SimpleDialogs.Automation.AutomationServiceSlotUI();
            ((System.ComponentModel.ISupportInitialize)(this.automationServiceSlots)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(12, 675);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(294, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateNew.Location = new System.Drawing.Point(12, 646);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(294, 23);
            this.btnCreateNew.TabIndex = 1;
            this.btnCreateNew.Text = "Create New";
            this.btnCreateNew.UseVisualStyleBackColor = true;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // automationServiceSlots
            // 
            this.automationServiceSlots.AllColumns.Add(this.olvColumn1);
            this.automationServiceSlots.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.automationServiceSlots.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.automationServiceSlots.FullRowSelect = true;
            this.automationServiceSlots.HideSelection = false;
            this.automationServiceSlots.Location = new System.Drawing.Point(12, 12);
            this.automationServiceSlots.Name = "automationServiceSlots";
            this.automationServiceSlots.Size = new System.Drawing.Size(294, 628);
            this.automationServiceSlots.TabIndex = 0;
            this.automationServiceSlots.UseCompatibleStateImageBehavior = false;
            this.automationServiceSlots.View = System.Windows.Forms.View.Details;
            this.automationServiceSlots.SelectedIndexChanged += new System.EventHandler(this.automationServiceSlots_SelectedIndexChanged);
            this.automationServiceSlots.KeyUp += new System.Windows.Forms.KeyEventHandler(this.automationServiceSlots_KeyUp);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "Automation Service Slots (you probably only want 1)";
            // 
            // automationServiceSlotUI1
            // 
            this.automationServiceSlotUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.automationServiceSlotUI1.AutomationServiceSlot = null;
            this.automationServiceSlotUI1.Location = new System.Drawing.Point(312, 12);
            this.automationServiceSlotUI1.Name = "automationServiceSlotUI1";
            this.automationServiceSlotUI1.Size = new System.Drawing.Size(1027, 686);
            this.automationServiceSlotUI1.TabIndex = 2;
            // 
            // AutomationServiceSlotManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1346, 705);
            this.Controls.Add(this.automationServiceSlotUI1);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCreateNew);
            this.Controls.Add(this.automationServiceSlots);
            this.Name = "AutomationServiceSlotManagement";
            this.Text = "AutomationServiceSlotManagement";
            ((System.ComponentModel.ISupportInitialize)(this.automationServiceSlots)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView automationServiceSlots;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Button btnDelete;
        private OLVColumn olvColumn1;
        private AutomationServiceSlotUI automationServiceSlotUI1;
    }
}