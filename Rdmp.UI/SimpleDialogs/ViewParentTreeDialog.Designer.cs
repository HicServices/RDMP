namespace Rdmp.UI.SimpleDialogs
{
    partial class ViewParentTreeDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tlv = new BrightIdeasSoftware.TreeListView();
            tlvParentColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));

            SuspendLayout();

            this.tlvParentColumn.Text = "Tree";
            this.tlvParentColumn.Width = 100;
            this.tlvParentColumn.AspectName = "ToString";
            this.tlvParentColumn.ImageGetter += ImageGetter;
            this.tlv.CellClick += ClickHandler;
            ((System.ComponentModel.ISupportInitialize)tlv).BeginInit();
            this.tlv.AllColumns.Add(tlvParentColumn);
            this.tlv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.tlvParentColumn});
            // 
            // tlv
            // 
            tlv.Location = new System.Drawing.Point(12, 12);
            tlv.Name = "tlv";
            tlv.ShowGroups = false;
            tlv.Size = new System.Drawing.Size(418, 426);
            tlv.TabIndex = 0;
            tlv.View = System.Windows.Forms.View.Details;
            tlv.VirtualMode = true;
            // 
            // ViewParentTreeDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(449, 450);
            Controls.Add(tlv);
            Name = "ViewParentTreeDialog";
            Text = "Parent Tree";
            ((System.ComponentModel.ISupportInitialize)tlv).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlv;
        private BrightIdeasSoftware.OLVColumn tlvParentColumn;

    }
}