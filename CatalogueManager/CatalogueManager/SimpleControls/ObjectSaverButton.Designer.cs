namespace CatalogueManager.SimpleControls
{
    partial class ObjectSaverButton
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

            if (_refreshBus != null)
                _refreshBus.Unsubscribe(this);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectSaverButton));
            this.btnSave = new System.Windows.Forms.Button();
            this.btnUndoRedo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(0, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(27, 27);
            this.btnSave.TabIndex = 0;
            this.btnSave.Tag = "";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnDiscard
            // 
            this.btnUndoRedo.Image = ((System.Drawing.Image)(resources.GetObject("btnDiscard.Image")));
            this.btnUndoRedo.Location = new System.Drawing.Point(27, 0);
            this.btnUndoRedo.Name = "btnUndoRedo";
            this.btnUndoRedo.Size = new System.Drawing.Size(27, 27);
            this.btnUndoRedo.TabIndex = 0;
            this.btnUndoRedo.Tag = "";
            this.btnUndoRedo.UseVisualStyleBackColor = true;
            this.btnUndoRedo.Click += new System.EventHandler(this.btnUndoRedo_Click);
            // 
            // ObjectSaverButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnUndoRedo);
            this.Controls.Add(this.btnSave);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ObjectSaverButton";
            this.Size = new System.Drawing.Size(54, 27);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnUndoRedo;
    }
}
