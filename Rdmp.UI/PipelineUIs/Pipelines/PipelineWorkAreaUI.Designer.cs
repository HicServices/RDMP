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
            splitContainer1 = new SplitContainer();
            panel1 = new Panel();
            tbSearchComponents = new TextBox();
            cbShowIncompatible = new CheckBox();
            label4 = new Label();
            olvComponents = new ObjectListView();
            olvName = new OLVColumn();
            olvNamespace = new OLVColumn();
            olvRole = new OLVColumn();
            olvCompatible = new OLVColumn();
            label3 = new Label();
            label1 = new Label();
            splitContainer2 = new SplitContainer();
            diagramPanel = new Panel();
            btnReRunChecks = new Button();
            gbArguments = new Panel();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)olvComponents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            diagramPanel.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel1);
            splitContainer1.Panel1.Controls.Add(olvComponents);
            splitContainer1.Panel1.Controls.Add(label3);
            splitContainer1.Panel1.Controls.Add(label1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Size = new System.Drawing.Size(1312, 622);
            splitContainer1.SplitterDistance = 436;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbSearchComponents);
            panel1.Controls.Add(cbShowIncompatible);
            panel1.Controls.Add(label4);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 596);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(436, 26);
            panel1.TabIndex = 3;
            // 
            // tbSearchComponents
            // 
            tbSearchComponents.Dock = DockStyle.Fill;
            tbSearchComponents.Location = new System.Drawing.Point(45, 0);
            tbSearchComponents.Name = "tbSearchComponents";
            tbSearchComponents.Size = new System.Drawing.Size(260, 23);
            tbSearchComponents.TabIndex = 1;
            tbSearchComponents.TextChanged += tbSearchComponents_TextChanged;
            // 
            // cbShowIncompatible
            // 
            cbShowIncompatible.AutoSize = true;
            cbShowIncompatible.Dock = DockStyle.Right;
            cbShowIncompatible.Location = new System.Drawing.Point(305, 0);
            cbShowIncompatible.Name = "cbShowIncompatible";
            cbShowIncompatible.Padding = new Padding(3, 0, 0, 0);
            cbShowIncompatible.Size = new System.Drawing.Size(131, 26);
            cbShowIncompatible.TabIndex = 4;
            cbShowIncompatible.Text = "Show Incompatible";
            cbShowIncompatible.UseVisualStyleBackColor = true;
            cbShowIncompatible.CheckedChanged += cbShowIncompatible_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Left;
            label4.Location = new System.Drawing.Point(0, 0);
            label4.Name = "label4";
            label4.Padding = new Padding(0, 4, 0, 0);
            label4.Size = new System.Drawing.Size(45, 19);
            label4.TabIndex = 0;
            label4.Text = "Search:";
            // 
            // olvComponents
            // 
            olvComponents.AllColumns.Add(olvName);
            olvComponents.AllColumns.Add(olvNamespace);
            olvComponents.AllColumns.Add(olvRole);
            olvComponents.AllColumns.Add(olvCompatible);
            olvComponents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            olvComponents.BorderStyle = BorderStyle.FixedSingle;
            olvComponents.CellEditUseWholeCell = false;
            olvComponents.Columns.AddRange(new ColumnHeader[] { olvName, olvNamespace, olvRole, olvCompatible });
            olvComponents.IsSimpleDragSource = true;
            olvComponents.Location = new System.Drawing.Point(0, 36);
            olvComponents.Margin = new Padding(4, 3, 4, 3);
            olvComponents.Name = "olvComponents";
            olvComponents.Size = new System.Drawing.Size(436, 554);
            olvComponents.TabIndex = 1;
            olvComponents.UseCompatibleStateImageBehavior = false;
            olvComponents.View = View.Details;
            olvComponents.CellRightClick += olvComponents_CellRightClick;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.Groupable = false;
            olvName.MinimumWidth = 100;
            olvName.Text = "Name";
            olvName.Width = 100;
            // 
            // olvNamespace
            // 
            olvNamespace.AspectName = "Namespace";
            olvNamespace.Groupable = false;
            olvNamespace.Text = "Namespace";
            // 
            // olvRole
            // 
            olvRole.AspectName = "GetRole";
            olvRole.Text = "Role";
            olvRole.Width = 80;
            // 
            // olvCompatible
            // 
            olvCompatible.AspectName = "UIDescribeCompatible";
            olvCompatible.Groupable = false;
            olvCompatible.Text = "Compatible?";
            olvCompatible.Width = 70;
            // 
            // label3
            // 
            label3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            label3.Dock = DockStyle.Top;
            label3.ForeColor = System.Drawing.Color.Red;
            label3.Location = new System.Drawing.Point(0, 18);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(436, 18);
            label3.TabIndex = 2;
            label3.Text = "(Add Components from below with Drag and Drop)";
            // 
            // label1
            // 
            label1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            label1.Dock = DockStyle.Top;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(436, 18);
            label1.TabIndex = 0;
            label1.Text = "Components";
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.Fixed3D;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 38);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(diagramPanel);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(gbArguments);
            splitContainer2.Size = new System.Drawing.Size(871, 584);
            splitContainer2.SplitterDistance = 313;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 7;
            // 
            // diagramPanel
            // 
            diagramPanel.AutoSize = true;
            diagramPanel.Controls.Add(btnReRunChecks);
            diagramPanel.Dock = DockStyle.Fill;
            diagramPanel.Location = new System.Drawing.Point(0, 0);
            diagramPanel.Margin = new Padding(4, 3, 4, 3);
            diagramPanel.MinimumSize = new System.Drawing.Size(2, 127);
            diagramPanel.Name = "diagramPanel";
            diagramPanel.Size = new System.Drawing.Size(867, 309);
            diagramPanel.TabIndex = 0;
            diagramPanel.Text = "pipelineDiagram";
            // 
            // btnReRunChecks
            // 
            btnReRunChecks.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReRunChecks.Location = new System.Drawing.Point(747, 253);
            btnReRunChecks.Margin = new Padding(4, 3, 4, 3);
            btnReRunChecks.Name = "btnReRunChecks";
            btnReRunChecks.Size = new System.Drawing.Size(118, 27);
            btnReRunChecks.TabIndex = 0;
            btnReRunChecks.Text = "Re-Run Checks";
            btnReRunChecks.UseVisualStyleBackColor = true;
            btnReRunChecks.Click += btnReRunChecks_Click;
            // 
            // gbArguments
            // 
            gbArguments.Dock = DockStyle.Fill;
            gbArguments.Location = new System.Drawing.Point(0, 0);
            gbArguments.Name = "gbArguments";
            gbArguments.Size = new System.Drawing.Size(867, 262);
            gbArguments.TabIndex = 0;
            // 
            // label2
            // 
            label2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            label2.Dock = DockStyle.Top;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            label2.Location = new System.Drawing.Point(0, 0);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Padding = new Padding(0, 10, 0, 0);
            label2.Size = new System.Drawing.Size(871, 38);
            label2.TabIndex = 6;
            label2.Text = "Current Pipeline Configuration";
            label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // PipelineWorkAreaUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "PipelineWorkAreaUI";
            Size = new System.Drawing.Size(1312, 622);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)olvComponents).EndInit();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            diagramPanel.ResumeLayout(false);
            ResumeLayout(false);

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
