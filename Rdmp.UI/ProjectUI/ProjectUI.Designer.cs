using System.Windows.Forms;
using Rdmp.UI.CohortUI;
using Rdmp.UI.LocationsMenu.Ticketing;

namespace Rdmp.UI.ProjectUI
{
    partial class ProjectUI
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
            this.tbID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblExtractions = new System.Windows.Forms.Label();
            this.tbExtractionDirectory = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnShowExtractionDirectory = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tbProjectNumber = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.extractableCohortCollection1 = new Rdmp.UI.CohortUI.ExtractableCohortCollectionUI();
            this.tcMasterTicket = new Rdmp.UI.LocationsMenu.Ticketing.TicketingControlUI();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(130, 2);
            this.tbID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(116, 23);
            this.tbID.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(101, 6);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 15);
            this.label5.TabIndex = 25;
            this.label5.Text = "ID:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(130, 32);
            this.tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(465, 23);
            this.tbName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 23;
            this.label1.Text = "Project Name:";
            // 
            // lblExtractions
            // 
            this.lblExtractions.AutoSize = true;
            this.lblExtractions.Location = new System.Drawing.Point(5, 148);
            this.lblExtractions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExtractions.Name = "lblExtractions";
            this.lblExtractions.Size = new System.Drawing.Size(68, 15);
            this.lblExtractions.TabIndex = 9;
            this.lblExtractions.Text = "Extractions:";
            // 
            // tbExtractionDirectory
            // 
            this.tbExtractionDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExtractionDirectory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbExtractionDirectory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbExtractionDirectory.Location = new System.Drawing.Point(130, 63);
            this.tbExtractionDirectory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbExtractionDirectory.Name = "tbExtractionDirectory";
            this.tbExtractionDirectory.Size = new System.Drawing.Size(825, 23);
            this.tbExtractionDirectory.TabIndex = 2;
            this.tbExtractionDirectory.TextChanged += new System.EventHandler(this.tbExtractionDirectory_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 66);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 15);
            this.label4.TabIndex = 34;
            this.label4.Text = "Extraction Directory:";
            // 
            // btnShowExtractionDirectory
            // 
            this.btnShowExtractionDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowExtractionDirectory.Location = new System.Drawing.Point(1029, 62);
            this.btnShowExtractionDirectory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnShowExtractionDirectory.Name = "btnShowExtractionDirectory";
            this.btnShowExtractionDirectory.Size = new System.Drawing.Size(58, 25);
            this.btnShowExtractionDirectory.TabIndex = 4;
            this.btnShowExtractionDirectory.Text = "Show";
            this.btnShowExtractionDirectory.UseVisualStyleBackColor = true;
            this.btnShowExtractionDirectory.Click += new System.EventHandler(this.btnShowExtractionDirectory_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(28, 96);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 15);
            this.label7.TabIndex = 37;
            this.label7.Text = "Project Number:";
            // 
            // tbProjectNumber
            // 
            this.tbProjectNumber.Location = new System.Drawing.Point(130, 93);
            this.tbProjectNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbProjectNumber.Name = "tbProjectNumber";
            this.tbProjectNumber.Size = new System.Drawing.Size(116, 23);
            this.tbProjectNumber.TabIndex = 5;
            this.tbProjectNumber.TextChanged += new System.EventHandler(this.tbProjectNumber_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(4, 166);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.extractableCohortCollection1);
            this.splitContainer1.Size = new System.Drawing.Size(1084, 690);
            this.splitContainer1.SplitterDistance = 342;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 39;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.Size = new System.Drawing.Size(1080, 338);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            // 
            // extractableCohortCollection1
            // 
            this.extractableCohortCollection1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extractableCohortCollection1.Location = new System.Drawing.Point(0, 0);
            this.extractableCohortCollection1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.extractableCohortCollection1.Name = "extractableCohortCollection1";
            this.extractableCohortCollection1.Size = new System.Drawing.Size(1080, 339);
            this.extractableCohortCollection1.TabIndex = 0;
            // 
            // tcMasterTicket
            // 
            this.tcMasterTicket.AutoSize = true;
            this.tcMasterTicket.Location = new System.Drawing.Point(735, 92);
            this.tcMasterTicket.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.tcMasterTicket.Name = "tcMasterTicket";
            this.tcMasterTicket.Size = new System.Drawing.Size(351, 68);
            this.tcMasterTicket.TabIndex = 6;
            this.tcMasterTicket.TicketText = "";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(963, 62);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(58, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lblExtractions);
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Controls.Add(this.btnBrowse);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Controls.Add(this.tcMasterTicket);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.tbExtractionDirectory);
            this.panel1.Controls.Add(this.tbProjectNumber);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.btnShowExtractionDirectory);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1091, 860);
            this.panel1.TabIndex = 40;
            // 
            // ProjectUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ProjectUI";
            this.Size = new System.Drawing.Size(1091, 860);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private DataGridView dataGridView1;
        private System.Windows.Forms.Label lblExtractions;
        private System.Windows.Forms.TextBox tbExtractionDirectory;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnShowExtractionDirectory;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbProjectNumber;
        private TicketingControlUI tcMasterTicket;
        private CohortUI.ExtractableCohortCollectionUI extractableCohortCollection1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnBrowse;
        private Panel panel1;
    }
}
