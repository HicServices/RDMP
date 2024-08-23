using BrightIdeasSoftware;

namespace Rdmp.UI.AggregationUIs.Advanced
{
    partial class SelectColumnUI
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

            if(!IsDisposed)
                base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.olvSelectColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvAddRemove = new BrightIdeasSoftware.OLVColumn();
            this.olvColumnSQL = new BrightIdeasSoftware.OLVColumn();
            this.olvEditInPopup = new BrightIdeasSoftware.OLVColumn();
            this.olvAlias = new BrightIdeasSoftware.OLVColumn();
            this.olvGroupBy = new BrightIdeasSoftware.OLVColumn();
            this.olvIncluded = new BrightIdeasSoftware.OLVColumn();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // olvSelectColumns
            // 
            this.olvSelectColumns.AllColumns.Add(this.olvAddRemove);
            this.olvSelectColumns.AllColumns.Add(this.olvColumnSQL);
            this.olvSelectColumns.AllColumns.Add(this.olvEditInPopup);
            this.olvSelectColumns.AllColumns.Add(this.olvAlias);
            this.olvSelectColumns.AllColumns.Add(this.olvGroupBy);
            this.olvSelectColumns.AllColumns.Add(this.olvIncluded);
            this.olvSelectColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvSelectColumns.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olvSelectColumns.CellEditUseWholeCell = false;
            this.olvSelectColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvAddRemove,
            this.olvColumnSQL,
            this.olvEditInPopup,
            this.olvAlias,this.olvGroupBy});
            this.olvSelectColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvSelectColumns.HideSelection = false;
            this.olvSelectColumns.Location = new System.Drawing.Point(0, 3);
            this.olvSelectColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvSelectColumns.Name = "olvSelectColumns";
            this.olvSelectColumns.RenderNonEditableCheckboxesAsDisabled = true;
            this.olvSelectColumns.RowHeight = 19;
            this.olvSelectColumns.ShowImagesOnSubItems = true;
            this.olvSelectColumns.Size = new System.Drawing.Size(817, 598);
            this.olvSelectColumns.TabIndex = 0;
            this.olvSelectColumns.UseCompatibleStateImageBehavior = false;
            this.olvSelectColumns.UseSubItemCheckBoxes = true;
            this.olvSelectColumns.View = System.Windows.Forms.View.Details;
            // 
            // olvAddRemove
            // 
            this.olvAddRemove.Groupable = false;
            this.olvAddRemove.Text = "+";
            this.olvAddRemove.Width = 22;
            // 
            // olvColumnSQL
            // 
            this.olvColumnSQL.AspectName = "SelectSQL";
            this.olvColumnSQL.Groupable = false;
            this.olvColumnSQL.Text = "Column";
            this.olvColumnSQL.Width = 300;
            // 
            // olvEditInPopup
            // 
            this.olvEditInPopup.Groupable = false;
            this.olvEditInPopup.IsButton = true;
            this.olvEditInPopup.Text = "";
            // 
            // olvAlias
            // 
            this.olvAlias.AspectName = "Alias";
            this.olvAlias.Groupable = false;
            this.olvAlias.Text = "Alias";
            this.olvAlias.Width = 100;  // 
            // olvGroupBy
            // 
            this.olvGroupBy.AspectName = "GroupBy";
            this.olvGroupBy.Groupable = true;
            this.olvGroupBy.CheckBoxes = true;
            this.olvGroupBy.Text = "Group By";
            this.olvGroupBy.Width = 100;

            // 
            // olvIncluded
            // 
            this.olvIncluded.DisplayIndex = 3;
            this.olvIncluded.IsVisible = false;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(48, 609);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(769, 23);
            this.tbFilter.TabIndex = 1;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 613);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // SelectColumnUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.olvSelectColumns);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SelectColumnUI";
            this.Size = new System.Drawing.Size(821, 636);
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectColumns)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvSelectColumns;
        private OLVColumn olvColumnSQL;
        private OLVColumn olvEditInPopup;
        private OLVColumn olvAlias;
        private OLVColumn olvGroupBy;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private OLVColumn olvIncluded;
        private OLVColumn olvAddRemove;
    }
}
