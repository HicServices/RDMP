using BrightIdeasSoftware;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs
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
            this.olvParameters = new BrightIdeasSoftware.ObjectListView();
            this.olvParameterName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvParameterSQL = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvComment = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOwner = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpDesign = new System.Windows.Forms.TabPage();
            this.tpSql = new System.Windows.Forms.TabPage();
            this.hiParameters = new HelpIcon();
            this.parameterEditorScintillaControl1 = new ParameterEditorScintillaControlUI();
            ((System.ComponentModel.ISupportInitialize)(this.olvParameters)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpDesign.SuspendLayout();
            this.tpSql.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvParameters
            // 
            this.olvParameters.AllColumns.Add(this.olvParameterName);
            this.olvParameters.AllColumns.Add(this.olvParameterSQL);
            this.olvParameters.AllColumns.Add(this.olvValue);
            this.olvParameters.AllColumns.Add(this.olvComment);
            this.olvParameters.AllColumns.Add(this.olvOwner);
            this.olvParameters.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.olvParameters.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
            this.olvParameters.CellEditUseWholeCell = false;
            this.olvParameters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvParameterName,
            this.olvParameterSQL,
            this.olvValue,
            this.olvComment,
            this.olvOwner});
            this.olvParameters.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvParameters.FullRowSelect = true;
            this.olvParameters.GroupImageList = this.imageList1;
            this.olvParameters.HideSelection = false;
            this.olvParameters.Location = new System.Drawing.Point(3, 3);
            this.olvParameters.Name = "olvParameters";
            this.olvParameters.Size = new System.Drawing.Size(1236, 572);
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
            this.olvParameterName.AspectName = "";
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
            this.olvValue.Width = 238;
            this.olvValue.MinimumWidth = 100;
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpDesign);
            this.tabControl1.Controls.Add(this.tpSql);
            this.tabControl1.Location = new System.Drawing.Point(1, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1250, 604);
            this.tabControl1.TabIndex = 1;
            // 
            // tpDesign
            // 
            this.tpDesign.Controls.Add(this.olvParameters);
            this.tpDesign.Location = new System.Drawing.Point(4, 22);
            this.tpDesign.Name = "tpDesign";
            this.tpDesign.Padding = new System.Windows.Forms.Padding(3);
            this.tpDesign.Size = new System.Drawing.Size(1242, 578);
            this.tpDesign.TabIndex = 1;
            this.tpDesign.Text = "Designer";
            this.tpDesign.UseVisualStyleBackColor = true;
            // 
            // tpSql
            // 
            this.tpSql.Controls.Add(this.parameterEditorScintillaControl1);
            this.tpSql.Location = new System.Drawing.Point(4, 22);
            this.tpSql.Name = "tpSql";
            this.tpSql.Padding = new System.Windows.Forms.Padding(3);
            this.tpSql.Size = new System.Drawing.Size(1242, 578);
            this.tpSql.TabIndex = 0;
            this.tpSql.Text = "SQL";
            this.tpSql.UseVisualStyleBackColor = true;
            // 
            // hiParameters
            // 
            this.hiParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hiParameters.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiParameters.BackgroundImage")));
            this.hiParameters.Location = new System.Drawing.Point(1251, 23);
            this.hiParameters.Name = "hiParameters";
            this.hiParameters.Size = new System.Drawing.Size(19, 19);
            this.hiParameters.TabIndex = 2;
            // 
            // parameterEditorScintillaControl1
            // 
            this.parameterEditorScintillaControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterEditorScintillaControl1.Location = new System.Drawing.Point(3, 3);
            this.parameterEditorScintillaControl1.Name = "parameterEditorScintillaControl1";
            this.parameterEditorScintillaControl1.Options = null;
            this.parameterEditorScintillaControl1.Size = new System.Drawing.Size(1236, 572);
            this.parameterEditorScintillaControl1.TabIndex = 0;
            // 
            // ParameterCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hiParameters);
            this.Controls.Add(this.tabControl1);
            this.Name = "ParameterCollectionUI";
            this.Size = new System.Drawing.Size(1271, 610);
            ((System.ComponentModel.ISupportInitialize)(this.olvParameters)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpDesign.ResumeLayout(false);
            this.tpSql.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView olvParameters;
        private OLVColumn olvParameterName;
        private OLVColumn olvParameterSQL;
        private OLVColumn olvValue;
        private OLVColumn olvComment;
        private System.Windows.Forms.ImageList imageList1;
        private ParameterEditorScintillaControlUI parameterEditorScintillaControl1;
        private OLVColumn olvOwner;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpSql;
        private System.Windows.Forms.TabPage tpDesign;
        private HelpIcon hiParameters;
    }
}
