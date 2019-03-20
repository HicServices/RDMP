namespace CatalogueManager.SimpleDialogs
{
    partial class PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItemsUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cbSelectAllFields = new System.Windows.Forms.CheckBox();
            this.cbSelectAllCatalogues = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.clbChangedProperties = new System.Windows.Forms.CheckedListBox();
            this.clbCatalogues = new System.Windows.Forms.CheckedListBox();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.cbSelectAllFields);
            this.splitContainer1.Panel1.Controls.Add(this.cbSelectAllCatalogues);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.clbChangedProperties);
            this.splitContainer1.Panel1.Controls.Add(this.clbCatalogues);
            this.splitContainer1.Panel1.Controls.Add(this.btnYes);
            this.splitContainer1.Panel1.Controls.Add(this.btnNo);
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1150, 782);
            this.splitContainer1.SplitterDistance = 375;
            this.splitContainer1.TabIndex = 0;
            // 
            // cbSelectAllFields
            // 
            this.cbSelectAllFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectAllFields.AutoSize = true;
            this.cbSelectAllFields.Location = new System.Drawing.Point(515, 314);
            this.cbSelectAllFields.Name = "cbSelectAllFields";
            this.cbSelectAllFields.Size = new System.Drawing.Size(67, 17);
            this.cbSelectAllFields.TabIndex = 7;
            this.cbSelectAllFields.Text = "select all";
            this.cbSelectAllFields.UseVisualStyleBackColor = true;
            this.cbSelectAllFields.CheckedChanged += new System.EventHandler(this.cbSelectAllFields_CheckedChanged);
            // 
            // cbSelectAllCatalogues
            // 
            this.cbSelectAllCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectAllCatalogues.AutoSize = true;
            this.cbSelectAllCatalogues.Location = new System.Drawing.Point(399, 314);
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
            this.label2.Location = new System.Drawing.Point(512, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Changed Fields To Propagate:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(112, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Catalogues To Propagate Changes Into:";
            // 
            // clbChangedProperties
            // 
            this.clbChangedProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clbChangedProperties.FormattingEnabled = true;
            this.clbChangedProperties.Location = new System.Drawing.Point(515, 53);
            this.clbChangedProperties.Name = "clbChangedProperties";
            this.clbChangedProperties.Size = new System.Drawing.Size(392, 259);
            this.clbChangedProperties.TabIndex = 3;
            this.clbChangedProperties.SelectedIndexChanged += new System.EventHandler(this.clbChangedProperties_SelectedIndexChanged);
            // 
            // clbCatalogues
            // 
            this.clbCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clbCatalogues.FormattingEnabled = true;
            this.clbCatalogues.Location = new System.Drawing.Point(115, 53);
            this.clbCatalogues.Name = "clbCatalogues";
            this.clbCatalogues.Size = new System.Drawing.Size(351, 259);
            this.clbCatalogues.TabIndex = 2;
            this.clbCatalogues.SelectedIndexChanged += new System.EventHandler(this.clbCatalogues_SelectedIndexChanged);
            // 
            // btnYes
            // 
            this.btnYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnYes.Location = new System.Drawing.Point(121, 336);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(262, 36);
            this.btnYes.TabIndex = 0;
            this.btnYes.Text = "Yes (copy over changes)";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNo.Location = new System.Drawing.Point(389, 337);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(262, 36);
            this.btnNo.TabIndex = 1;
            this.btnNo.Text = "No (save only this one)";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(657, 336);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(262, 36);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel (I\'ve changed my mind, dont save anything)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Size = new System.Drawing.Size(1150, 403);
            this.splitContainer2.SplitterDistance = 565;
            this.splitContainer2.TabIndex = 0;
            // 
            // PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 782);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems";
            this.Text = "Propagate Changes To Other Catalogues?";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
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
        private System.Windows.Forms.CheckedListBox clbChangedProperties;
        private System.Windows.Forms.CheckedListBox clbCatalogues;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnCancel;
    }
}