using BrightIdeasSoftware;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.Refreshing;

namespace CatalogueManager.MainFormUITabs
{
    partial class CatalogueItemTab
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
            this.ci_tbID = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.ci_tbComments = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.ci_tbLimitations = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.ci_tbTopics = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.ci_tbAggregationMethod = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.ci_tbDescription = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.ci_tbResearchRelevance = new System.Windows.Forms.TextBox();
            this.ci_ddPeriodicity = new System.Windows.Forms.ComboBox();
            this.ci_tbStatisticalConsiderations = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.ci_tbName = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ci_tbID
            // 
            this.ci_tbID.Location = new System.Drawing.Point(103, 12);
            this.ci_tbID.Name = "ci_tbID";
            this.ci_tbID.ReadOnly = true;
            this.ci_tbID.Size = new System.Drawing.Size(145, 20);
            this.ci_tbID.TabIndex = 137;
            // 
            // label28
            // 
            this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(5, 15);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(92, 13);
            this.label28.TabIndex = 147;
            this.label28.Text = "Catalogue Item ID";
            // 
            // label27
            // 
            this.label27.Location = new System.Drawing.Point(5, 12);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(124, 113);
            this.label27.TabIndex = 148;
            this.label27.Text = "Statistical Concept and Methodology";
            // 
            // label26
            // 
            this.label26.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(62, 37);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(35, 13);
            this.label26.TabIndex = 149;
            this.label26.Text = "Name";
            // 
            // ci_tbComments
            // 
            this.ci_tbComments.AcceptsReturn = true;
            this.ci_tbComments.Location = new System.Drawing.Point(135, 504);
            this.ci_tbComments.Multiline = true;
            this.ci_tbComments.Name = "ci_tbComments";
            this.ci_tbComments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbComments.Size = new System.Drawing.Size(639, 84);
            this.ci_tbComments.TabIndex = 146;
            this.ci_tbComments.TextChanged += new System.EventHandler(this.ci_tbComments_TextChanged);
            // 
            // label30
            // 
            this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(26, 131);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(108, 13);
            this.label30.TabIndex = 150;
            this.label30.Text = "Research Relevance";
            // 
            // ci_tbLimitations
            // 
            this.ci_tbLimitations.AcceptsReturn = true;
            this.ci_tbLimitations.Location = new System.Drawing.Point(135, 407);
            this.ci_tbLimitations.Multiline = true;
            this.ci_tbLimitations.Name = "ci_tbLimitations";
            this.ci_tbLimitations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbLimitations.Size = new System.Drawing.Size(639, 91);
            this.ci_tbLimitations.TabIndex = 145;
            this.ci_tbLimitations.TextChanged += new System.EventHandler(this.ci_tbLimitations_TextChanged);
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(37, 63);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(60, 13);
            this.label31.TabIndex = 151;
            this.label31.Text = "Description";
            // 
            // ci_tbTopics
            // 
            this.ci_tbTopics.Location = new System.Drawing.Point(135, 251);
            this.ci_tbTopics.Name = "ci_tbTopics";
            this.ci_tbTopics.Size = new System.Drawing.Size(639, 20);
            this.ci_tbTopics.TabIndex = 142;
            this.ci_tbTopics.TextChanged += new System.EventHandler(this.ci_tbTopics_TextChanged);
            // 
            // label32
            // 
            this.label32.Location = new System.Drawing.Point(8, 307);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(121, 94);
            this.label32.TabIndex = 152;
            this.label32.Text = "Aggregation ProcessPipelineData";
            // 
            // ci_tbAggregationMethod
            // 
            this.ci_tbAggregationMethod.AcceptsReturn = true;
            this.ci_tbAggregationMethod.Location = new System.Drawing.Point(135, 304);
            this.ci_tbAggregationMethod.Multiline = true;
            this.ci_tbAggregationMethod.Name = "ci_tbAggregationMethod";
            this.ci_tbAggregationMethod.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbAggregationMethod.Size = new System.Drawing.Size(639, 97);
            this.ci_tbAggregationMethod.TabIndex = 144;
            this.ci_tbAggregationMethod.TextChanged += new System.EventHandler(this.ci_tbAggregationMethod_TextChanged);
            // 
            // label33
            // 
            this.label33.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(81, 254);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(34, 13);
            this.label33.TabIndex = 153;
            this.label33.Text = "Topic";
            // 
            // ci_tbDescription
            // 
            this.ci_tbDescription.AcceptsReturn = true;
            this.ci_tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ci_tbDescription.Location = new System.Drawing.Point(103, 60);
            this.ci_tbDescription.Multiline = true;
            this.ci_tbDescription.Name = "ci_tbDescription";
            this.ci_tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbDescription.Size = new System.Drawing.Size(699, 122);
            this.ci_tbDescription.TabIndex = 139;
            this.ci_tbDescription.TextChanged += new System.EventHandler(this.ci_tbDescription_TextChanged);
            // 
            // label34
            // 
            this.label34.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(69, 285);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(55, 13);
            this.label34.TabIndex = 154;
            this.label34.Text = "Periodicity";
            // 
            // ci_tbResearchRelevance
            // 
            this.ci_tbResearchRelevance.AcceptsReturn = true;
            this.ci_tbResearchRelevance.Location = new System.Drawing.Point(135, 131);
            this.ci_tbResearchRelevance.Multiline = true;
            this.ci_tbResearchRelevance.Name = "ci_tbResearchRelevance";
            this.ci_tbResearchRelevance.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbResearchRelevance.Size = new System.Drawing.Size(639, 114);
            this.ci_tbResearchRelevance.TabIndex = 141;
            this.ci_tbResearchRelevance.TextChanged += new System.EventHandler(this.ci_tbResearchRelevance_TextChanged);
            // 
            // ci_ddPeriodicity
            // 
            this.ci_ddPeriodicity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ci_ddPeriodicity.FormattingEnabled = true;
            this.ci_ddPeriodicity.Location = new System.Drawing.Point(135, 277);
            this.ci_ddPeriodicity.Name = "ci_ddPeriodicity";
            this.ci_ddPeriodicity.Size = new System.Drawing.Size(335, 21);
            this.ci_ddPeriodicity.TabIndex = 143;
            this.ci_ddPeriodicity.SelectedIndexChanged += new System.EventHandler(this.ci_ddPeriodicity_SelectedIndexChanged);
            // 
            // ci_tbStatisticalConsiderations
            // 
            this.ci_tbStatisticalConsiderations.AcceptsReturn = true;
            this.ci_tbStatisticalConsiderations.Location = new System.Drawing.Point(135, 12);
            this.ci_tbStatisticalConsiderations.Multiline = true;
            this.ci_tbStatisticalConsiderations.Name = "ci_tbStatisticalConsiderations";
            this.ci_tbStatisticalConsiderations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ci_tbStatisticalConsiderations.Size = new System.Drawing.Size(639, 113);
            this.ci_tbStatisticalConsiderations.TabIndex = 140;
            this.ci_tbStatisticalConsiderations.TextChanged += new System.EventHandler(this.ci_tbStatisticalConsiderations_TextChanged);
            // 
            // label35
            // 
            this.label35.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(68, 407);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(56, 13);
            this.label35.TabIndex = 155;
            this.label35.Text = "Limitations";
            // 
            // label36
            // 
            this.label36.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(68, 507);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(56, 13);
            this.label36.TabIndex = 156;
            this.label36.Text = "Comments";
            // 
            // ci_tbName
            // 
            this.ci_tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ci_tbName.Location = new System.Drawing.Point(103, 34);
            this.ci_tbName.Name = "ci_tbName";
            this.ci_tbName.Size = new System.Drawing.Size(694, 20);
            this.ci_tbName.TabIndex = 138;
            this.ci_tbName.TextChanged += new System.EventHandler(this.ci_tbName_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.objectSaverButton1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnExpandOrCollapse);
            this.splitContainer1.Panel1.Controls.Add(this.ci_tbDescription);
            this.splitContainer1.Panel1.Controls.Add(this.ci_tbID);
            this.splitContainer1.Panel1.Controls.Add(this.ci_tbName);
            this.splitContainer1.Panel1.Controls.Add(this.label31);
            this.splitContainer1.Panel1.Controls.Add(this.label28);
            this.splitContainer1.Panel1.Controls.Add(this.label26);
            this.splitContainer1.Panel1MinSize = 220;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.label27);
            this.splitContainer1.Panel2.Controls.Add(this.label36);
            this.splitContainer1.Panel2.Controls.Add(this.label35);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbComments);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbStatisticalConsiderations);
            this.splitContainer1.Panel2.Controls.Add(this.label30);
            this.splitContainer1.Panel2.Controls.Add(this.ci_ddPeriodicity);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbLimitations);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbResearchRelevance);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbTopics);
            this.splitContainer1.Panel2.Controls.Add(this.label34);
            this.splitContainer1.Panel2.Controls.Add(this.label32);
            this.splitContainer1.Panel2.Controls.Add(this.label33);
            this.splitContainer1.Panel2.Controls.Add(this.ci_tbAggregationMethod);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(805, 727);
            this.splitContainer1.SplitterDistance = 220;
            this.splitContainer1.TabIndex = 158;
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(132, 185);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 162;
            this.objectSaverButton1.Text = "objectSaverButton1";
            this.objectSaverButton1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 190);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 160;
            this.label1.Text = "Additional Fields";
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(8, 185);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(29, 23);
            this.btnExpandOrCollapse.TabIndex = 159;
            this.btnExpandOrCollapse.Text = "+";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // CatalogueItemTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "CatalogueItemTab";
            this.Size = new System.Drawing.Size(805, 727);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox ci_tbID;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox ci_tbComments;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox ci_tbLimitations;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox ci_tbTopics;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox ci_tbAggregationMethod;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox ci_tbDescription;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox ci_tbResearchRelevance;
        private System.Windows.Forms.ComboBox ci_ddPeriodicity;
        private System.Windows.Forms.TextBox ci_tbStatisticalConsiderations;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox ci_tbName;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnExpandOrCollapse;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}
