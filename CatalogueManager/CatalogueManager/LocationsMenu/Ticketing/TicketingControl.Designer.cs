namespace CatalogueManager.LocationsMenu.Ticketing
{
    partial class TicketingControl
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
            this.gbTicketing = new System.Windows.Forms.GroupBox();
            this.checksUIIconOnly1 = new ReusableUIComponents.ChecksUI.ChecksUIIconOnly();
            this.btnShowTicket = new System.Windows.Forms.Button();
            this.tbTicket = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.gbTicketing.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTicketing
            // 
            this.gbTicketing.Controls.Add(this.checksUIIconOnly1);
            this.gbTicketing.Controls.Add(this.btnShowTicket);
            this.gbTicketing.Controls.Add(this.tbTicket);
            this.gbTicketing.Controls.Add(this.label6);
            this.gbTicketing.Location = new System.Drawing.Point(3, 3);
            this.gbTicketing.Name = "gbTicketing";
            this.gbTicketing.Size = new System.Drawing.Size(294, 46);
            this.gbTicketing.TabIndex = 37;
            this.gbTicketing.TabStop = false;
            this.gbTicketing.Text = "Ticketing";
            // 
            // checksUIIconOnly1
            // 
            this.checksUIIconOnly1.Location = new System.Drawing.Point(2, 17);
            this.checksUIIconOnly1.Name = "checksUIIconOnly1";
            this.checksUIIconOnly1.Size = new System.Drawing.Size(20, 20);
            this.checksUIIconOnly1.TabIndex = 33;
            // 
            // btnShowTicket
            // 
            this.btnShowTicket.Location = new System.Drawing.Point(208, 17);
            this.btnShowTicket.Name = "btnShowTicket";
            this.btnShowTicket.Size = new System.Drawing.Size(75, 23);
            this.btnShowTicket.TabIndex = 32;
            this.btnShowTicket.Text = "Show";
            this.btnShowTicket.UseVisualStyleBackColor = true;
            this.btnShowTicket.Click += new System.EventHandler(this.btnShowTicket_Click);
            // 
            // tbTicket
            // 
            this.tbTicket.Location = new System.Drawing.Point(62, 21);
            this.tbTicket.Name = "tbTicket";
            this.tbTicket.Size = new System.Drawing.Size(140, 20);
            this.tbTicket.TabIndex = 30;
            this.tbTicket.TextChanged += new System.EventHandler(this.tbTicket_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Ticket:";
            // 
            // TicketingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTicketing);
            this.Name = "TicketingControl";
            this.Size = new System.Drawing.Size(303, 54);
            this.gbTicketing.ResumeLayout(false);
            this.gbTicketing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTicketing;
        private System.Windows.Forms.Button btnShowTicket;
        private System.Windows.Forms.TextBox tbTicket;
        private System.Windows.Forms.Label label6;
        private ReusableUIComponents.ChecksUI.ChecksUIIconOnly checksUIIconOnly1;
    }
}
