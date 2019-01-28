namespace CatalogueManager.MainFormUITabs.SubComponents
{
    partial class TableInfoUI
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
            this.btnSynchronize = new System.Windows.Forms.Button();
            this.cbIsPrimaryExtractionTable = new System.Windows.Forms.CheckBox();
            this.btnParameters = new System.Windows.Forms.Button();
            this.tbTableInfoDatabaseName = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
            this.tbTableInfoDatabaseAccess = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.tbTableInfoName = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.tbTableInfoID = new System.Windows.Forms.TextBox();
            this.label38 = new System.Windows.Forms.Label();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSchema = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnSynchronize
            // 
            this.btnSynchronize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnSynchronize.Location = new System.Drawing.Point(165, 133);
            this.btnSynchronize.Name = "btnSynchronize";
            this.btnSynchronize.Size = new System.Drawing.Size(73, 23);
            this.btnSynchronize.TabIndex = 8;
            this.btnSynchronize.Text = "Synchronize";
            this.btnSynchronize.UseVisualStyleBackColor = true;
            this.btnSynchronize.Click += new System.EventHandler(this.btnSynchronize_Click);
            // 
            // cbIsPrimaryExtractionTable
            // 
            this.cbIsPrimaryExtractionTable.AutoSize = true;
            this.cbIsPrimaryExtractionTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cbIsPrimaryExtractionTable.Location = new System.Drawing.Point(242, 8);
            this.cbIsPrimaryExtractionTable.Name = "cbIsPrimaryExtractionTable";
            this.cbIsPrimaryExtractionTable.Size = new System.Drawing.Size(151, 17);
            this.cbIsPrimaryExtractionTable.TabIndex = 1;
            this.cbIsPrimaryExtractionTable.Text = "Is Primary Extraction Table";
            this.cbIsPrimaryExtractionTable.UseVisualStyleBackColor = true;
            this.cbIsPrimaryExtractionTable.CheckedChanged += new System.EventHandler(this.cbIsPrimaryExtractionTable_CheckedChanged);
            // 
            // btnParameters
            // 
            this.btnParameters.Enabled = false;
            this.btnParameters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnParameters.Location = new System.Drawing.Point(244, 133);
            this.btnParameters.Name = "btnParameters";
            this.btnParameters.Size = new System.Drawing.Size(233, 23);
            this.btnParameters.TabIndex = 9;
            this.btnParameters.Text = "Default Table Valued Function Parameters...";
            this.btnParameters.UseVisualStyleBackColor = true;
            this.btnParameters.Click += new System.EventHandler(this.btnParameters_Click);
            // 
            // tbTableInfoDatabaseName
            // 
            this.tbTableInfoDatabaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTableInfoDatabaseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTableInfoDatabaseName.Location = new System.Drawing.Point(101, 80);
            this.tbTableInfoDatabaseName.Name = "tbTableInfoDatabaseName";
            this.tbTableInfoDatabaseName.Size = new System.Drawing.Size(674, 20);
            this.tbTableInfoDatabaseName.TabIndex = 5;
            this.tbTableInfoDatabaseName.TextChanged += new System.EventHandler(this.tbTableInfoDatabaseName_TextChanged);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.Location = new System.Drawing.Point(11, 83);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(84, 13);
            this.label51.TabIndex = 151;
            this.label51.Text = "Database Name";
            // 
            // tbTableInfoDatabaseAccess
            // 
            this.tbTableInfoDatabaseAccess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTableInfoDatabaseAccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTableInfoDatabaseAccess.Location = new System.Drawing.Point(101, 54);
            this.tbTableInfoDatabaseAccess.Name = "tbTableInfoDatabaseAccess";
            this.tbTableInfoDatabaseAccess.Size = new System.Drawing.Size(674, 20);
            this.tbTableInfoDatabaseAccess.TabIndex = 4;
            this.tbTableInfoDatabaseAccess.TextChanged += new System.EventHandler(this.tbTableInfoDatabaseAccess_TextChanged);
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label40.Location = new System.Drawing.Point(53, 57);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(38, 13);
            this.label40.TabIndex = 146;
            this.label40.Text = "Server";
            // 
            // tbTableInfoName
            // 
            this.tbTableInfoName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTableInfoName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTableInfoName.Location = new System.Drawing.Point(101, 28);
            this.tbTableInfoName.Name = "tbTableInfoName";
            this.tbTableInfoName.Size = new System.Drawing.Size(674, 20);
            this.tbTableInfoName.TabIndex = 3;
            this.tbTableInfoName.TextChanged += new System.EventHandler(this.tbTableInfoName_TextChanged);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label37.Location = new System.Drawing.Point(28, 31);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(65, 13);
            this.label37.TabIndex = 142;
            this.label37.Text = "Table Name";
            // 
            // tbTableInfoID
            // 
            this.tbTableInfoID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTableInfoID.Location = new System.Drawing.Point(101, 6);
            this.tbTableInfoID.Name = "tbTableInfoID";
            this.tbTableInfoID.ReadOnly = true;
            this.tbTableInfoID.Size = new System.Drawing.Size(134, 20);
            this.tbTableInfoID.TabIndex = 0;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.Location = new System.Drawing.Point(25, 9);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(66, 13);
            this.label38.TabIndex = 141;
            this.label38.Text = "TableInfo ID";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(399, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(45, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 151;
            this.label1.Text = "Schema";
            // 
            // tbSchema
            // 
            this.tbSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSchema.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbSchema.Location = new System.Drawing.Point(101, 106);
            this.tbSchema.Name = "tbSchema";
            this.tbSchema.Size = new System.Drawing.Size(188, 20);
            this.tbSchema.TabIndex = 6;
            this.tbSchema.TextChanged += new System.EventHandler(this.tbSchema_TextChanged);
            // 
            // TableInfoUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            
            this.Controls.Add(this.btnSynchronize);
            this.Controls.Add(this.label38);
            this.Controls.Add(this.cbIsPrimaryExtractionTable);
            this.Controls.Add(this.tbTableInfoID);
            this.Controls.Add(this.btnParameters);
            this.Controls.Add(this.label37);
            this.Controls.Add(this.tbSchema);
            this.Controls.Add(this.tbTableInfoDatabaseName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbTableInfoName);
            this.Controls.Add(this.label51);
            this.Controls.Add(this.label40);
            this.Controls.Add(this.tbTableInfoDatabaseAccess);
            this.Name = "TableInfoUI";
            this.Size = new System.Drawing.Size(780, 162);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbIsPrimaryExtractionTable;
        private System.Windows.Forms.TextBox tbTableInfoDatabaseName;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.TextBox tbTableInfoDatabaseAccess;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.TextBox tbTableInfoName;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.TextBox tbTableInfoID;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Button btnParameters;
        private System.Windows.Forms.Button btnSynchronize;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSchema;
    }
}
