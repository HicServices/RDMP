namespace ReusableUIComponents.Dependencies
{
    partial class DependencyGraphUI
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
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.nDependencyDepth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ddLayout = new System.Windows.Forms.ComboBox();
            this.pFilterCheckboxes = new System.Windows.Forms.FlowLayoutPanel();
            this.pHighlightCheckboxes = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTimeSpentFindingDependencies = new System.Windows.Forms.Label();
            this.lblTimeSpentLayingout = new System.Windows.Forms.Label();
            this.lblTimeToHighlight = new System.Windows.Forms.Label();
            this.lblLoadProgress = new System.Windows.Forms.Label();
            this.lblThisMayTakeSomeTime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbHighlight = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.cbToggleAllHighlight = new System.Windows.Forms.CheckBox();
            this.cbToggleAllShow = new System.Windows.Forms.CheckBox();
            this.loadIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.nDependencyDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.elementHost1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(1206, 614);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = null;
            // 
            // nDependencyDepth
            // 
            this.nDependencyDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nDependencyDepth.Location = new System.Drawing.Point(115, 683);
            this.nDependencyDepth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nDependencyDepth.Name = "nDependencyDepth";
            this.nDependencyDepth.Size = new System.Drawing.Size(76, 20);
            this.nDependencyDepth.TabIndex = 2;
            this.nDependencyDepth.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nDependencyDepth.ValueChanged += new System.EventHandler(this.nDependencyDepth_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 685);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Dependency Depth:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 655);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Layout:";
            // 
            // ddLayout
            // 
            this.ddLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLayout.FormattingEnabled = true;
            this.ddLayout.Location = new System.Drawing.Point(115, 652);
            this.ddLayout.Name = "ddLayout";
            this.ddLayout.Size = new System.Drawing.Size(76, 21);
            this.ddLayout.TabIndex = 5;
            this.ddLayout.SelectedIndexChanged += new System.EventHandler(this.ddLayout_SelectedIndexChanged);
            // 
            // pFilterCheckboxes
            // 
            this.pFilterCheckboxes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pFilterCheckboxes.AutoScroll = true;
            this.pFilterCheckboxes.Location = new System.Drawing.Point(549, 679);
            this.pFilterCheckboxes.Name = "pFilterCheckboxes";
            this.pFilterCheckboxes.Size = new System.Drawing.Size(603, 22);
            this.pFilterCheckboxes.TabIndex = 6;
            // 
            // pHighlightCheckboxes
            // 
            this.pHighlightCheckboxes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pHighlightCheckboxes.AutoScroll = true;
            this.pHighlightCheckboxes.Location = new System.Drawing.Point(549, 652);
            this.pHighlightCheckboxes.Name = "pHighlightCheckboxes";
            this.pHighlightCheckboxes.Size = new System.Drawing.Size(603, 20);
            this.pHighlightCheckboxes.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(450, 655);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Highlight:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(464, 682);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Show:";
            // 
            // lblTimeSpentFindingDependencies
            // 
            this.lblTimeSpentFindingDependencies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTimeSpentFindingDependencies.AutoSize = true;
            this.lblTimeSpentFindingDependencies.Location = new System.Drawing.Point(197, 655);
            this.lblTimeSpentFindingDependencies.Name = "lblTimeSpentFindingDependencies";
            this.lblTimeSpentFindingDependencies.Size = new System.Drawing.Size(116, 13);
            this.lblTimeSpentFindingDependencies.TabIndex = 10;
            this.lblTimeSpentFindingDependencies.Text = "Finding Dependencies:";
            // 
            // lblTimeSpentLayingout
            // 
            this.lblTimeSpentLayingout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTimeSpentLayingout.AutoSize = true;
            this.lblTimeSpentLayingout.Location = new System.Drawing.Point(197, 672);
            this.lblTimeSpentLayingout.Name = "lblTimeSpentLayingout";
            this.lblTimeSpentLayingout.Size = new System.Drawing.Size(42, 13);
            this.lblTimeSpentLayingout.TabIndex = 10;
            this.lblTimeSpentLayingout.Text = "Layout:";
            // 
            // lblTimeToHighlight
            // 
            this.lblTimeToHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTimeToHighlight.AutoSize = true;
            this.lblTimeToHighlight.Location = new System.Drawing.Point(197, 688);
            this.lblTimeToHighlight.Name = "lblTimeToHighlight";
            this.lblTimeToHighlight.Size = new System.Drawing.Size(77, 13);
            this.lblTimeToHighlight.TabIndex = 12;
            this.lblTimeToHighlight.Text = "Highlight Time:";
            // 
            // lblLoadProgress
            // 
            this.lblLoadProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLoadProgress.BackColor = System.Drawing.Color.Transparent;
            this.lblLoadProgress.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoadProgress.Location = new System.Drawing.Point(422, 344);
            this.lblLoadProgress.Name = "lblLoadProgress";
            this.lblLoadProgress.Size = new System.Drawing.Size(321, 22);
            this.lblLoadProgress.TabIndex = 13;
            this.lblLoadProgress.Text = "...";
            this.lblLoadProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblThisMayTakeSomeTime
            // 
            this.lblThisMayTakeSomeTime.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblThisMayTakeSomeTime.BackColor = System.Drawing.Color.Transparent;
            this.lblThisMayTakeSomeTime.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThisMayTakeSomeTime.Location = new System.Drawing.Point(289, 378);
            this.lblThisMayTakeSomeTime.Name = "lblThisMayTakeSomeTime";
            this.lblThisMayTakeSomeTime.Size = new System.Drawing.Size(632, 63);
            this.lblThisMayTakeSomeTime.TabIndex = 14;
            this.lblThisMayTakeSomeTime.Text = "This may take some time depending on dataset size, layout algorithm chosen and sy" +
    "stem hardware.";
            this.lblThisMayTakeSomeTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblThisMayTakeSomeTime.Visible = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(385, 629);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Search and Highlight:";
            // 
            // tbHighlight
            // 
            this.tbHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbHighlight.Location = new System.Drawing.Point(500, 626);
            this.tbHighlight.MaximumSize = new System.Drawing.Size(360, 20);
            this.tbHighlight.MinimumSize = new System.Drawing.Size(200, 20);
            this.tbHighlight.Name = "tbHighlight";
            this.tbHighlight.Size = new System.Drawing.Size(360, 20);
            this.tbHighlight.TabIndex = 16;
            this.tbHighlight.TextChanged += new System.EventHandler(this.tbHighlight_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(870, 629);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(16, 13);
            this.lblSearch.TabIndex = 17;
            this.lblSearch.Text = "...";
            // 
            // cbToggleAllHighlight
            // 
            this.cbToggleAllHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbToggleAllHighlight.AutoSize = true;
            this.cbToggleAllHighlight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbToggleAllHighlight.Location = new System.Drawing.Point(500, 654);
            this.cbToggleAllHighlight.Name = "cbToggleAllHighlight";
            this.cbToggleAllHighlight.Size = new System.Drawing.Size(40, 17);
            this.cbToggleAllHighlight.TabIndex = 18;
            this.cbToggleAllHighlight.Text = "All";
            this.cbToggleAllHighlight.UseVisualStyleBackColor = true;
            // 
            // cbToggleAllShow
            // 
            this.cbToggleAllShow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbToggleAllShow.AutoSize = true;
            this.cbToggleAllShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbToggleAllShow.Location = new System.Drawing.Point(500, 681);
            this.cbToggleAllShow.Name = "cbToggleAllShow";
            this.cbToggleAllShow.Size = new System.Drawing.Size(40, 17);
            this.cbToggleAllShow.TabIndex = 19;
            this.cbToggleAllShow.Text = "All";
            this.cbToggleAllShow.UseVisualStyleBackColor = true;
            // 
            // loadIcon
            // 
            this.loadIcon.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.loadIcon.BackColor = System.Drawing.Color.Transparent;
            this.loadIcon.Image = global::ReusableUIComponents.Images.LoadIcon;
            this.loadIcon.InitialImage = global::ReusableUIComponents.Images.LoadIcon;
            this.loadIcon.Location = new System.Drawing.Point(531, 240);
            this.loadIcon.Name = "loadIcon";
            this.loadIcon.Size = new System.Drawing.Size(101, 101);
            this.loadIcon.TabIndex = 11;
            this.loadIcon.TabStop = false;
            // 
            // DependencyGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbToggleAllShow);
            this.Controls.Add(this.cbToggleAllHighlight);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.tbHighlight);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblThisMayTakeSomeTime);
            this.Controls.Add(this.lblLoadProgress);
            this.Controls.Add(this.lblTimeToHighlight);
            this.Controls.Add(this.loadIcon);
            this.Controls.Add(this.lblTimeSpentLayingout);
            this.Controls.Add(this.lblTimeSpentFindingDependencies);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pHighlightCheckboxes);
            this.Controls.Add(this.pFilterCheckboxes);
            this.Controls.Add(this.ddLayout);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nDependencyDepth);
            this.Controls.Add(this.elementHost1);
            this.Name = "DependencyGraph";
            this.Size = new System.Drawing.Size(1209, 704);
            this.Load += new System.EventHandler(this.DependencyGraph_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nDependencyDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.NumericUpDown nDependencyDepth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ddLayout;
        private System.Windows.Forms.FlowLayoutPanel pFilterCheckboxes;
        private System.Windows.Forms.FlowLayoutPanel pHighlightCheckboxes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTimeSpentFindingDependencies;
        private System.Windows.Forms.Label lblTimeSpentLayingout;
        private System.Windows.Forms.PictureBox loadIcon;
        private System.Windows.Forms.Label lblTimeToHighlight;
        private System.Windows.Forms.Label lblLoadProgress;
        private System.Windows.Forms.Label lblThisMayTakeSomeTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbHighlight;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.CheckBox cbToggleAllHighlight;
        private System.Windows.Forms.CheckBox cbToggleAllShow;
    }
}
