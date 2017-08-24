namespace CohortManager.Results
{
    partial class CohortIdentificationExecutionResultsUI
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpDataTable = new System.Windows.Forms.TabPage();
            this.lblRowCountIdentifiers = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tpSample = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.btnSaveResults = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tpDataTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tpSample.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Controls.Add(this.tpDataTable);
            this.tabControl1.Controls.Add(this.tpSample);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(525, 471);
            this.tabControl1.TabIndex = 4;
            // 
            // tpDataTable
            // 
            this.tpDataTable.Controls.Add(this.btnSaveResults);
            this.tpDataTable.Controls.Add(this.lblRowCountIdentifiers);
            this.tpDataTable.Controls.Add(this.dataGridView1);
            this.tpDataTable.Location = new System.Drawing.Point(4, 4);
            this.tpDataTable.Name = "tpDataTable";
            this.tpDataTable.Padding = new System.Windows.Forms.Padding(3);
            this.tpDataTable.Size = new System.Drawing.Size(517, 445);
            this.tpDataTable.TabIndex = 1;
            this.tpDataTable.Text = "Identifiers";
            this.tpDataTable.UseVisualStyleBackColor = true;
            // 
            // lblRowCountIdentifiers
            // 
            this.lblRowCountIdentifiers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRowCountIdentifiers.Location = new System.Drawing.Point(105, 428);
            this.lblRowCountIdentifiers.Name = "lblRowCountIdentifiers";
            this.lblRowCountIdentifiers.Size = new System.Drawing.Size(412, 14);
            this.lblRowCountIdentifiers.TabIndex = 1;
            this.lblRowCountIdentifiers.Text = "Row Count:";
            this.lblRowCountIdentifiers.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(511, 412);
            this.dataGridView1.TabIndex = 0;
            // 
            // tpSample
            // 
            this.tpSample.Controls.Add(this.label1);
            this.tpSample.Controls.Add(this.dataGridView2);
            this.tpSample.Location = new System.Drawing.Point(4, 4);
            this.tpSample.Name = "tpSample";
            this.tpSample.Padding = new System.Windows.Forms.Padding(3);
            this.tpSample.Size = new System.Drawing.Size(517, 445);
            this.tpSample.TabIndex = 2;
            this.tpSample.Text = "Dataset Sample";
            this.tpSample.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(1, 428);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(514, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "Dataset Sample (TOP 1000)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dataGridView2
            // 
            this.dataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(3, 3);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(511, 422);
            this.dataGridView2.TabIndex = 2;
            // 
            // btnSaveResults
            // 
            this.btnSaveResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveResults.Location = new System.Drawing.Point(6, 421);
            this.btnSaveResults.Name = "btnSaveResults";
            this.btnSaveResults.Size = new System.Drawing.Size(93, 21);
            this.btnSaveResults.TabIndex = 2;
            this.btnSaveResults.Text = "Save Results...";
            this.btnSaveResults.UseVisualStyleBackColor = true;
            this.btnSaveResults.Click += new System.EventHandler(this.btnSaveResults_Click);
            // 
            // CohortIdentificationExecutionResultsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "CohortIdentificationExecutionResultsUI";
            this.Size = new System.Drawing.Size(525, 471);
            this.tabControl1.ResumeLayout(false);
            this.tpDataTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tpSample.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpDataTable;
        private System.Windows.Forms.Label lblRowCountIdentifiers;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TabPage tpSample;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Button btnSaveResults;
    }
}
