using BrightIdeasSoftware;

namespace Rdmp.UI.PluginManagement
{
    partial class PluginDependencyVisualisationUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.olvcName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.treeView1 = new BrightIdeasSoftware.TreeListView();
            ((System.ComponentModel.ISupportInitialize)(this.treeView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dependencies:";
            // 
            // olvcName
            // 
            this.olvcName.AspectName = "ToString";
            this.olvcName.FillsFreeSpace = true;
            this.olvcName.Groupable = false;
            this.olvcName.Text = "Name";
            // 
            // treeView1
            // 
            this.treeView1.AllColumns.Add(this.olvcName);
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvcName});
            this.treeView1.Location = new System.Drawing.Point(3, 20);
            this.treeView1.Name = "treeView1";
            this.treeView1.OwnerDraw = true;
            this.treeView1.ShowGroups = false;
            this.treeView1.Size = new System.Drawing.Size(901, 583);
            this.treeView1.TabIndex = 0;
            this.treeView1.UseCompatibleStateImageBehavior = false;
            this.treeView1.View = System.Windows.Forms.View.Details;
            this.treeView1.VirtualMode = true;
            this.treeView1.ItemActivate += new System.EventHandler(this.treeView1_ItemActivate);
            // 
            // PluginDependencyVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeView1);
            this.Name = "PluginDependencyVisualisation";
            this.Size = new System.Drawing.Size(907, 606);
            ((System.ComponentModel.ISupportInitialize)(this.treeView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private OLVColumn olvcName;
        private TreeListView treeView1;

    }
}
