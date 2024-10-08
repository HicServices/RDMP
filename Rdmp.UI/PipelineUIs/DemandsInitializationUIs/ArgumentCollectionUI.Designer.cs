﻿using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs
{
    partial class ArgumentCollectionUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArgumentCollectionUI));
            this.lblClassName = new System.Windows.Forms.Label();
            this.helpIcon1 = new Rdmp.UI.SimpleControls.HelpIcon();
            this.lblNoArguments = new System.Windows.Forms.Label();
            this.pArguments = new System.Windows.Forms.Panel();
            this.lblTypeUnloadable = new System.Windows.Forms.Label();
            this.lblComponentNotFound = new System.Windows.Forms.Label();
            this.lblArgumentsTitle = new System.Windows.Forms.Label();
            this.pArguments.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblClassName
            // 
            this.lblClassName.AutoSize = true;
            this.lblClassName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblClassName.Location = new System.Drawing.Point(5, 7);
            this.lblClassName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(66, 15);
            this.lblClassName.TabIndex = 3;
            this.lblClassName.Text = "ClassName";
            this.lblClassName.Visible = false;
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(245, 1);
            this.helpIcon1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(26, 25);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(26, 25);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(26, 25);
            this.helpIcon1.SuppressClick = false;
            this.helpIcon1.TabIndex = 8;
            this.helpIcon1.Visible = false;
            // 
            // lblNoArguments
            // 
            this.lblNoArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoArguments.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblNoArguments.Location = new System.Drawing.Point(0, 19);
            this.lblNoArguments.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNoArguments.Name = "lblNoArguments";
            this.lblNoArguments.Size = new System.Drawing.Size(832, 13);
            this.lblNoArguments.TabIndex = 0;
            this.lblNoArguments.Text = "No Arguments Found";
            this.lblNoArguments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblNoArguments.Visible = false;
            // 
            // pArguments
            // 
            this.pArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pArguments.AutoScroll = true;
            this.pArguments.Controls.Add(this.lblTypeUnloadable);
            this.pArguments.Controls.Add(this.lblNoArguments);
            this.pArguments.Location = new System.Drawing.Point(5, 55);
            this.pArguments.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pArguments.Name = "pArguments";
            this.pArguments.Size = new System.Drawing.Size(832, 476);
            this.pArguments.TabIndex = 9;
            // 
            // lblTypeUnloadable
            // 
            this.lblTypeUnloadable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTypeUnloadable.ForeColor = System.Drawing.Color.Red;
            this.lblTypeUnloadable.Location = new System.Drawing.Point(0, 42);
            this.lblTypeUnloadable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTypeUnloadable.Name = "lblTypeUnloadable";
            this.lblTypeUnloadable.Size = new System.Drawing.Size(832, 57);
            this.lblTypeUnloadable.TabIndex = 10;
            this.lblTypeUnloadable.Text = "Component Type not Selected.\r\n\r\nPlease select a Pipeline Component from above to " +
    "configure its Argument values.";
            this.lblTypeUnloadable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblComponentNotFound
            // 
            this.lblComponentNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblComponentNotFound.ForeColor = System.Drawing.Color.Red;
            this.lblComponentNotFound.Location = new System.Drawing.Point(5, 42);
            this.lblComponentNotFound.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblComponentNotFound.Name = "lblComponentNotFound";
            this.lblComponentNotFound.Size = new System.Drawing.Size(832, 93);
            this.lblComponentNotFound.TabIndex = 11;
            this.lblComponentNotFound.Text = resources.GetString("lblComponentNotFound.Text");
            this.lblComponentNotFound.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblComponentNotFound.Visible = false;
            // 
            // lblArgumentsTitle
            // 
            this.lblArgumentsTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArgumentsTitle.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblArgumentsTitle.Location = new System.Drawing.Point(5, 31);
            this.lblArgumentsTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblArgumentsTitle.Name = "lblArgumentsTitle";
            this.lblArgumentsTitle.Size = new System.Drawing.Size(832, 23);
            this.lblArgumentsTitle.TabIndex = 11;
            this.lblArgumentsTitle.Text = "Arguments";
            this.lblArgumentsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblArgumentsTitle.Visible = false;
            // 
            // ArgumentCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.lblComponentNotFound);
            this.Controls.Add(this.lblArgumentsTitle);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.lblClassName);
            this.Controls.Add(this.pArguments);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ArgumentCollectionUI";
            this.Size = new System.Drawing.Size(840, 534);
            this.pArguments.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblClassName;
        private HelpIcon helpIcon1;
        private System.Windows.Forms.Label lblNoArguments;
        private System.Windows.Forms.Panel pArguments;
        private System.Windows.Forms.Label lblTypeUnloadable;
        private System.Windows.Forms.Label lblArgumentsTitle;
        private System.Windows.Forms.Label lblComponentNotFound;
    }
}
