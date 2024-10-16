using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    partial class ConfigurationsCollectionUI
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
            this.components = new System.ComponentModel.Container();
            this.tbFilter = new TextBox();
            this.tlvConfigurations = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfigurations)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvDatasets
            // 
            this.tlvConfigurations.AllColumns.Add(this.olvName);
            this.tlvConfigurations.CellEditUseWholeCell = false;
            this.tlvConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvConfigurations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvConfigurations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvConfigurations.FullRowSelect = true;
            this.tlvConfigurations.HideSelection = false;
            this.tlvConfigurations.Location = new System.Drawing.Point(0, 0);
            this.tlvConfigurations.ShowGroups = false;
            this.tlvConfigurations.Size = new System.Drawing.Size(322, 557);
            this.tlvConfigurations.TabIndex = 2;
            this.tlvConfigurations.UseCompatibleStateImageBehavior = false;
            this.tlvConfigurations.View = System.Windows.Forms.View.Details;
            this.tlvConfigurations.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
            // 
            // DatasetsCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvConfigurations);
            this.Name = "ConfigurationsCollectionUI";
            this.Size = new System.Drawing.Size(322, 557);
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfigurations)).EndInit();
            this.ResumeLayout(false);
            this.Text = "ConfigurationsCollection";
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvConfigurations;
        private BrightIdeasSoftware.OLVColumn olvName;
        private TextBox tbFilter;
    }
}