using BrightIdeasSoftware;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups
{
    partial class LookupUI
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
            this.olvExtractionDescriptions = new BrightIdeasSoftware.ObjectListView();
            this.olvExtractionInformationName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCompositeJoins = new BrightIdeasSoftware.ObjectListView();
            this.olvCompositeJoinColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbPk = new System.Windows.Forms.TextBox();
            this.tbFk = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbTable = new System.Windows.Forms.PictureBox();
            this.tbTable = new System.Windows.Forms.TextBox();
            this.pbColumnInfo1 = new System.Windows.Forms.PictureBox();
            this.pbColumnInfo2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionDescriptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvCompositeJoins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbColumnInfo1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbColumnInfo2)).BeginInit();
            this.SuspendLayout();
            // 
            // olvExtractionDescriptions
            // 
            this.olvExtractionDescriptions.AllColumns.Add(this.olvExtractionInformationName);
            this.olvExtractionDescriptions.CellEditUseWholeCell = false;
            this.olvExtractionDescriptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvExtractionInformationName});
            this.olvExtractionDescriptions.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvExtractionDescriptions.Location = new System.Drawing.Point(6, 224);
            this.olvExtractionDescriptions.Name = "olvExtractionDescriptions";
            this.olvExtractionDescriptions.RowHeight = 19;
            this.olvExtractionDescriptions.ShowGroups = false;
            this.olvExtractionDescriptions.Size = new System.Drawing.Size(745, 257);
            this.olvExtractionDescriptions.TabIndex = 0;
            this.olvExtractionDescriptions.UseCompatibleStateImageBehavior = false;
            this.olvExtractionDescriptions.View = System.Windows.Forms.View.Details;
            this.olvExtractionDescriptions.ItemActivate += new System.EventHandler(this.olvExtractionDescriptions_ItemActivate);
            this.olvExtractionDescriptions.KeyUp += new System.Windows.Forms.KeyEventHandler(this.olv_KeyUp);
            // 
            // olvExtractionInformationName
            // 
            this.olvExtractionInformationName.AspectName = "ToString";
            this.olvExtractionInformationName.FillsFreeSpace = true;
            this.olvExtractionInformationName.Text = "All Descriptions Provided By Table";
            this.olvExtractionInformationName.MinimumWidth = 100;
            // 
            // olvCompositeJoins
            // 
            this.olvCompositeJoins.AllColumns.Add(this.olvCompositeJoinColumn);
            this.olvCompositeJoins.CellEditUseWholeCell = false;
            this.olvCompositeJoins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvCompositeJoinColumn});
            this.olvCompositeJoins.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvCompositeJoins.Location = new System.Drawing.Point(6, 33);
            this.olvCompositeJoins.Name = "olvCompositeJoins";
            this.olvCompositeJoins.RowHeight = 19;
            this.olvCompositeJoins.ShowGroups = false;
            this.olvCompositeJoins.Size = new System.Drawing.Size(745, 156);
            this.olvCompositeJoins.TabIndex = 0;
            this.olvCompositeJoins.UseCompatibleStateImageBehavior = false;
            this.olvCompositeJoins.View = System.Windows.Forms.View.Details;
            this.olvCompositeJoins.KeyUp += new System.Windows.Forms.KeyEventHandler(this.olv_KeyUp);
            // 
            // olvCompositeJoin
            // 
            this.olvCompositeJoinColumn.AspectName = "ToString";
            this.olvCompositeJoinColumn.FillsFreeSpace = true;
            this.olvCompositeJoinColumn.Text = "Composite Joins";
            this.olvCompositeJoinColumn.Width = 142;
            this.olvCompositeJoinColumn.MinimumWidth = 100;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Primary Key:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(410, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Foreign Key:";
            // 
            // tbPk
            // 
            this.tbPk.Location = new System.Drawing.Point(106, 11);
            this.tbPk.Name = "tbPk";
            this.tbPk.ReadOnly = true;
            this.tbPk.Size = new System.Drawing.Size(269, 20);
            this.tbPk.TabIndex = 3;
            // 
            // tbFk
            // 
            this.tbFk.Location = new System.Drawing.Point(482, 11);
            this.tbFk.Name = "tbFk";
            this.tbFk.ReadOnly = true;
            this.tbFk.Size = new System.Drawing.Size(269, 20);
            this.tbFk.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 201);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Lookup Table:";
            // 
            // pbTable
            // 
            this.pbTable.Location = new System.Drawing.Point(6, 195);
            this.pbTable.Name = "pbTable";
            this.pbTable.Size = new System.Drawing.Size(23, 23);
            this.pbTable.TabIndex = 4;
            this.pbTable.TabStop = false;
            // 
            // tbTable
            // 
            this.tbTable.Location = new System.Drawing.Point(117, 198);
            this.tbTable.Name = "tbTable";
            this.tbTable.ReadOnly = true;
            this.tbTable.Size = new System.Drawing.Size(269, 20);
            this.tbTable.TabIndex = 3;
            // 
            // pbColumnInfo1
            // 
            this.pbColumnInfo1.Location = new System.Drawing.Point(6, 7);
            this.pbColumnInfo1.Name = "pbColumnInfo1";
            this.pbColumnInfo1.Size = new System.Drawing.Size(23, 23);
            this.pbColumnInfo1.TabIndex = 4;
            this.pbColumnInfo1.TabStop = false;
            // 
            // pbColumnInfo2
            // 
            this.pbColumnInfo2.Location = new System.Drawing.Point(381, 8);
            this.pbColumnInfo2.Name = "pbColumnInfo2";
            this.pbColumnInfo2.Size = new System.Drawing.Size(23, 23);
            this.pbColumnInfo2.TabIndex = 4;
            this.pbColumnInfo2.TabStop = false;
            // 
            // LookupUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbColumnInfo2);
            this.Controls.Add(this.pbColumnInfo1);
            this.Controls.Add(this.pbTable);
            this.Controls.Add(this.tbFk);
            this.Controls.Add(this.tbTable);
            this.Controls.Add(this.tbPk);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.olvCompositeJoins);
            this.Controls.Add(this.olvExtractionDescriptions);
            this.Name = "LookupUI";
            this.Size = new System.Drawing.Size(764, 510);
            ((System.ComponentModel.ISupportInitialize)(this.olvExtractionDescriptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvCompositeJoins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbColumnInfo1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbColumnInfo2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvExtractionDescriptions;
        private OLVColumn olvExtractionInformationName;
        private ObjectListView olvCompositeJoins;
        private OLVColumn olvCompositeJoinColumn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbPk;
        private System.Windows.Forms.TextBox tbFk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbTable;
        private System.Windows.Forms.TextBox tbTable;
        private System.Windows.Forms.PictureBox pbColumnInfo1;
        private System.Windows.Forms.PictureBox pbColumnInfo2;
    }
}
