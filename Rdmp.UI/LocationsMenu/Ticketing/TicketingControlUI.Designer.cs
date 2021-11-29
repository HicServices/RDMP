using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.LocationsMenu.Ticketing
{
    partial class TicketingControlUI
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
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.btnShowTicket = new System.Windows.Forms.Button();
            this.tbTicket = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.gbTicketing.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTicketing
            // 
            this.gbTicketing.Controls.Add(this.ragSmiley1);
            this.gbTicketing.Controls.Add(this.btnShowTicket);
            this.gbTicketing.Controls.Add(this.tbTicket);
            this.gbTicketing.Controls.Add(this.label6);
            this.gbTicketing.Location = new System.Drawing.Point(4, 3);
            this.gbTicketing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTicketing.Name = "gbTicketing";
            this.gbTicketing.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTicketing.Size = new System.Drawing.Size(343, 53);
            this.gbTicketing.TabIndex = 37;
            this.gbTicketing.TabStop = false;
            this.gbTicketing.Text = "Ticketing";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(308, 18);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(29, 29);
            this.ragSmiley1.TabIndex = 33;
            this.ragSmiley1.Visible = false;
            // 
            // btnShowTicket
            // 
            this.btnShowTicket.Location = new System.Drawing.Point(241, 19);
            this.btnShowTicket.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnShowTicket.Name = "btnShowTicket";
            this.btnShowTicket.Size = new System.Drawing.Size(61, 25);
            this.btnShowTicket.TabIndex = 32;
            this.btnShowTicket.Text = "Show";
            this.btnShowTicket.UseVisualStyleBackColor = true;
            this.btnShowTicket.Click += new System.EventHandler(this.btnShowTicket_Click);
            // 
            // tbTicket
            // 
            this.tbTicket.Location = new System.Drawing.Point(50, 20);
            this.tbTicket.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbTicket.Name = "tbTicket";
            this.tbTicket.Size = new System.Drawing.Size(187, 23);
            this.tbTicket.TabIndex = 30;
            this.tbTicket.TextChanged += new System.EventHandler(this.tbTicket_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 23);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 31;
            this.label6.Text = "Ticket:";
            // 
            // TicketingControlUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTicketing);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TicketingControlUI";
            this.Size = new System.Drawing.Size(354, 62);
            this.gbTicketing.ResumeLayout(false);
            this.gbTicketing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTicketing;
        private System.Windows.Forms.Button btnShowTicket;
        private System.Windows.Forms.TextBox tbTicket;
        private System.Windows.Forms.Label label6;
        private RAGSmiley ragSmiley1;
        
    }
}
