namespace PXEBoot
{
    partial class frmConfigWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfigWindow));
            this.cmdOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRootPath = new System.Windows.Forms.TextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lstIF = new System.Windows.Forms.CheckedListBox();
            this.chkUseAllIF = new System.Windows.Forms.CheckBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.chkDisableSecureBootRecovery = new System.Windows.Forms.CheckBox();
            this.cmdBrowseFolder = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(371, 251);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 7;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "TFTP Root Path:";
            // 
            // txtRootPath
            // 
            this.txtRootPath.Location = new System.Drawing.Point(120, 12);
            this.txtRootPath.Name = "txtRootPath";
            this.txtRootPath.Size = new System.Drawing.Size(291, 20);
            this.txtRootPath.TabIndex = 0;
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(281, 251);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 8;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lstIF
            // 
            this.lstIF.FormattingEnabled = true;
            this.lstIF.IntegralHeight = false;
            this.lstIF.Location = new System.Drawing.Point(15, 96);
            this.lstIF.Name = "lstIF";
            this.lstIF.ScrollAlwaysVisible = true;
            this.lstIF.Size = new System.Drawing.Size(431, 96);
            this.lstIF.TabIndex = 5;
            // 
            // chkUseAllIF
            // 
            this.chkUseAllIF.AutoSize = true;
            this.chkUseAllIF.Location = new System.Drawing.Point(15, 73);
            this.chkUseAllIF.Name = "chkUseAllIF";
            this.chkUseAllIF.Size = new System.Drawing.Size(108, 17);
            this.chkUseAllIF.TabIndex = 3;
            this.chkUseAllIF.Text = "&Use all Interfaces";
            this.chkUseAllIF.UseVisualStyleBackColor = true;
            this.chkUseAllIF.CheckedChanged += new System.EventHandler(this.chkUseAllIF_CheckedChanged);
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(12, 195);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(434, 35);
            this.lblHint.TabIndex = 6;
            this.lblHint.Text = "Hint: you need to reconfigure Fox PXE Service when something\'s changed on the net" +
    "work configuration";
            // 
            // chkDisableSecureBootRecovery
            // 
            this.chkDisableSecureBootRecovery.AutoSize = true;
            this.chkDisableSecureBootRecovery.Location = new System.Drawing.Point(15, 50);
            this.chkDisableSecureBootRecovery.Name = "chkDisableSecureBootRecovery";
            this.chkDisableSecureBootRecovery.Size = new System.Drawing.Size(238, 17);
            this.chkDisableSecureBootRecovery.TabIndex = 2;
            this.chkDisableSecureBootRecovery.Text = "&Disable providing of SecureBootRecovery.efi";
            this.chkDisableSecureBootRecovery.UseVisualStyleBackColor = true;
            // 
            // cmdBrowseFolder
            // 
            this.cmdBrowseFolder.Location = new System.Drawing.Point(417, 10);
            this.cmdBrowseFolder.Name = "cmdBrowseFolder";
            this.cmdBrowseFolder.Size = new System.Drawing.Size(29, 23);
            this.cmdBrowseFolder.TabIndex = 1;
            this.cmdBrowseFolder.Text = "...";
            this.cmdBrowseFolder.UseVisualStyleBackColor = true;
            this.cmdBrowseFolder.Click += new System.EventHandler(this.cmdBrowseFolder_Click);
            // 
            // frmConfigWindow
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(460, 286);
            this.Controls.Add(this.cmdBrowseFolder);
            this.Controls.Add(this.chkDisableSecureBootRecovery);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.chkUseAllIF);
            this.Controls.Add(this.lstIF);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtRootPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmConfigWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fox PXE Service Configuration";
            this.Load += new System.EventHandler(this.frmConfigWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRootPath;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckedListBox lstIF;
        private System.Windows.Forms.CheckBox chkUseAllIF;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.CheckBox chkDisableSecureBootRecovery;
        private System.Windows.Forms.Button cmdBrowseFolder;
    }
}