using BrightIdeasSoftware;

namespace Rdmp.UI.ExtractionUIs
{
    partial class ReOrderCatalogueItemsUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReOrderCatalogueItemsUI));
            this.olvExtractionInformations = new BrightIdeasSoftware.ObjectListView();
            this.olvColumns = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.lbDesiredOrder = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbNewOrder = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnSaveNewOrder = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnClear = new System.Windows.Forms.Button();
            this.rbSimple = new System.Windows.Forms.RadioButton();
            this.rbAdvanced = new System.Windows.Forms.RadioButton();
            this.helpIcon1 = new Rdmp.UI.SimpleControls.HelpIcon();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvExtractionInformations
            // 
            this.olvExtractionInformations.AllColumns.Add(this.olvColumns);
            this.olvExtractionInformations.AllColumns.Add(this.olvOrder);
            this.olvExtractionInformations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvExtractionInformations.CellEditUseWholeCell = false;
            this.olvExtractionInformations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumns,
            this.olvOrder});
            this.olvExtractionInformations.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvExtractionInformations.FullRowSelect = true;
            this.olvExtractionInformations.HideSelection = false;
            this.olvExtractionInformations.IsSimpleDragSource = true;
            this.olvExtractionInformations.IsSimpleDropSink = true;
            this.olvExtractionInformations.Location = new System.Drawing.Point(0, 19);
            this.olvExtractionInformations.Name = "olvExtractionInformations";
            this.olvExtractionInformations.Size = new System.Drawing.Size(355, 685);
            this.olvExtractionInformations.TabIndex = 0;
            this.olvExtractionInformations.UseCompatibleStateImageBehavior = false;
            this.olvExtractionInformations.View = System.Windows.Forms.View.Details;
            this.olvExtractionInformations.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvExtractionInformations_ModelCanDrop);
            this.olvExtractionInformations.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.olvExtractionInformations_ModelDropped);
            this.olvExtractionInformations.ItemActivate += new System.EventHandler(this.olvExtractionInformations_ItemActivate);
            // 
            // olvColumns
            // 
            this.olvColumns.AspectName = "ToString";
            this.olvColumns.Groupable = false;
            this.olvColumns.MinimumWidth = 100;
            this.olvColumns.Sortable = false;
            this.olvColumns.Text = "Columns";
            this.olvColumns.Width = 100;
            // 
            // olvOrder
            // 
            this.olvOrder.AspectName = "Order";
            this.olvOrder.Groupable = false;
            this.olvOrder.Sortable = false;
            this.olvOrder.Text = "Order";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Current Order (Drag and Drop To Reorder)";
            // 
            // lbDesiredOrder
            // 
            this.lbDesiredOrder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbDesiredOrder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbDesiredOrder.FormattingEnabled = true;
            this.lbDesiredOrder.Location = new System.Drawing.Point(7, 19);
            this.lbDesiredOrder.Name = "lbDesiredOrder";
            this.lbDesiredOrder.Size = new System.Drawing.Size(316, 641);
            this.lbDesiredOrder.TabIndex = 2;
            this.lbDesiredOrder.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbDesiredOrder_DrawItem);
            this.lbDesiredOrder.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbDesiredOrder_KeyUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Desired Order (Ctrl + V)";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BackColor = System.Drawing.Color.Lime;
            this.label3.Location = new System.Drawing.Point(332, 634);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 15);
            this.label3.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(355, 634);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Start of reorder";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(355, 652);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Items being moved";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.BackColor = System.Drawing.Color.Purple;
            this.label6.Location = new System.Drawing.Point(332, 652);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 15);
            this.label6.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(324, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "New Order";
            // 
            // lbNewOrder
            // 
            this.lbNewOrder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbNewOrder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbNewOrder.FormattingEnabled = true;
            this.lbNewOrder.Location = new System.Drawing.Point(327, 19);
            this.lbNewOrder.Name = "lbNewOrder";
            this.lbNewOrder.Size = new System.Drawing.Size(288, 563);
            this.lbNewOrder.TabIndex = 8;
            this.lbNewOrder.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbCurrentOrder_DrawItem);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.Location = new System.Drawing.Point(355, 670);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(262, 32);
            this.label8.TabIndex = 11;
            this.label8.Text = "Not found in desired order (will retain position relative to starting item)";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.BackColor = System.Drawing.SystemColors.Window;
            this.label9.Location = new System.Drawing.Point(332, 671);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(16, 15);
            this.label9.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 682);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Not Found";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.BackColor = System.Drawing.Color.Red;
            this.label11.Location = new System.Drawing.Point(5, 682);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(16, 15);
            this.label11.TabIndex = 12;
            // 
            // btnSaveNewOrder
            // 
            this.btnSaveNewOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveNewOrder.Location = new System.Drawing.Point(327, 608);
            this.btnSaveNewOrder.Name = "btnSaveNewOrder";
            this.btnSaveNewOrder.Size = new System.Drawing.Size(114, 23);
            this.btnSaveNewOrder.TabIndex = 14;
            this.btnSaveNewOrder.Text = "Apply New Order";
            this.btnSaveNewOrder.UseVisualStyleBackColor = true;
            this.btnSaveNewOrder.Click += new System.EventHandler(this.btnSaveNewOrder_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 30);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.olvExtractionInformations);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnClear);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.lbDesiredOrder);
            this.splitContainer1.Panel2.Controls.Add(this.btnSaveNewOrder);
            this.splitContainer1.Panel2.Controls.Add(this.lbNewOrder);
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.label11);
            this.splitContainer1.Panel2.Controls.Add(this.label9);
            this.splitContainer1.Panel2.Controls.Add(this.label10);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(984, 704);
            this.splitContainer1.SplitterDistance = 358;
            this.splitContainer1.TabIndex = 17;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(447, 608);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(114, 23);
            this.btnClear.TabIndex = 15;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // rbSimple
            // 
            this.rbSimple.AutoSize = true;
            this.rbSimple.Checked = true;
            this.rbSimple.Location = new System.Drawing.Point(3, 3);
            this.rbSimple.Name = "rbSimple";
            this.rbSimple.Size = new System.Drawing.Size(56, 17);
            this.rbSimple.TabIndex = 18;
            this.rbSimple.TabStop = true;
            this.rbSimple.Text = "Simple";
            this.rbSimple.UseVisualStyleBackColor = true;
            this.rbSimple.CheckedChanged += new System.EventHandler(this.rbSimple_CheckedChanged);
            // 
            // rbAdvanced
            // 
            this.rbAdvanced.AutoSize = true;
            this.rbAdvanced.Location = new System.Drawing.Point(65, 3);
            this.rbAdvanced.Name = "rbAdvanced";
            this.rbAdvanced.Size = new System.Drawing.Size(74, 17);
            this.rbAdvanced.TabIndex = 18;
            this.rbAdvanced.Text = "Advanced";
            this.rbAdvanced.UseVisualStyleBackColor = true;
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(145, 3);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.SuppressClick = false;
            this.helpIcon1.TabIndex = 19;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.rbSimple);
            this.flowLayoutPanel1.Controls.Add(this.rbAdvanced);
            this.flowLayoutPanel1.Controls.Add(this.helpIcon1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(984, 30);
            this.flowLayoutPanel1.TabIndex = 20;
            // 
            // ReOrderCatalogueItemsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 734);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "ReOrderCatalogueItemsUI";
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionInformations)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView olvExtractionInformations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lbDesiredOrder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox lbNewOrder;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnSaveNewOrder;
        private OLVColumn olvColumns;
        private OLVColumn olvOrder;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RadioButton rbSimple;
        private System.Windows.Forms.RadioButton rbAdvanced;
        private System.Windows.Forms.Button btnClear;
        private SimpleControls.HelpIcon helpIcon1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}