namespace CatalogueManager.SimpleDialogs.Automation
{
    partial class AutomationServiceSlotUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dqeMaxJobs = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.dqeDaysBetween = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.dleMaxJobs = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ddDQEFailureStrategy = new System.Windows.Forms.ComboBox();
            this.ddDQESelectionStrategy = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ddDLEFailureStrategy = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ddCacheFailureStrategy = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.cacheMaxJobs = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.lockableUI1 = new ReusableUIComponents.LockableUI();
            this.overrideCommandTimeout = new System.Windows.Forms.NumericUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.automateablePipelineCollectionUI1 = new CatalogueManager.SimpleDialogs.Automation.AutomateablePipelineCollectionUI();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            ((System.ComponentModel.ISupportInitialize)(this.dqeMaxJobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dqeDaysBetween)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dleMaxJobs)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cacheMaxJobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overrideCommandTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Slot ID:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(47, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "DQE Maximum Concurrent Jobs:";
            // 
            // dqeMaxJobs
            // 
            this.dqeMaxJobs.Location = new System.Drawing.Point(184, 19);
            this.dqeMaxJobs.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.dqeMaxJobs.Name = "dqeMaxJobs";
            this.dqeMaxJobs.Size = new System.Drawing.Size(120, 20);
            this.dqeMaxJobs.TabIndex = 3;
            this.dqeMaxJobs.ValueChanged += new System.EventHandler(this.dqeMaxJobs_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "DQE Days Between Evaluations:";
            // 
            // dqeDaysBetween
            // 
            this.dqeDaysBetween.Location = new System.Drawing.Point(184, 45);
            this.dqeDaysBetween.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.dqeDaysBetween.Name = "dqeDaysBetween";
            this.dqeDaysBetween.Size = new System.Drawing.Size(120, 20);
            this.dqeDaysBetween.TabIndex = 3;
            this.dqeDaysBetween.ValueChanged += new System.EventHandler(this.dqeDaysBetween_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(158, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "DLE Maximum Concurrent Jobs:";
            // 
            // dleMaxJobs
            // 
            this.dleMaxJobs.Location = new System.Drawing.Point(199, 19);
            this.dleMaxJobs.Name = "dleMaxJobs";
            this.dleMaxJobs.Size = new System.Drawing.Size(120, 20);
            this.dleMaxJobs.TabIndex = 3;
            this.dleMaxJobs.ValueChanged += new System.EventHandler(this.dleMaxJobs_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ddDQEFailureStrategy);
            this.groupBox1.Controls.Add(this.ddDQESelectionStrategy);
            this.groupBox1.Controls.Add(this.dqeMaxJobs);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dqeDaysBetween);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(9, 89);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1015, 156);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Quality Engine Automation (Tells the Automation server to run the DQE in a r" +
    "ound robin of your datasets)";
            // 
            // ddDQEFailureStrategy
            // 
            this.ddDQEFailureStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDQEFailureStrategy.FormattingEnabled = true;
            this.ddDQEFailureStrategy.Location = new System.Drawing.Point(184, 111);
            this.ddDQEFailureStrategy.Name = "ddDQEFailureStrategy";
            this.ddDQEFailureStrategy.Size = new System.Drawing.Size(457, 21);
            this.ddDQEFailureStrategy.TabIndex = 5;
            this.ddDQEFailureStrategy.SelectedIndexChanged += new System.EventHandler(this.ddDQEFailureStrategy_SelectedIndexChanged);
            // 
            // ddDQESelectionStrategy
            // 
            this.ddDQESelectionStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDQESelectionStrategy.FormattingEnabled = true;
            this.ddDQESelectionStrategy.Location = new System.Drawing.Point(184, 71);
            this.ddDQESelectionStrategy.Name = "ddDQESelectionStrategy";
            this.ddDQESelectionStrategy.Size = new System.Drawing.Size(457, 21);
            this.ddDQESelectionStrategy.TabIndex = 5;
            this.ddDQESelectionStrategy.SelectedIndexChanged += new System.EventHandler(this.ddDQESelectionStrategy_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(70, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "DQE Failure Strategy:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(318, 135);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(148, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "(What to do if a DQE run fails)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(310, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(581, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "(Catalogues will only be selected to have the DQE run on them if there isn\'t alre" +
    "ady DQE results within this number of days)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(310, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(521, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "(Maximum number of simultaneous DQE jobs to run, if this is too high it might slo" +
    "w down your database server)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(196, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(419, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "(How to choose which Catalogue to run the DQE on next - when theres a slot availa" +
    "ble)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(56, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "DQE Selection Strategy:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.ddDLEFailureStrategy);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.dleMaxJobs);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(9, 251);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1015, 104);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data Load Engine (Tells the Automation server to run any Due loads - See LoadPeri" +
    "odically)";
            // 
            // ddDLEFailureStrategy
            // 
            this.ddDLEFailureStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDLEFailureStrategy.FormattingEnabled = true;
            this.ddDLEFailureStrategy.Location = new System.Drawing.Point(197, 45);
            this.ddDLEFailureStrategy.Name = "ddDLEFailureStrategy";
            this.ddDLEFailureStrategy.Size = new System.Drawing.Size(457, 21);
            this.ddDLEFailureStrategy.TabIndex = 8;
            this.ddDLEFailureStrategy.SelectedIndexChanged += new System.EventHandler(this.ddDLEFailureStrategy_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(83, 48);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(107, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "DLE Failure Strategy:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(331, 69);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(199, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "(What to do if a Data Load crashes/fails)";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.ddCacheFailureStrategy);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.cacheMaxJobs);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Location = new System.Drawing.Point(9, 361);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1015, 104);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Caching Engine (Tells the Automation server to run caching tasks you have configu" +
    "red - See LoadProgress/CacheProgress)";
            // 
            // ddCacheFailureStrategy
            // 
            this.ddCacheFailureStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheFailureStrategy.FormattingEnabled = true;
            this.ddCacheFailureStrategy.Location = new System.Drawing.Point(197, 45);
            this.ddCacheFailureStrategy.Name = "ddCacheFailureStrategy";
            this.ddCacheFailureStrategy.Size = new System.Drawing.Size(457, 21);
            this.ddCacheFailureStrategy.TabIndex = 8;
            this.ddCacheFailureStrategy.SelectedIndexChanged += new System.EventHandler(this.ddCacheFailureStrategy_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(83, 48);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(117, 13);
            this.label13.TabIndex = 6;
            this.label13.Text = "Cache Failure Strategy:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(331, 69);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(199, 13);
            this.label14.TabIndex = 7;
            this.label14.Text = "(What to do if a Data Load crashes/fails)";
            // 
            // cacheMaxJobs
            // 
            this.cacheMaxJobs.Location = new System.Drawing.Point(199, 19);
            this.cacheMaxJobs.Name = "cacheMaxJobs";
            this.cacheMaxJobs.Size = new System.Drawing.Size(120, 20);
            this.cacheMaxJobs.TabIndex = 3;
            this.cacheMaxJobs.ValueChanged += new System.EventHandler(this.cacheMaxJobs_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(25, 21);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(168, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "Cache Maximum Concurrent Jobs:";
            // 
            // lockableUI1
            // 
            this.lockableUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lockableUI1.Location = new System.Drawing.Point(9, 29);
            this.lockableUI1.Lockable = null;
            this.lockableUI1.Name = "lockableUI1";
            this.lockableUI1.Poll = false;
            this.lockableUI1.Size = new System.Drawing.Size(1015, 61);
            this.lockableUI1.TabIndex = 2;
            // 
            // overrideCommandTimeout
            // 
            this.overrideCommandTimeout.Location = new System.Drawing.Point(319, 4);
            this.overrideCommandTimeout.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.overrideCommandTimeout.Name = "overrideCommandTimeout";
            this.overrideCommandTimeout.Size = new System.Drawing.Size(120, 20);
            this.overrideCommandTimeout.TabIndex = 7;
            this.overrideCommandTimeout.ValueChanged += new System.EventHandler(this.overrideCommandTimeout_ValueChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(170, 6);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(143, 13);
            this.label16.TabIndex = 6;
            this.label16.Text = "Override Command Timeouts";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(445, 6);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(468, 13);
            this.label17.TabIndex = 6;
            this.label17.Text = "(Default timeout for commands in SQL is 30s, set this to a nonzero positive numbe" +
    "r to override this)";
            // 
            // automateablePipelineCollectionUI1
            // 
            this.automateablePipelineCollectionUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.automateablePipelineCollectionUI1.AutomationServiceSlot = null;
            this.automateablePipelineCollectionUI1.Location = new System.Drawing.Point(9, 498);
            this.automateablePipelineCollectionUI1.Name = "automateablePipelineCollectionUI1";
            this.automateablePipelineCollectionUI1.Size = new System.Drawing.Size(1015, 355);
            this.automateablePipelineCollectionUI1.TabIndex = 7;
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(9, 468);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(54, 27);
            this.objectSaverButton1.TabIndex = 9;
            // 
            // AutomationServiceSlotUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.overrideCommandTimeout);
            this.Controls.Add(this.automateablePipelineCollectionUI1);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lockableUI1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label1);
            this.Name = "AutomationServiceSlotUI";
            this.Size = new System.Drawing.Size(1027, 856);
            ((System.ComponentModel.ISupportInitialize)(this.dqeMaxJobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dqeDaysBetween)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dleMaxJobs)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cacheMaxJobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overrideCommandTimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label2;
        private ReusableUIComponents.LockableUI lockableUI1;
        private System.Windows.Forms.NumericUpDown dqeMaxJobs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown dqeDaysBetween;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown dleMaxJobs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ddDQESelectionStrategy;
        private System.Windows.Forms.ComboBox ddDQEFailureStrategy;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox ddDLEFailureStrategy;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox ddCacheFailureStrategy;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown cacheMaxJobs;
        private System.Windows.Forms.Label label15;
        private AutomateablePipelineCollectionUI automateablePipelineCollectionUI1;
        private System.Windows.Forms.NumericUpDown overrideCommandTimeout;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}
