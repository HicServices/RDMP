using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.PipelineUIs.DataObjects
{
    partial class DataFlowComponentVisualisation 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataFlowComponentVisualisation));
            this.pComponent = new System.Windows.Forms.Panel();
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.lblText = new System.Windows.Forms.Label();
            this.prongRight1 = new System.Windows.Forms.Panel();
            this.prongRight2 = new System.Windows.Forms.Panel();
            this.prongLeft1 = new System.Windows.Forms.Panel();
            this.prongLeft2 = new System.Windows.Forms.Panel();
            this.pbPadlock = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pbInsertHere = new System.Windows.Forms.PictureBox();
            this.pComponent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPadlock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbInsertHere)).BeginInit();
            this.SuspendLayout();
            // 
            // pComponent
            // 
            this.pComponent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pComponent.Controls.Add(this.ragSmiley1);
            this.pComponent.Controls.Add(this.lblText);
            this.pComponent.Location = new System.Drawing.Point(13, 5);
            this.pComponent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pComponent.Name = "pComponent";
            this.pComponent.Size = new System.Drawing.Size(202, 44);
            this.pComponent.TabIndex = 0;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(163, 7);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(29, 29);
            this.ragSmiley1.TabIndex = 1;
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(13, 13);
            this.lblText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(149, 17);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "label1";
            this.lblText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // prongRight1
            // 
            this.prongRight1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prongRight1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.prongRight1.Location = new System.Drawing.Point(207, 14);
            this.prongRight1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.prongRight1.Name = "prongRight1";
            this.prongRight1.Size = new System.Drawing.Size(17, 5);
            this.prongRight1.TabIndex = 1;
            // 
            // prongRight2
            // 
            this.prongRight2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prongRight2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.prongRight2.Location = new System.Drawing.Point(207, 35);
            this.prongRight2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.prongRight2.Name = "prongRight2";
            this.prongRight2.Size = new System.Drawing.Size(17, 5);
            this.prongRight2.TabIndex = 1;
            // 
            // prongLeft1
            // 
            this.prongLeft1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.prongLeft1.Location = new System.Drawing.Point(4, 14);
            this.prongLeft1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.prongLeft1.Name = "prongLeft1";
            this.prongLeft1.Size = new System.Drawing.Size(17, 5);
            this.prongLeft1.TabIndex = 2;
            // 
            // prongLeft2
            // 
            this.prongLeft2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.prongLeft2.Location = new System.Drawing.Point(4, 35);
            this.prongLeft2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.prongLeft2.Name = "prongLeft2";
            this.prongLeft2.Size = new System.Drawing.Size(17, 5);
            this.prongLeft2.TabIndex = 3;
            // 
            // pbPadlock
            // 
            this.pbPadlock.BackColor = System.Drawing.Color.Transparent;
            this.pbPadlock.Image = ((System.Drawing.Image)(resources.GetObject("pbPadlock.Image")));
            this.pbPadlock.Location = new System.Drawing.Point(18, 36);
            this.pbPadlock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbPadlock.Name = "pbPadlock";
            this.pbPadlock.Size = new System.Drawing.Size(22, 22);
            this.pbPadlock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbPadlock.TabIndex = 4;
            this.pbPadlock.TabStop = false;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Pass");
            this.imageList1.Images.SetKeyName(1, "Fail");
            this.imageList1.Images.SetKeyName(2, "Warning");
            this.imageList1.Images.SetKeyName(3, "FailedWithException");
            this.imageList1.Images.SetKeyName(4, "WarningWithException");
            // 
            // pbInsertHere
            // 
            this.pbInsertHere.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pbInsertHere.Image = ((System.Drawing.Image)(resources.GetObject("pbInsertHere.Image")));
            this.pbInsertHere.Location = new System.Drawing.Point(48, 40);
            this.pbInsertHere.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbInsertHere.Name = "pbInsertHere";
            this.pbInsertHere.Size = new System.Drawing.Size(139, 16);
            this.pbInsertHere.TabIndex = 5;
            this.pbInsertHere.TabStop = false;
            this.pbInsertHere.Visible = false;
            // 
            // DataFlowComponentVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbPadlock);
            this.Controls.Add(this.pbInsertHere);
            this.Controls.Add(this.prongRight1);
            this.Controls.Add(this.prongLeft1);
            this.Controls.Add(this.prongLeft2);
            this.Controls.Add(this.prongRight2);
            this.Controls.Add(this.pComponent);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DataFlowComponentVisualisation";
            this.Size = new System.Drawing.Size(233, 58);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DataFlowComponentVisualisation_DragEnter);
            this.DragLeave += new System.EventHandler(this.DataFlowComponentVisualisation_DragLeave);
            this.pComponent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbPadlock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbInsertHere)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pComponent;
        protected System.Windows.Forms.Panel prongRight1;
        protected System.Windows.Forms.Panel prongRight2;
        protected System.Windows.Forms.Panel prongLeft1;
        protected System.Windows.Forms.Panel prongLeft2;
        private System.Windows.Forms.PictureBox pbPadlock;
        protected System.Windows.Forms.Label lblText;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pbInsertHere;
        protected RAGSmiley ragSmiley1;
    }
}
