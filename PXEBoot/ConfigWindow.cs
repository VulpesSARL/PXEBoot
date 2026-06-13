using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PXEBoot
{
    public partial class frmConfigWindow : Form
    {
        public frmConfigWindow()
        {
            InitializeComponent();
        }

        private void frmConfigWindow_Load(object sender, EventArgs e)
        {
            lstIF.Items.Clear();
            foreach (KeyValuePair<IPAddress, string> kvp in Program.GetNICsAndIPAddresses())
            {
                lstIF.Items.Add(kvp.Value);
            }

            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot", false))
            {
                if (reg == null)
                {
                    txtRootPath.Text = Program.AppPath + "TFTPRoot";
                    chkUseAllIF.Checked = true;
                    chkUseAllIF_CheckedChanged(null, null);
                    return;
                }

                txtRootPath.Text = reg.GetValue("RootPath", "").ToString();
                chkUseAllIF.Checked = reg.GetValue("UseAllInterfaces", "1").ToString() == "1" ? true : false;
                chkDisableSecureBootRecovery.Checked = reg.GetValue("DisableSecureBootRecovery", "0").ToString() == "1" ? true : false;
                chkUseAllIF_CheckedChanged(null, null);

                using (RegistryKey IF = reg.OpenSubKey("Interfaces"))
                {
                    if (IF != null)
                    {
                        foreach (string vn in IF.GetValueNames())
                        {
                            string n = IF.GetValue(vn).ToString();
                            if (string.IsNullOrWhiteSpace(n) == true)
                                continue;
                            for (int i = 0; i < lstIF.Items.Count; i++)
                            {
                                if (lstIF.Items[i].ToString() == n)
                                    lstIF.SetItemChecked(i, true);
                            }
                        }
                        IF.Close();
                    }
                }

                reg.Close();
            }

            if (Program.IsRunningAsAdministrator() == false)
            {
                MessageBox.Show(this, "Missing administrative permissions!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl == cmdCancel)
                        continue;
                    ctrl.Enabled = false;
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey reg = Registry.LocalMachine.CreateSubKey("Software\\Fox\\PXEBoot"))
                {
                    if (reg == null)
                    {
                        MessageBox.Show(this, "Cannot save settings", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtRootPath.Text) == true)
                    {
                        MessageBox.Show(this, "Invalid root path", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (chkUseAllIF.Checked == false && lstIF.CheckedItems.Count == 0)
                    {
                        MessageBox.Show(this, "Select at least one interface", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    reg.SetValue("RootPath", txtRootPath.Text.Trim(), RegistryValueKind.String);
                    reg.SetValue("UseAllInterfaces", chkUseAllIF.Checked == true ? "1" : "0", RegistryValueKind.String);
                    reg.SetValue("DisableSecureBootRecovery", chkDisableSecureBootRecovery.Checked == true ? "1" : "0", RegistryValueKind.String);
                    reg.DeleteSubKeyTree("Interfaces", false);

                    using (RegistryKey IF = reg.CreateSubKey("Interfaces"))
                    {
                        if (IF == null)
                        {
                            MessageBox.Show(this, "Cannot save interface list", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        int Counter = 1;
                        for (int i = 0; i < lstIF.Items.Count; i++)
                        {
                            if (lstIF.GetItemChecked(i) == true)
                            {
                                IF.SetValue(Counter.ToString(), lstIF.Items[i].ToString(), RegistryValueKind.String);
                                Counter++;
                            }
                        }
                    }

                    this.Close();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "SEH: " + ee.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fldr = new FolderBrowserDialog();
            fldr.ShowNewFolderButton = true;
            if (string.IsNullOrWhiteSpace(txtRootPath.Text) == false)
                fldr.SelectedPath = txtRootPath.Text;
            if (fldr.ShowDialog(this) != DialogResult.OK)
                return;
            txtRootPath.Text = fldr.SelectedPath;
        }

        private void chkUseAllIF_CheckedChanged(object sender, EventArgs e)
        {
            lstIF.Enabled = lblHint.Enabled = !chkUseAllIF.Checked;
        }
    }
}
