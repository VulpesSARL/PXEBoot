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
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(371, 210);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Root Path";
            // 
            // txtRootPath
            // 
            this.txtRootPath.Location = new System.Drawing.Point(151, 12);
            this.txtRootPath.Name = "txtRootPath";
            this.txtRootPath.Size = new System.Drawing.Size(295, 20);
            this.txtRootPath.TabIndex = 0;
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(281, 210);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lstIF
            // 
            this.lstIF.FormattingEnabled = true;
            this.lstIF.IntegralHeight = false;
            this.lstIF.Location = new System.Drawing.Point(15, 73);
            this.lstIF.Name = "lstIF";
            this.lstIF.ScrollAlwaysVisible = true;
            this.lstIF.Size = new System.Drawing.Size(431, 96);
            this.lstIF.TabIndex = 2;
            // 
            // chkUseAllIF
            // 
            this.chkUseAllIF.AutoSize = true;
            this.chkUseAllIF.Location = new System.Drawing.Point(15, 50);
            this.chkUseAllIF.Name = "chkUseAllIF";
            this.chkUseAllIF.Size = new System.Drawing.Size(108, 17);
            this.chkUseAllIF.TabIndex = 1;
            this.chkUseAllIF.Text = "&Use all Interfaces";
            this.chkUseAllIF.UseVisualStyleBackColor = true;
            this.chkUseAllIF.CheckedChanged += new System.EventHandler(this.chkUseAllIF_CheckedChanged);
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(12, 172);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(434, 35);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = "Hint: you need to reconfigure Fox PXE Boot when something\'s changed on the networ" +
    "k configuration";
            // 
            // frmConfigWindow
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(460, 245);
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
            this.Text = "PXEBoot Configuration";
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
    }
}