using BrightIdeasSoftware;

namespace Rdmp.UI.AggregationUIs.Advanced
{
    partial class AggregateEditorUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregateEditorUI));
            this.gbDescription = new System.Windows.Forms.GroupBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblFromTable = new System.Windows.Forms.Label();
            this.cbExtractable = new System.Windows.Forms.CheckBox();
            this.gbAxis = new System.Windows.Forms.GroupBox();
            this.btnClearAxis = new System.Windows.Forms.Button();
            this.ddAxisDimension = new System.Windows.Forms.ComboBox();
            this.aggregateContinuousDateAxisUI1 = new Rdmp.UI.AggregationUIs.AggregateContinuousDateAxisUI();
            this.gbPivot = new System.Windows.Forms.GroupBox();
            this.btnClearPivotDimension = new System.Windows.Forms.Button();
            this.ddPivotDimension = new System.Windows.Forms.ComboBox();
            this.gbHaving = new System.Windows.Forms.GroupBox();
            this.olvJoin = new BrightIdeasSoftware.ObjectListView();
            this.olvJoinTableName = new BrightIdeasSoftware.OLVColumn();
            this.olvJoinDirection = new BrightIdeasSoftware.OLVColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this._aggregateTopXui1 = new Rdmp.UI.AggregationUIs.Advanced.AggregateTopXUI();
            this.selectColumnUI1 = new Rdmp.UI.AggregationUIs.Advanced.SelectColumnUI();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gbDescription.SuspendLayout();
            this.gbAxis.SuspendLayout();
            this.gbPivot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDescription
            // 
            this.gbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDescription.Controls.Add(this.tbDescription);
            this.gbDescription.Location = new System.Drawing.Point(4, 788);
            this.gbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbDescription.Name = "gbDescription";
            this.gbDescription.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbDescription.Size = new System.Drawing.Size(808, 87);
            this.gbDescription.TabIndex = 16;
            this.gbDescription.TabStop = false;
            this.gbDescription.Text = "Description";
            // 
            // tbDescription
            // 
            this.tbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDescription.Location = new System.Drawing.Point(4, 19);
            this.tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(800, 65);
            this.tbDescription.TabIndex = 0;
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(156, 49);
            this.tbID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(80, 23);
            this.tbID.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(128, 52);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 15);
            this.label4.TabIndex = 17;
            this.label4.Text = "ID";
            // 
            // lblFromTable
            // 
            this.lblFromTable.AutoSize = true;
            this.lblFromTable.Location = new System.Drawing.Point(45, 329);
            this.lblFromTable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFromTable.Name = "lblFromTable";
            this.lblFromTable.Size = new System.Drawing.Size(12, 15);
            this.lblFromTable.TabIndex = 11;
            this.lblFromTable.Text = "-";
            // 
            // cbExtractable
            // 
            this.cbExtractable.AutoSize = true;
            this.cbExtractable.Location = new System.Drawing.Point(244, 51);
            this.cbExtractable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbExtractable.Name = "cbExtractable";
            this.cbExtractable.Size = new System.Drawing.Size(84, 19);
            this.cbExtractable.TabIndex = 3;
            this.cbExtractable.Text = "Extractable";
            this.cbExtractable.UseVisualStyleBackColor = true;
            // 
            // gbAxis
            // 
            this.gbAxis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbAxis.Controls.Add(this.btnClearAxis);
            this.gbAxis.Controls.Add(this.ddAxisDimension);
            this.gbAxis.Controls.Add(this.aggregateContinuousDateAxisUI1);
            this.gbAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.gbAxis.ForeColor = System.Drawing.Color.Blue;
            this.gbAxis.Location = new System.Drawing.Point(5, 600);
            this.gbAxis.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbAxis.Name = "gbAxis";
            this.gbAxis.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbAxis.Size = new System.Drawing.Size(807, 115);
            this.gbAxis.TabIndex = 14;
            this.gbAxis.TabStop = false;
            this.gbAxis.Text = "AXIS";
            // 
            // btnClearAxis
            // 
            this.btnClearAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnClearAxis.ForeColor = System.Drawing.Color.Black;
            this.btnClearAxis.Location = new System.Drawing.Point(711, 20);
            this.btnClearAxis.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearAxis.Name = "btnClearAxis";
            this.btnClearAxis.Size = new System.Drawing.Size(88, 25);
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
            this.ddAxisDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ddAxisDimension.FormattingEnabled = true;
            this.ddAxisDimension.Location = new System.Drawing.Point(8, 22);
            this.ddAxisDimension.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddAxisDimension.Name = "ddAxisDimension";
            this.ddAxisDimension.Size = new System.Drawing.Size(695, 21);
            this.ddAxisDimension.TabIndex = 0;
            this.ddAxisDimension.SelectedIndexChanged += new System.EventHandler(this.ddAxisDimension_SelectedIndexChanged);
            // 
            // aggregateContinuousDateAxisUI1
            // 
            this.aggregateContinuousDateAxisUI1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.aggregateContinuousDateAxisUI1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.aggregateContinuousDateAxisUI1.Location = new System.Drawing.Point(8, 53);
            this.aggregateContinuousDateAxisUI1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.aggregateContinuousDateAxisUI1.Name = "aggregateContinuousDateAxisUI1";
            this.aggregateContinuousDateAxisUI1.Size = new System.Drawing.Size(791, 55);
            this.aggregateContinuousDateAxisUI1.TabIndex = 2;
            // 
            // gbPivot
            // 
            this.gbPivot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPivot.Controls.Add(this.btnClearPivotDimension);
            this.gbPivot.Controls.Add(this.ddPivotDimension);
            this.gbPivot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.gbPivot.ForeColor = System.Drawing.Color.Blue;
            this.gbPivot.Location = new System.Drawing.Point(5, 722);
            this.gbPivot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPivot.Name = "gbPivot";
            this.gbPivot.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPivot.Size = new System.Drawing.Size(807, 60);
            this.gbPivot.TabIndex = 15;
            this.gbPivot.TabStop = false;
            this.gbPivot.Text = "PIVOT";
            // 
            // btnClearPivotDimension
            // 
            this.btnClearPivotDimension.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearPivotDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnClearPivotDimension.ForeColor = System.Drawing.Color.Black;
            this.btnClearPivotDimension.Location = new System.Drawing.Point(711, 19);
            this.btnClearPivotDimension.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearPivotDimension.Name = "btnClearPivotDimension";
            this.btnClearPivotDimension.Size = new System.Drawing.Size(88, 25);
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
            this.ddPivotDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ddPivotDimension.FormattingEnabled = true;
            this.ddPivotDimension.Location = new System.Drawing.Point(8, 22);
            this.ddPivotDimension.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddPivotDimension.Name = "ddPivotDimension";
            this.ddPivotDimension.Size = new System.Drawing.Size(692, 21);
            this.ddPivotDimension.Sorted = true;
            this.ddPivotDimension.TabIndex = 0;
            this.ddPivotDimension.SelectedIndexChanged += new System.EventHandler(this.ddPivotDimension_SelectedIndexChanged);
            // 
            // gbHaving
            // 
            this.gbHaving.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbHaving.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.gbHaving.ForeColor = System.Drawing.Color.Blue;
            this.gbHaving.Location = new System.Drawing.Point(5, 479);
            this.gbHaving.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbHaving.Name = "gbHaving";
            this.gbHaving.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbHaving.Size = new System.Drawing.Size(807, 114);
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
            this.olvJoin.HideSelection = false;
            this.olvJoin.IsSimpleDropSink = true;
            this.olvJoin.Location = new System.Drawing.Point(5, 350);
            this.olvJoin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvJoin.Name = "olvJoin";
            this.olvJoin.RowHeight = 19;
            this.olvJoin.Size = new System.Drawing.Size(807, 122);
            this.olvJoin.TabIndex = 12;
            this.olvJoin.UseCompatibleStateImageBehavior = false;
            this.olvJoin.View = System.Windows.Forms.View.Details;
            this.olvJoin.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.olvAny_CellEditFinishing);
            this.olvJoin.ItemActivate += new System.EventHandler(this.olvJoin_ItemActivate);
            // 
            // olvJoinTableName
            // 
            this.olvJoinTableName.AspectName = "ToString";
            this.olvJoinTableName.FillsFreeSpace = true;
            this.olvJoinTableName.Groupable = false;
            this.olvJoinTableName.IsEditable = false;
            this.olvJoinTableName.MinimumWidth = 100;
            this.olvJoinTableName.Text = "Table Name";
            this.olvJoinTableName.Width = 163;
            // 
            // olvJoinDirection
            // 
            this.olvJoinDirection.Groupable = false;
            this.olvJoinDirection.Text = "Join Direction";
            this.olvJoinDirection.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(1, 331);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "FROM";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(5, 64);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "SELECT";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Warning.png");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 3);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(88, 87);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(5, 13);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "TOP ";
            // 
            // _aggregateTopXui1
            // 
            this._aggregateTopXui1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._aggregateTopXui1.Location = new System.Drawing.Point(5, 29);
            this._aggregateTopXui1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._aggregateTopXui1.Name = "_aggregateTopXui1";
            this._aggregateTopXui1.Size = new System.Drawing.Size(803, 32);
            this._aggregateTopXui1.TabIndex = 24;
            // 
            // selectColumnUI1
            // 
            this.selectColumnUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectColumnUI1.Location = new System.Drawing.Point(5, 78);
            this.selectColumnUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.selectColumnUI1.Name = "selectColumnUI1";
            this.selectColumnUI1.Size = new System.Drawing.Size(807, 248);
            this.selectColumnUI1.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this._aggregateTopXui1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.selectColumnUI1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.olvJoin);
            this.panel1.Controls.Add(this.gbDescription);
            this.panel1.Controls.Add(this.lblFromTable);
            this.panel1.Controls.Add(this.gbHaving);
            this.panel1.Controls.Add(this.gbAxis);
            this.panel1.Controls.Add(this.gbPivot);
            this.panel1.Location = new System.Drawing.Point(4, 90);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(817, 877);
            this.panel1.TabIndex = 27;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(156, 19);
            this.tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(550, 23);
            this.tbName.TabIndex = 28;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(108, 22);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 29;
            this.label6.Text = "Name";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.cbExtractable);
            this.panel2.Controls.Add(this.tbName);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.tbID);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(824, 971);
            this.panel2.TabIndex = 30;
            // 
            // AggregateEditorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "AggregateEditorUI";
            this.Size = new System.Drawing.Size(824, 971);
            this.gbDescription.ResumeLayout(false);
            this.gbDescription.PerformLayout();
            this.gbAxis.ResumeLayout(false);
            this.gbPivot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvJoin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

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
        internal System.Windows.Forms.ComboBox ddPivotDimension;
        internal System.Windows.Forms.ComboBox ddAxisDimension;
        private System.Windows.Forms.Button btnClearAxis;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.TextBox tbDescription;
        internal SelectColumnUI selectColumnUI1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private AggregateTopXUI _aggregateTopXui1;
        
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel2;
    }
}
