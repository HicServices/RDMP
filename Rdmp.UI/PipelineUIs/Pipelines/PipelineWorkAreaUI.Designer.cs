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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbSearchComponents = new System.Windows.Forms.TextBox();
            this.cbShowIncompatible = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.olvComponents = new BrightIdeasSoftware.ObjectListView();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvNamespace = new BrightIdeasSoftware.OLVColumn();
            this.olvRole = new BrightIdeasSoftware.OLVColumn();
            this.olvCompatible = new BrightIdeasSoftware.OLVColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.diagramPanel = new System.Windows.Forms.Panel();
            this.btnReRunChecks = new System.Windows.Forms.Button();
            this.gbArguments = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
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
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.olvComponents);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer1.Size = new System.Drawing.Size(1312, 622);
            this.splitContainer1.SplitterDistance = 436;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbSearchComponents);
            this.panel1.Controls.Add(this.cbShowIncompatible);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 596);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(436, 26);
            this.panel1.TabIndex = 3;
            // 
            // tbSearchComponents
            // 
            this.tbSearchComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSearchComponents.Location = new System.Drawing.Point(45, 0);
            this.tbSearchComponents.Name = "tbSearchComponents";
            this.tbSearchComponents.Size = new System.Drawing.Size(260, 23);
            this.tbSearchComponents.TabIndex = 1;
            this.tbSearchComponents.TextChanged += new System.EventHandler(this.tbSearchComponents_TextChanged);
            // 
            // cbShowIncompatible
            // 
            this.cbShowIncompatible.AutoSize = true;
            this.cbShowIncompatible.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbShowIncompatible.Location = new System.Drawing.Point(305, 0);
            this.cbShowIncompatible.Name = "cbShowIncompatible";
            this.cbShowIncompatible.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cbShowIncompatible.Size = new System.Drawing.Size(131, 26);
            this.cbShowIncompatible.TabIndex = 4;
            this.cbShowIncompatible.Text = "Show Incompatible";
            this.cbShowIncompatible.UseVisualStyleBackColor = true;
            this.cbShowIncompatible.CheckedChanged += new System.EventHandler(this.cbShowIncompatible_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.label4.Size = new System.Drawing.Size(45, 19);
            this.label4.TabIndex = 0;
            this.label4.Text = "Search:";
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
            this.olvComponents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.olvComponents.CellEditUseWholeCell = false;
            this.olvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvNamespace,
            this.olvRole,
            this.olvCompatible});
            this.olvComponents.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvComponents.HideSelection = false;
            this.olvComponents.IsSimpleDragSource = true;
            this.olvComponents.Location = new System.Drawing.Point(0, 36);
            this.olvComponents.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvComponents.Name = "olvComponents";
            this.olvComponents.Size = new System.Drawing.Size(436, 554);
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
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(0, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(436, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "(Add Components from below with Drag and Drop)";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(436, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Components";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 38);
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
            this.splitContainer2.Size = new System.Drawing.Size(871, 584);
            this.splitContainer2.SplitterDistance = 313;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 7;
            // 
            // diagramPanel
            // 
            this.diagramPanel.AutoSize = true;
            this.diagramPanel.Controls.Add(this.btnReRunChecks);
            this.diagramPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.diagramPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagramPanel.Location = new System.Drawing.Point(0, 0);
            this.diagramPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.diagramPanel.MinimumSize = new System.Drawing.Size(2, 127);
            this.diagramPanel.Name = "diagramPanel";
            this.diagramPanel.Size = new System.Drawing.Size(867, 309);
            this.diagramPanel.TabIndex = 0;
            this.diagramPanel.Text = "pipelineDiagram";
            // 
            // btnReRunChecks
            // 
            this.btnReRunChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReRunChecks.Location = new System.Drawing.Point(747, 279);
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
            this.gbArguments.Cursor = System.Windows.Forms.Cursors.Default;
            this.gbArguments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbArguments.Location = new System.Drawing.Point(0, 0);
            this.gbArguments.Name = "gbArguments";
            this.gbArguments.Size = new System.Drawing.Size(867, 262);
            this.gbArguments.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label2.Size = new System.Drawing.Size(871, 38);
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
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private OLVColumn olvName;
        private OLVColumn olvCompatible;
        private OLVColumn olvRole;
        private OLVColumn olvNamespace;
        private Label label2;
        private Button btnReRunChecks;
        private Label label3;
        private SplitContainer splitContainer2;
        private Panel panel1;
        private TextBox tbSearchComponents;
        private Label label4;
        private CheckBox cbShowIncompatible;
        private Panel gbArguments;
    }
}
