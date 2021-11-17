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
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvNamespace = new BrightIdeasSoftware.OLVColumn();
            this.olvRole = new BrightIdeasSoftware.OLVColumn();
            this.olvCompatible = new BrightIdeasSoftware.OLVColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.diagramPanel = new System.Windows.Forms.Panel();
            this.btnReRunChecks = new System.Windows.Forms.Button();
            this.gbArguments = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.diagramPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.olvComponents);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(1312, 622);
            this.splitContainer1.SplitterDistance = 436;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.ForeColor = System.Drawing.Color.Brown;
            this.label3.Location = new System.Drawing.Point(0, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(432, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "(Add Components from below with Drag and Drop)";
            // 
            // olvComponents
            // 
            this.olvComponents.AllColumns.Add(this.olvName);
            this.olvComponents.AllColumns.Add(this.olvNamespace);
            this.olvComponents.AllColumns.Add(this.olvRole);
            this.olvComponents.AllColumns.Add(this.olvCompatible);
            this.olvComponents.CellEditUseWholeCell = false;
            this.olvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvNamespace,
            this.olvRole,
            this.olvCompatible});
            this.olvComponents.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvComponents.HideSelection = false;
            this.olvComponents.IsSimpleDragSource = true;
            this.olvComponents.Location = new System.Drawing.Point(0, 36);
            this.olvComponents.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvComponents.Name = "olvComponents";
            this.olvComponents.Size = new System.Drawing.Size(432, 582);
            this.olvComponents.TabIndex = 1;
            this.olvComponents.UseCompatibleStateImageBehavior = false;
            this.olvComponents.View = System.Windows.Forms.View.Details;
            this.olvComponents.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.olvComponents_CellRightClick);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Groupable = false;
            this.olvName.MinimumWidth = 100;
            this.olvName.Text = "Name";
            this.olvName.Width = 100;
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
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(432, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Components";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 18);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
            this.splitContainer2.Size = new System.Drawing.Size(871, 604);
            this.splitContainer2.SplitterDistance = 302;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 7;
            // 
            // diagramPanel
            // 
            this.diagramPanel.AutoSize = true;
            this.diagramPanel.Controls.Add(this.btnReRunChecks);
            this.diagramPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagramPanel.Location = new System.Drawing.Point(0, 0);
            this.diagramPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.diagramPanel.MinimumSize = new System.Drawing.Size(0, 127);
            this.diagramPanel.Name = "diagramPanel";
            this.diagramPanel.Size = new System.Drawing.Size(867, 298);
            this.diagramPanel.TabIndex = 0;
            this.diagramPanel.Text = "pipelineDiagram";
            // 
            // btnReRunChecks
            // 
            this.btnReRunChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReRunChecks.Location = new System.Drawing.Point(746, 268);
            this.btnReRunChecks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnReRunChecks.Name = "btnReRunChecks";
            this.btnReRunChecks.Size = new System.Drawing.Size(118, 27);
            this.btnReRunChecks.TabIndex = 0;
            this.btnReRunChecks.Text = "Re-Run Checks";
            this.btnReRunChecks.UseVisualStyleBackColor = true;
            this.btnReRunChecks.Click += new System.EventHandler(this.btnReRunChecks_Click);
            // 
            // gbArguments
            // 
            this.gbArguments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbArguments.Location = new System.Drawing.Point(0, 0);
            this.gbArguments.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbArguments.Name = "gbArguments";
            this.gbArguments.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbArguments.Size = new System.Drawing.Size(867, 293);
            this.gbArguments.TabIndex = 5;
            this.gbArguments.TabStop = false;
            this.gbArguments.Text = "Arguments:";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(871, 18);
            this.label2.TabIndex = 6;
            this.label2.Text = "Current Pipeline Configuration";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // PipelineWorkAreaUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "PipelineWorkAreaUI";
            this.Size = new System.Drawing.Size(1312, 622);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.diagramPanel.ResumeLayout(false);
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
