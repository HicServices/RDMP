namespace Rdmp.UI.SimpleDialogs
{
    partial class FindAndReplaceUI
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
            this.olvAllObjects = new BrightIdeasSoftware.FastObjectListView();
            this.olvObject = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvProperty = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.cbMatchCase = new System.Windows.Forms.CheckBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.tbReplace = new System.Windows.Forms.TextBox();
            this.tbFind = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rbSqlAttribute = new System.Windows.Forms.RadioButton();
            this.rbLocationsAttribute = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.olvAllObjects)).BeginInit();
            this.gbControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvAllObjects
            // 
            this.olvAllObjects.AllColumns.Add(this.olvObject);
            this.olvAllObjects.AllColumns.Add(this.olvProperty);
            this.olvAllObjects.AllColumns.Add(this.olvValue);
            this.olvAllObjects.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olvAllObjects.CellEditUseWholeCell = false;
            this.olvAllObjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvObject,
            this.olvProperty,
            this.olvValue});
            this.olvAllObjects.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvAllObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvAllObjects.Location = new System.Drawing.Point(0, 0);
            this.olvAllObjects.Name = "olvAllObjects";
            this.olvAllObjects.RowHeight = 19;
            this.olvAllObjects.ShowGroups = false;
            this.olvAllObjects.Size = new System.Drawing.Size(956, 687);
            this.olvAllObjects.TabIndex = 0;
            this.olvAllObjects.Text = "label1";
            this.olvAllObjects.UseCompatibleStateImageBehavior = false;
            this.olvAllObjects.UseFiltering = true;
            this.olvAllObjects.View = System.Windows.Forms.View.Details;
            this.olvAllObjects.VirtualMode = true;
            this.olvAllObjects.ItemActivate += new System.EventHandler(this.tlvAllObjects_ItemActivate);
            // 
            // olvObject
            // 
            this.olvObject.AspectName = "ToString";
            this.olvObject.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvObject.IsEditable = false;
            this.olvObject.Text = "Object";
            this.olvObject.Width = 314;
            // 
            // olvProperty
            // 
            this.olvProperty.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvProperty.IsEditable = false;
            this.olvProperty.Text = "Property";
            this.olvProperty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvProperty.Width = 174;
            // 
            // olvValue
            // 
            this.olvValue.CellEditUseWholeCell = true;
            this.olvValue.Text = "Value";
            this.olvValue.Width = 234;
            // 
            // gbControls
            // 
            this.gbControls.Controls.Add(this.cbMatchCase);
            this.gbControls.Controls.Add(this.btnFind);
            this.gbControls.Controls.Add(this.btnReplaceAll);
            this.gbControls.Controls.Add(this.tbReplace);
            this.gbControls.Controls.Add(this.tbFind);
            this.gbControls.Controls.Add(this.label3);
            this.gbControls.Controls.Add(this.label2);
            this.gbControls.Controls.Add(this.label1);
            this.gbControls.Controls.Add(this.rbSqlAttribute);
            this.gbControls.Controls.Add(this.rbLocationsAttribute);
            this.gbControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbControls.Location = new System.Drawing.Point(0, 687);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new System.Drawing.Size(956, 100);
            this.gbControls.TabIndex = 1;
            this.gbControls.TabStop = false;
            this.gbControls.Text = _showReplace ? "Find and Replace" : "Find";
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point(600, 16);
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size(83, 17);
            this.cbMatchCase.TabIndex = 4;
            this.cbMatchCase.Text = "Match Case";
            this.cbMatchCase.UseVisualStyleBackColor = true;
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(518, 11);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(75, 23);
            this.btnFind.TabIndex = 3;
            this.btnFind.Text = "Find All";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Location = new System.Drawing.Point(801, 40);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(75, 23);
            this.btnReplaceAll.TabIndex = 8;
            this.btnReplaceAll.Text = "Replace All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            this.btnReplaceAll.Visible = _showReplace;
            // 
            // tbReplace
            // 
            this.tbReplace.Location = new System.Drawing.Point(353, 42);
            this.tbReplace.Name = "tbReplace";
            this.tbReplace.Size = new System.Drawing.Size(442, 20);
            this.tbReplace.TabIndex = 7;
            this.tbReplace.Visible = _showReplace;
            // 
            // tbFind
            // 
            this.tbFind.Location = new System.Drawing.Point(353, 13);
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size(159, 20);
            this.tbFind.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(514, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "(This Cannot be undone!)";
            this.label3.Visible = _showReplace;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(297, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Replace:";
            this.label2.Visible = _showReplace;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(297, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Find what:";
            // 
            // rbSqlAttribute
            // 
            this.rbSqlAttribute.AutoSize = true;
            this.rbSqlAttribute.Location = new System.Drawing.Point(16, 43);
            this.rbSqlAttribute.Name = "rbSqlAttribute";
            this.rbSqlAttribute.Size = new System.Drawing.Size(40, 17);
            this.rbSqlAttribute.TabIndex = 5;
            this.rbSqlAttribute.TabStop = true;
            this.rbSqlAttribute.Text = "Sql";
            this.rbSqlAttribute.UseVisualStyleBackColor = true;
            this.rbSqlAttribute.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // rbLocationsAttribute
            // 
            this.rbLocationsAttribute.AutoSize = true;
            this.rbLocationsAttribute.Checked = true;
            this.rbLocationsAttribute.Location = new System.Drawing.Point(16, 20);
            this.rbLocationsAttribute.Name = "rbLocationsAttribute";
            this.rbLocationsAttribute.Size = new System.Drawing.Size(172, 17);
            this.rbLocationsAttribute.TabIndex = 0;
            this.rbLocationsAttribute.TabStop = true;
            this.rbLocationsAttribute.Text = "Physical Locations Referenced";
            this.rbLocationsAttribute.UseVisualStyleBackColor = true;
            this.rbLocationsAttribute.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // FindAndReplaceUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.olvAllObjects);
            this.Controls.Add(this.gbControls);
            this.Name = "FindAndReplaceUI";
            this.Size = new System.Drawing.Size(956, 787);
            ((System.ComponentModel.ISupportInitialize)(this.olvAllObjects)).EndInit();
            this.gbControls.ResumeLayout(false);
            this.gbControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView olvAllObjects;
        private BrightIdeasSoftware.OLVColumn olvObject;
        private BrightIdeasSoftware.OLVColumn olvProperty;
        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.RadioButton rbSqlAttribute;
        private System.Windows.Forms.RadioButton rbLocationsAttribute;
        private BrightIdeasSoftware.OLVColumn olvValue;
        private System.Windows.Forms.TextBox tbReplace;
        private System.Windows.Forms.TextBox tbFind;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.CheckBox cbMatchCase;
    }
}
