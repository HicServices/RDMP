using BrightIdeasSoftware;

namespace Rdmp.UI.SimpleDialogs
{
    partial class PropagateCatalogueItemChangesToSimilarNamedUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropagateCatalogueItemChangesToSimilarNamedUI));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.olvCatalogues = new BrightIdeasSoftware.ObjectListView();
            this.olvCatalogueItemName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCatalogueItemState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cbSelectAllCatalogues = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbSelectAllFields = new System.Windows.Forms.CheckBox();
            this.olvProperties = new BrightIdeasSoftware.ObjectListView();
            this.olvPropertyName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogues)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel1.Controls.Add(this.btnYes);
            this.splitContainer1.Panel1.Controls.Add(this.btnNo);
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1020, 782);
            this.splitContainer1.SplitterDistance = 385;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            this.splitContainer3.Panel1.Controls.Add(this.olvCatalogues);
            this.splitContainer3.Panel1.Controls.Add(this.cbSelectAllCatalogues);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.label2);
            this.splitContainer3.Panel2.Controls.Add(this.cbSelectAllFields);
            this.splitContainer3.Panel2.Controls.Add(this.olvProperties);
            this.splitContainer3.Size = new System.Drawing.Size(1014, 338);
            this.splitContainer3.SplitterDistance = 527;
            this.splitContainer3.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Catalogues To Propagate Changes Into:";
            // 
            // olvCatalogues
            // 
            this.olvCatalogues.AllColumns.Add(this.olvCatalogueItemName);
            this.olvCatalogues.AllColumns.Add(this.olvCatalogueItemState);
            this.olvCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCatalogues.CellEditUseWholeCell = false;
            this.olvCatalogues.CheckBoxes = true;
            this.olvCatalogues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvCatalogueItemName,
            this.olvCatalogueItemState});
            this.olvCatalogues.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvCatalogues.FullRowSelect = true;
            this.olvCatalogues.HideSelection = false;
            this.olvCatalogues.Location = new System.Drawing.Point(6, 16);
            this.olvCatalogues.Name = "olvCatalogues";
            this.olvCatalogues.RowHeight = 19;
            this.olvCatalogues.ShowGroups = false;
            this.olvCatalogues.Size = new System.Drawing.Size(518, 296);
            this.olvCatalogues.TabIndex = 2;
            this.olvCatalogues.UseCompatibleStateImageBehavior = false;
            this.olvCatalogues.View = System.Windows.Forms.View.Details;
            this.olvCatalogues.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            this.olvCatalogues.SelectedIndexChanged += new System.EventHandler(this.clbCatalogues_SelectedIndexChanged);
            // 
            // olvCatalogueItemName
            // 
            this.olvCatalogueItemName.Text = "Name";
            this.olvCatalogueItemName.MinimumWidth = 100;
            // 
            // olvCatalogueItemState
            // 
            this.olvCatalogueItemState.Text = "State";
            // 
            // cbSelectAllCatalogues
            // 
            this.cbSelectAllCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSelectAllCatalogues.AutoSize = true;
            this.cbSelectAllCatalogues.Location = new System.Drawing.Point(457, 318);
            this.cbSelectAllCatalogues.Name = "cbSelectAllCatalogues";
            this.cbSelectAllCatalogues.Size = new System.Drawing.Size(67, 17);
            this.cbSelectAllCatalogues.TabIndex = 6;
            this.cbSelectAllCatalogues.Text = "select all";
            this.cbSelectAllCatalogues.UseVisualStyleBackColor = true;
            this.cbSelectAllCatalogues.CheckedChanged += new System.EventHandler(this.cbSelectAllCatalogues_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Changed Fields To Propagate:";
            // 
            // cbSelectAllFields
            // 
            this.cbSelectAllFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectAllFields.AutoSize = true;
            this.cbSelectAllFields.Location = new System.Drawing.Point(3, 318);
            this.cbSelectAllFields.Name = "cbSelectAllFields";
            this.cbSelectAllFields.Size = new System.Drawing.Size(67, 17);
            this.cbSelectAllFields.TabIndex = 7;
            this.cbSelectAllFields.Text = "select all";
            this.cbSelectAllFields.UseVisualStyleBackColor = true;
            this.cbSelectAllFields.CheckedChanged += new System.EventHandler(this.cbSelectAllFields_CheckedChanged);
            // 
            // olvProperties
            // 
            this.olvProperties.AllColumns.Add(this.olvPropertyName);
            this.olvProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvProperties.CellEditUseWholeCell = false;
            this.olvProperties.CheckBoxes = true;
            this.olvProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvPropertyName});
            this.olvProperties.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvProperties.FullRowSelect = true;
            this.olvProperties.HideSelection = false;
            this.olvProperties.Location = new System.Drawing.Point(3, 16);
            this.olvProperties.Name = "olvProperties";
            this.olvProperties.RowHeight = 19;
            this.olvProperties.ShowGroups = false;
            this.olvProperties.Size = new System.Drawing.Size(477, 296);
            this.olvProperties.TabIndex = 3;
            this.olvProperties.UseCompatibleStateImageBehavior = false;
            this.olvProperties.View = System.Windows.Forms.View.Details;
            this.olvProperties.ItemActivate += new System.EventHandler(this.olv_ItemActivate);
            this.olvProperties.SelectedIndexChanged += new System.EventHandler(this.clbChangedProperties_SelectedIndexChanged);
            // 
            // olvPropertyName
            // 
            this.olvPropertyName.AspectName = "Name";
            this.olvPropertyName.Text = "Property";
            this.olvPropertyName.MinimumWidth = 100;
            // 
            // btnYes
            // 
            this.btnYes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnYes.Image = ((System.Drawing.Image)(resources.GetObject("btnYes.Image")));
            this.btnYes.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnYes.Location = new System.Drawing.Point(155, 347);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(262, 36);
            this.btnYes.TabIndex = 1;
            this.btnYes.Text = " Yes - Copy over changes";
            this.btnYes.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnNo.Image = ((System.Drawing.Image)(resources.GetObject("btnNo.Image")));
            this.btnNo.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnNo.Location = new System.Drawing.Point(423, 347);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(262, 36);
            this.btnNo.TabIndex = 0;
            this.btnNo.Text = " No - Save only this one";
            this.btnNo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.Location = new System.Drawing.Point(691, 347);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(262, 36);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel (Dont save anything)";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Size = new System.Drawing.Size(1020, 393);
            this.splitContainer2.SplitterDistance = 535;
            this.splitContainer2.TabIndex = 0;
            // 
            // PropagateCatalogueItemChangesToSimilarNamedUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 782);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PropagateCatalogueItemChangesToSimilarNamedUI";
            this.Text = "Propagate Changes To Other Catalogues?";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogues)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.CheckBox cbSelectAllFields;
        private System.Windows.Forms.CheckBox cbSelectAllCatalogues;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private ObjectListView olvProperties;
        private ObjectListView olvCatalogues;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnCancel;
        private OLVColumn olvCatalogueItemName;
        private OLVColumn olvCatalogueItemState;
        private OLVColumn olvPropertyName;
        private System.Windows.Forms.SplitContainer splitContainer3;
    }
}