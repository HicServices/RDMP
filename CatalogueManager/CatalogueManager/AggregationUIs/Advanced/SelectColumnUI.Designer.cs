using BrightIdeasSoftware;

namespace CatalogueManager.AggregationUIs.Advanced
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
            this.olvColumnSQL = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvEditInPopup = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvAlias = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIncluded = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.olvAddRemove = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // olvSelectColumns
            // 
            this.olvSelectColumns.AllColumns.Add(this.olvAddRemove);
            this.olvSelectColumns.AllColumns.Add(this.olvColumnSQL);
            this.olvSelectColumns.AllColumns.Add(this.olvEditInPopup);
            this.olvSelectColumns.AllColumns.Add(this.olvAlias);
            this.olvSelectColumns.AllColumns.Add(this.olvIncluded);
            this.olvSelectColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvSelectColumns.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olvSelectColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvAddRemove,
            this.olvColumnSQL,
            this.olvEditInPopup,
            this.olvAlias});
            this.olvSelectColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvSelectColumns.Location = new System.Drawing.Point(3, 3);
            this.olvSelectColumns.Name = "olvSelectColumns";
            this.olvSelectColumns.RenderNonEditableCheckboxesAsDisabled = true;
            this.olvSelectColumns.RowHeight = 19;
            this.olvSelectColumns.ShowImagesOnSubItems = true;
            this.olvSelectColumns.Size = new System.Drawing.Size(793, 519);
            this.olvSelectColumns.TabIndex = 0;
            this.olvSelectColumns.UseCompatibleStateImageBehavior = false;
            this.olvSelectColumns.UseSubItemCheckBoxes = true;
            this.olvSelectColumns.View = System.Windows.Forms.View.Details;
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
            this.olvAlias.Width = 100;
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
            this.tbFilter.Location = new System.Drawing.Point(41, 528);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(755, 20);
            this.tbFilter.TabIndex = 1;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 531);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // olvAddRemove
            // 
            this.olvAddRemove.Groupable = false;
            this.olvAddRemove.Text = "+";
            this.olvAddRemove.Width = 22;
            // 
            // SelectColumnUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.olvSelectColumns);
            this.Name = "SelectColumnUI";
            this.Size = new System.Drawing.Size(799, 551);
            ((System.ComponentModel.ISupportInitialize)(this.olvSelectColumns)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvSelectColumns;
        private OLVColumn olvColumnSQL;
        private OLVColumn olvEditInPopup;
        private OLVColumn olvAlias;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private OLVColumn olvIncluded;
        private OLVColumn olvAddRemove;
    }
}
