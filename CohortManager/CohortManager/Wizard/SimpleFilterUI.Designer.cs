namespace CohortManager.Wizard
{
    partial class SimpleFilterUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleFilterUI));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFilterName = new System.Windows.Forms.Label();
            this.pbFlter = new System.Windows.Forms.PictureBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.pbKnownValueSets = new System.Windows.Forms.PictureBox();
            this.ddKnownGoodValues = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFlter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbKnownValueSets)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(537, 33);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ddKnownGoodValues);
            this.panel1.Controls.Add(this.pbKnownValueSets);
            this.panel1.Controls.Add(this.pbFlter);
            this.panel1.Controls.Add(this.lblFilterName);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(531, 30);
            this.panel1.TabIndex = 5;
            // 
            // lblFilterName
            // 
            this.lblFilterName.AutoSize = true;
            this.lblFilterName.Location = new System.Drawing.Point(29, 7);
            this.lblFilterName.Name = "lblFilterName";
            this.lblFilterName.Size = new System.Drawing.Size(57, 13);
            this.lblFilterName.TabIndex = 0;
            this.lblFilterName.Text = "FilterName";
            this.lblFilterName.Click += new System.EventHandler(this.lblFilterName_Click);
            // 
            // pbFlter
            // 
            this.pbFlter.Location = new System.Drawing.Point(3, 3);
            this.pbFlter.Name = "pbFlter";
            this.pbFlter.Size = new System.Drawing.Size(20, 20);
            this.pbFlter.TabIndex = 2;
            this.pbFlter.TabStop = false;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(503, 1);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(25, 25);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // pbKnownValueSets
            // 
            this.pbKnownValueSets.Location = new System.Drawing.Point(350, 3);
            this.pbKnownValueSets.Name = "pbKnownValueSets";
            this.pbKnownValueSets.Size = new System.Drawing.Size(20, 20);
            this.pbKnownValueSets.TabIndex = 3;
            this.pbKnownValueSets.TabStop = false;
            // 
            // ddKnownGoodValues
            // 
            this.ddKnownGoodValues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddKnownGoodValues.FormattingEnabled = true;
            this.ddKnownGoodValues.Location = new System.Drawing.Point(376, 3);
            this.ddKnownGoodValues.Name = "ddKnownGoodValues";
            this.ddKnownGoodValues.Size = new System.Drawing.Size(121, 21);
            this.ddKnownGoodValues.TabIndex = 4;
            this.ddKnownGoodValues.SelectedIndexChanged += new System.EventHandler(this.ddKnownGoodValues_SelectedIndexChanged);
            // 
            // SimpleFilterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SimpleFilterUI";
            this.Size = new System.Drawing.Size(537, 33);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFlter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbKnownValueSets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFilterName;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.PictureBox pbFlter;
        private System.Windows.Forms.PictureBox pbKnownValueSets;
        private System.Windows.Forms.ComboBox ddKnownGoodValues;


    }
}
