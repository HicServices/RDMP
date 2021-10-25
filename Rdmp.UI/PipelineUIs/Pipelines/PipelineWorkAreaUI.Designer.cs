using System.Windows.Forms;
using BrightIdeasSoftware;

namespace Rdmp.UI.PipelineUIs.Pipelines
{
    partial class PipelineWorkAreaUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.olvComponents = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNamespace = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvRole = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCompatible = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gbArguments = new System.Windows.Forms.GroupBox();
            this.diagramPanel = new System.Windows.Forms.Panel();
            this.btnReRunChecks = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).BeginInit();
            this.diagramPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.olvComponents);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(1125, 539);
            this.splitContainer1.SplitterDistance = 374;
            this.splitContainer1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label3.ForeColor = System.Drawing.Color.Brown;
            this.label3.Location = new System.Drawing.Point(66, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(301, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "(Add Components from below with Drag and Drop)";
            // 
            // olvComponents
            // 
            this.olvComponents.AllColumns.Add(this.olvName);
            this.olvComponents.AllColumns.Add(this.olvNamespace);
            this.olvComponents.AllColumns.Add(this.olvRole);
            this.olvComponents.AllColumns.Add(this.olvCompatible);
            this.olvComponents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvComponents.CellEditUseWholeCell = false;
            this.olvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvNamespace,
            this.olvRole,
            this.olvCompatible});
            this.olvComponents.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvComponents.IsSimpleDragSource = true;
            this.olvComponents.Location = new System.Drawing.Point(3, 24);
            this.olvComponents.Name = "olvComponents";
            this.olvComponents.Size = new System.Drawing.Size(364, 508);
            this.olvComponents.TabIndex = 1;
            this.olvComponents.UseCompatibleStateImageBehavior = false;
            this.olvComponents.View = System.Windows.Forms.View.Details;
            this.olvComponents.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.olvComponents_CellRightClick);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
            // 
            // olvNamespace
            // 
            this.olvNamespace.AspectName = "Namespace";
            this.olvNamespace.Groupable = false;
            this.olvNamespace.Text = "Namespace";
            // 
            // olvRole
            // 
            this.olvRole.AspectName = "GetRole";
            this.olvRole.Text = "Role";
            this.olvRole.Width = 80;
            // 
            // olvCompatible
            // 
            this.olvCompatible.AspectName = "UIDescribeCompatible";
            this.olvCompatible.Groupable = false;
            this.olvCompatible.Text = "Compatible?";
            this.olvCompatible.Width = 70;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Components";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(737, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Current Pipeline Configuration";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // gbArguments
            // 
            this.gbArguments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbArguments.Location = new System.Drawing.Point(0, 0);
            this.gbArguments.Name = "gbArguments";
            this.gbArguments.Size = new System.Drawing.Size(733, 246);
            this.gbArguments.TabIndex = 5;
            this.gbArguments.TabStop = false;
            this.gbArguments.Text = "Arguments:";
            // 
            // diagramPanel
            // 
            this.diagramPanel.AutoSize = true;
            this.diagramPanel.Controls.Add(this.btnReRunChecks);
            this.diagramPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagramPanel.Location = new System.Drawing.Point(0, 0);
            this.diagramPanel.MinimumSize = new System.Drawing.Size(0, 110);
            this.diagramPanel.Name = "diagramPanel";
            this.diagramPanel.Size = new System.Drawing.Size(733, 250);
            this.diagramPanel.TabIndex = 0;
            this.diagramPanel.Text = "pipelineDiagram";
            // 
            // btnReRunChecks
            // 
            this.btnReRunChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReRunChecks.Location = new System.Drawing.Point(629, 224);
            this.btnReRunChecks.Name = "btnReRunChecks";
            this.btnReRunChecks.Size = new System.Drawing.Size(101, 23);
            this.btnReRunChecks.TabIndex = 0;
            this.btnReRunChecks.Text = "Re-Run Checks";
            this.btnReRunChecks.UseVisualStyleBackColor = true;
            this.btnReRunChecks.Click += new System.EventHandler(this.btnReRunChecks_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Location = new System.Drawing.Point(3, 24);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.diagramPanel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gbArguments);
            this.splitContainer2.Size = new System.Drawing.Size(737, 508);
            this.splitContainer2.SplitterDistance = 254;
            this.splitContainer2.TabIndex = 7;
            // 
            // PipelineWorkArea
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "PipelineWorkArea";
            this.Size = new System.Drawing.Size(1125, 539);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).EndInit();
            this.diagramPanel.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private ObjectListView olvComponents;
        private Panel diagramPanel;
        private GroupBox gbArguments;
        private OLVColumn olvName;
        private OLVColumn olvCompatible;
        private OLVColumn olvRole;
        private OLVColumn olvNamespace;
        private Label label2;
        private Button btnReRunChecks;
        private Label label3;
        private SplitContainer splitContainer2;
    }
}
