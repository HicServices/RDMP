using BrightIdeasSoftware;

namespace CatalogueManager.AggregationUIs.Advanced
{
    partial class AggregateEditor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregateEditor));
            this.gbDescription = new System.Windows.Forms.GroupBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblFromTable = new System.Windows.Forms.Label();
            this.cbExtractable = new System.Windows.Forms.CheckBox();
            this.gbAxis = new System.Windows.Forms.GroupBox();
            this.btnClearAxis = new System.Windows.Forms.Button();
            this.ddAxisDimension = new System.Windows.Forms.ComboBox();
            this.gbPivot = new System.Windows.Forms.GroupBox();
            this.btnClearPivotDimension = new System.Windows.Forms.Button();
            this.ddPivotDimension = new System.Windows.Forms.ComboBox();
            this.gbHaving = new System.Windows.Forms.GroupBox();
            this.olvJoin = new BrightIdeasSoftware.ObjectListView();
            this.olvJoinTableName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvJoinDirection = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblParams = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnViewQuery = new System.Windows.Forms.Button();
            this.btnParameters = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this._aggregateTopXui1 = new CatalogueManager.AggregationUIs.Advanced.AggregateTopXUI();
            this.selectColumnUI1 = new CatalogueManager.AggregationUIs.Advanced.SelectColumnUI();
            this.aggregateContinuousDateAxisUI1 = new CatalogueManager.AggregationUIs.AggregateContinuousDateAxisUI();
            this.btnShow = new System.Windows.Forms.Button();
            this.gbDescription.SuspendLayout();
            this.gbAxis.SuspendLayout();
            this.gbPivot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // gbDescription
            // 
            this.gbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDescription.Controls.Add(this.tbDescription);
            this.gbDescription.Location = new System.Drawing.Point(4, 790);
            this.gbDescription.Name = "gbDescription";
            this.gbDescription.Size = new System.Drawing.Size(700, 107);
            this.gbDescription.TabIndex = 16;
            this.gbDescription.TabStop = false;
            this.gbDescription.Text = "Description";
            // 
            // tbDescription
            // 
            this.tbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDescription.Location = new System.Drawing.Point(3, 16);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(694, 88);
            this.tbDescription.TabIndex = 0;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(128, 31);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(69, 20);
            this.tbID.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(104, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "ID";
            // 
            // lblFromTable
            // 
            this.lblFromTable.AutoSize = true;
            this.lblFromTable.Location = new System.Drawing.Point(45, 380);
            this.lblFromTable.Name = "lblFromTable";
            this.lblFromTable.Size = new System.Drawing.Size(10, 13);
            this.lblFromTable.TabIndex = 11;
            this.lblFromTable.Text = "-";
            // 
            // cbExtractable
            // 
            this.cbExtractable.AutoSize = true;
            this.cbExtractable.Location = new System.Drawing.Point(203, 33);
            this.cbExtractable.Name = "cbExtractable";
            this.cbExtractable.Size = new System.Drawing.Size(79, 17);
            this.cbExtractable.TabIndex = 3;
            this.cbExtractable.Text = "Extractable";
            this.cbExtractable.UseVisualStyleBackColor = true;
            this.cbExtractable.CheckedChanged += new System.EventHandler(this.cbExtractable_CheckedChanged);
            // 
            // gbAxis
            // 
            this.gbAxis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbAxis.Controls.Add(this.btnClearAxis);
            this.gbAxis.Controls.Add(this.ddAxisDimension);
            this.gbAxis.Controls.Add(this.aggregateContinuousDateAxisUI1);
            this.gbAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbAxis.ForeColor = System.Drawing.Color.Blue;
            this.gbAxis.Location = new System.Drawing.Point(3, 609);
            this.gbAxis.Name = "gbAxis";
            this.gbAxis.Size = new System.Drawing.Size(700, 100);
            this.gbAxis.TabIndex = 14;
            this.gbAxis.TabStop = false;
            this.gbAxis.Text = "AXIS";
            // 
            // btnClearAxis
            // 
            this.btnClearAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearAxis.ForeColor = System.Drawing.Color.Black;
            this.btnClearAxis.Location = new System.Drawing.Point(602, 17);
            this.btnClearAxis.Name = "btnClearAxis";
            this.btnClearAxis.Size = new System.Drawing.Size(75, 23);
            this.btnClearAxis.TabIndex = 1;
            this.btnClearAxis.Text = "Clear";
            this.btnClearAxis.UseVisualStyleBackColor = true;
            this.btnClearAxis.Click += new System.EventHandler(this.btnClearAxis_Click);
            // 
            // ddAxisDimension
            // 
            this.ddAxisDimension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddAxisDimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAxisDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddAxisDimension.FormattingEnabled = true;
            this.ddAxisDimension.Location = new System.Drawing.Point(15, 19);
            this.ddAxisDimension.Name = "ddAxisDimension";
            this.ddAxisDimension.Size = new System.Drawing.Size(581, 21);
            this.ddAxisDimension.TabIndex = 0;
            this.ddAxisDimension.SelectedIndexChanged += new System.EventHandler(this.ddAxisDimension_SelectedIndexChanged);
            // 
            // gbPivot
            // 
            this.gbPivot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPivot.Controls.Add(this.btnClearPivotDimension);
            this.gbPivot.Controls.Add(this.ddPivotDimension);
            this.gbPivot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbPivot.ForeColor = System.Drawing.Color.Blue;
            this.gbPivot.Location = new System.Drawing.Point(4, 715);
            this.gbPivot.Name = "gbPivot";
            this.gbPivot.Size = new System.Drawing.Size(700, 65);
            this.gbPivot.TabIndex = 15;
            this.gbPivot.TabStop = false;
            this.gbPivot.Text = "PIVOT";
            // 
            // btnClearPivotDimension
            // 
            this.btnClearPivotDimension.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearPivotDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearPivotDimension.ForeColor = System.Drawing.Color.Black;
            this.btnClearPivotDimension.Location = new System.Drawing.Point(602, 19);
            this.btnClearPivotDimension.Name = "btnClearPivotDimension";
            this.btnClearPivotDimension.Size = new System.Drawing.Size(75, 23);
            this.btnClearPivotDimension.TabIndex = 1;
            this.btnClearPivotDimension.Text = "Clear";
            this.btnClearPivotDimension.UseVisualStyleBackColor = true;
            this.btnClearPivotDimension.Click += new System.EventHandler(this.btnClearPivotDimension_Click);
            // 
            // ddPivotDimension
            // 
            this.ddPivotDimension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddPivotDimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPivotDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddPivotDimension.FormattingEnabled = true;
            this.ddPivotDimension.Location = new System.Drawing.Point(15, 19);
            this.ddPivotDimension.Name = "ddPivotDimension";
            this.ddPivotDimension.Size = new System.Drawing.Size(581, 21);
            this.ddPivotDimension.Sorted = true;
            this.ddPivotDimension.TabIndex = 0;
            this.ddPivotDimension.SelectedIndexChanged += new System.EventHandler(this.ddPivotDimension_SelectedIndexChanged);
            // 
            // gbHaving
            // 
            this.gbHaving.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbHaving.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbHaving.ForeColor = System.Drawing.Color.Blue;
            this.gbHaving.Location = new System.Drawing.Point(3, 504);
            this.gbHaving.Name = "gbHaving";
            this.gbHaving.Size = new System.Drawing.Size(700, 99);
            this.gbHaving.TabIndex = 13;
            this.gbHaving.TabStop = false;
            this.gbHaving.Text = "HAVING";
            // 
            // olvJoin
            // 
            this.olvJoin.AllColumns.Add(this.olvJoinTableName);
            this.olvJoin.AllColumns.Add(this.olvJoinDirection);
            this.olvJoin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvJoin.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olvJoin.CellEditUseWholeCell = false;
            this.olvJoin.CheckBoxes = true;
            this.olvJoin.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvJoinTableName,
            this.olvJoinDirection});
            this.olvJoin.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvJoin.FullRowSelect = true;
            this.olvJoin.IsSimpleDropSink = true;
            this.olvJoin.Location = new System.Drawing.Point(4, 396);
            this.olvJoin.Name = "olvJoin";
            this.olvJoin.Size = new System.Drawing.Size(699, 106);
            this.olvJoin.TabIndex = 12;
            this.olvJoin.UseCompatibleStateImageBehavior = false;
            this.olvJoin.View = System.Windows.Forms.View.Details;
            this.olvJoin.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.olvAny_CellEditFinishing);
            this.olvJoin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnListboxKeyUp);
            // 
            // olvJoinTableName
            // 
            this.olvJoinTableName.AspectName = "ToString";
            this.olvJoinTableName.FillsFreeSpace = true;
            this.olvJoinTableName.Groupable = false;
            this.olvJoinTableName.IsEditable = false;
            this.olvJoinTableName.Text = "Table Name";
            this.olvJoinTableName.Width = 163;
            // 
            // olvJoinDirection
            // 
            this.olvJoinDirection.AspectName = "JoinType";
            this.olvJoinDirection.Groupable = false;
            this.olvJoinDirection.Text = "Join Direction";
            this.olvJoinDirection.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(1, 380);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "FROM";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(6, 149);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "SELECT";
            // 
            // lblParams
            // 
            this.lblParams.AutoSize = true;
            this.lblParams.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.lblParams.Location = new System.Drawing.Point(121, 100);
            this.lblParams.Name = "lblParams";
            this.lblParams.Size = new System.Drawing.Size(60, 13);
            this.lblParams.TabIndex = 16;
            this.lblParams.Text = "Parameters";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Warning.png");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.label5.Location = new System.Drawing.Point(179, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "View Query";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(75, 75);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // btnViewQuery
            // 
            this.btnViewQuery.Image = ((System.Drawing.Image)(resources.GetObject("btnViewQuery.Image")));
            this.btnViewQuery.Location = new System.Drawing.Point(187, 57);
            this.btnViewQuery.Name = "btnViewQuery";
            this.btnViewQuery.Size = new System.Drawing.Size(45, 45);
            this.btnViewQuery.TabIndex = 6;
            this.btnViewQuery.UseVisualStyleBackColor = true;
            this.btnViewQuery.Click += new System.EventHandler(this.btnViewQuery_Click);
            // 
            // btnParameters
            // 
            this.btnParameters.Image = ((System.Drawing.Image)(resources.GetObject("btnParameters.Image")));
            this.btnParameters.Location = new System.Drawing.Point(128, 57);
            this.btnParameters.Name = "btnParameters";
            this.btnParameters.Size = new System.Drawing.Size(45, 45);
            this.btnParameters.TabIndex = 5;
            this.btnParameters.UseVisualStyleBackColor = true;
            this.btnParameters.Click += new System.EventHandler(this.btnParameters_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(6, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "TOP ";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(128, 7);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 13);
            this.lblName.TabIndex = 25;
            this.lblName.Text = "lblName";
            // 
            // _aggregateTopXui1
            // 
            this._aggregateTopXui1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._aggregateTopXui1.Location = new System.Drawing.Point(7, 120);
            this._aggregateTopXui1.Name = "_aggregateTopXui1";
            this._aggregateTopXui1.Size = new System.Drawing.Size(683, 28);
            this._aggregateTopXui1.TabIndex = 24;
            // 
            // selectColumnUI1
            // 
            this.selectColumnUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectColumnUI1.Location = new System.Drawing.Point(6, 161);
            this.selectColumnUI1.Name = "selectColumnUI1";
            this.selectColumnUI1.Size = new System.Drawing.Size(696, 216);
            this.selectColumnUI1.TabIndex = 9;
            // 
            // aggregateContinuousDateAxisUI1
            // 
            this.aggregateContinuousDateAxisUI1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.aggregateContinuousDateAxisUI1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.aggregateContinuousDateAxisUI1.Location = new System.Drawing.Point(15, 46);
            this.aggregateContinuousDateAxisUI1.Name = "aggregateContinuousDateAxisUI1";
            this.aggregateContinuousDateAxisUI1.Size = new System.Drawing.Size(633, 48);
            this.aggregateContinuousDateAxisUI1.TabIndex = 2;
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(187, 3);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(45, 22);
            this.btnShow.TabIndex = 0;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // AggregateEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this._aggregateTopXui1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnViewQuery);
            this.Controls.Add(this.gbDescription);
            this.Controls.Add(this.selectColumnUI1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblParams);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnParameters);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblFromTable);
            this.Controls.Add(this.cbExtractable);
            this.Controls.Add(this.olvJoin);
            this.Controls.Add(this.gbHaving);
            this.Controls.Add(this.gbPivot);
            this.Controls.Add(this.gbAxis);
            this.Name = "AggregateEditor";
            this.Size = new System.Drawing.Size(706, 900);
            this.gbDescription.ResumeLayout(false);
            this.gbDescription.PerformLayout();
            this.gbAxis.ResumeLayout(false);
            this.gbPivot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ObjectListView olvJoin;
        private OLVColumn olvJoinTableName;
        private OLVColumn olvJoinDirection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbPivot;
        private System.Windows.Forms.GroupBox gbHaving;
        private System.Windows.Forms.GroupBox gbAxis;
        private AggregateContinuousDateAxisUI aggregateContinuousDateAxisUI1;
        private System.Windows.Forms.CheckBox cbExtractable;
        private System.Windows.Forms.Label lblFromTable;
        private System.Windows.Forms.Button btnClearPivotDimension;
        private System.Windows.Forms.ComboBox ddPivotDimension;
        private System.Windows.Forms.ComboBox ddAxisDimension;
        private System.Windows.Forms.Button btnClearAxis;
        private System.Windows.Forms.Button btnParameters;
        private System.Windows.Forms.Label lblParams;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.TextBox tbDescription;
        private SelectColumnUI selectColumnUI1;
        private System.Windows.Forms.Button btnViewQuery;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private AggregateTopXUI _aggregateTopXui1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnShow;
    }
}
