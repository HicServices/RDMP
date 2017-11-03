using BrightIdeasSoftware;

namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs
{
    partial class ParameterCollectionUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParameterCollectionUI));
            this.btnAddParameter = new System.Windows.Forms.Button();
            this.olvParameters = new BrightIdeasSoftware.ObjectListView();
            this.olvParameterName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvParameterSQL = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvComment = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOwner = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnOverride = new System.Windows.Forms.Button();
            this.lblUseCase = new System.Windows.Forms.Label();
            this.parameterEditorScintillaControl1 = new CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.ParameterEditorScintillaControl();
            ((System.ComponentModel.ISupportInitialize)(this.olvParameters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAddParameter
            // 
            this.btnAddParameter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddParameter.Location = new System.Drawing.Point(583, 9);
            this.btnAddParameter.Name = "btnAddParameter";
            this.btnAddParameter.Size = new System.Drawing.Size(129, 23);
            this.btnAddParameter.TabIndex = 10;
            this.btnAddParameter.Text = "Add Parameter";
            this.btnAddParameter.UseVisualStyleBackColor = true;
            this.btnAddParameter.Click += new System.EventHandler(this.btnAddParameter_Click);
            // 
            // olvParameters
            // 
            this.olvParameters.AllColumns.Add(this.olvParameterName);
            this.olvParameters.AllColumns.Add(this.olvParameterSQL);
            this.olvParameters.AllColumns.Add(this.olvValue);
            this.olvParameters.AllColumns.Add(this.olvComment);
            this.olvParameters.AllColumns.Add(this.olvOwner);
            this.olvParameters.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.olvParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvParameters.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
            this.olvParameters.CellEditUseWholeCell = false;
            this.olvParameters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvParameterName,
            this.olvParameterSQL,
            this.olvValue,
            this.olvComment,
            this.olvOwner});
            this.olvParameters.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvParameters.FullRowSelect = true;
            this.olvParameters.GroupImageList = this.imageList1;
            this.olvParameters.HideSelection = false;
            this.olvParameters.Location = new System.Drawing.Point(3, 38);
            this.olvParameters.Name = "olvParameters";
            this.olvParameters.Size = new System.Drawing.Size(838, 477);
            this.olvParameters.SmallImageList = this.imageList1;
            this.olvParameters.TabIndex = 11;
            this.olvParameters.UseCompatibleStateImageBehavior = false;
            this.olvParameters.View = System.Windows.Forms.View.Details;
            this.olvParameters.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.olvParameters_CellEditFinishing);
            this.olvParameters.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(this.olvParameters_CellEditStarting);
            this.olvParameters.SelectedIndexChanged += new System.EventHandler(this.olvParameters_SelectedIndexChanged);
            this.olvParameters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.olvParameters_KeyDown);
            // 
            // olvParameterName
            // 
            this.olvParameterName.AspectName = "ParameterName";
            this.olvParameterName.IsEditable = false;
            this.olvParameterName.Text = "ParameterName";
            this.olvParameterName.Width = 150;
            // 
            // olvParameterSQL
            // 
            this.olvParameterSQL.AspectName = "ParameterSQL";
            this.olvParameterSQL.CellEditUseWholeCell = true;
            this.olvParameterSQL.Groupable = false;
            this.olvParameterSQL.Text = "ParameterSQL";
            this.olvParameterSQL.Width = 200;
            // 
            // olvValue
            // 
            this.olvValue.AspectName = "Value";
            this.olvValue.CellEditUseWholeCell = true;
            this.olvValue.Groupable = false;
            this.olvValue.Text = "Value";
            this.olvValue.Width = 100;
            // 
            // olvComment
            // 
            this.olvComment.AspectName = "Comment";
            this.olvComment.CellEditUseWholeCell = true;
            this.olvComment.Groupable = false;
            this.olvComment.Text = "Comment";
            this.olvComment.Width = 200;
            // 
            // olvOwner
            // 
            this.olvOwner.IsEditable = false;
            this.olvOwner.Text = "Owner";
            this.olvOwner.Width = 200;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Locked.png");
            this.imageList1.Images.SetKeyName(1, "Overridden.png");
            this.imageList1.Images.SetKeyName(2, "Warning.png");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(0, 86);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.parameterEditorScintillaControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnOverride);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddParameter);
            this.splitContainer1.Panel2.Controls.Add(this.olvParameters);
            this.splitContainer1.Size = new System.Drawing.Size(1271, 524);
            this.splitContainer1.SplitterDistance = 421;
            this.splitContainer1.TabIndex = 13;
            // 
            // btnOverride
            // 
            this.btnOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOverride.Enabled = false;
            this.btnOverride.Location = new System.Drawing.Point(718, 9);
            this.btnOverride.Name = "btnOverride";
            this.btnOverride.Size = new System.Drawing.Size(123, 23);
            this.btnOverride.TabIndex = 10;
            this.btnOverride.Text = "Override Selected";
            this.btnOverride.UseVisualStyleBackColor = true;
            this.btnOverride.Click += new System.EventHandler(this.btnOverride_Click);
            // 
            // lblUseCase
            // 
            this.lblUseCase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUseCase.Location = new System.Drawing.Point(3, 9);
            this.lblUseCase.Name = "lblUseCase";
            this.lblUseCase.Size = new System.Drawing.Size(1268, 74);
            this.lblUseCase.TabIndex = 14;
            this.lblUseCase.Text = "Use Case:";
            // 
            // parameterEditorScintillaControl1
            // 
            this.parameterEditorScintillaControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterEditorScintillaControl1.Location = new System.Drawing.Point(0, 0);
            this.parameterEditorScintillaControl1.Name = "parameterEditorScintillaControl1";
            this.parameterEditorScintillaControl1.Options = null;
            this.parameterEditorScintillaControl1.Size = new System.Drawing.Size(419, 522);
            this.parameterEditorScintillaControl1.TabIndex = 0;
            // 
            // ParameterCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblUseCase);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ParameterCollectionUI";
            this.Size = new System.Drawing.Size(1271, 610);
            ((System.ComponentModel.ISupportInitialize)(this.olvParameters)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddParameter;
        private ObjectListView olvParameters;
        private OLVColumn olvParameterName;
        private OLVColumn olvParameterSQL;
        private OLVColumn olvValue;
        private OLVColumn olvComment;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ParameterEditorScintillaControl parameterEditorScintillaControl1;
        private OLVColumn olvOwner;
        private System.Windows.Forms.Label lblUseCase;
        private System.Windows.Forms.Button btnOverride;
    }
}
