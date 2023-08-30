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

        private void frmConfigWindow_Load(object sender, EventArgs e)
        {
            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot", false))
            {
                if (reg == null)
                    return;

                txtRootPath.Text = reg.GetValue("RootPath", "").ToString();
                chkUseAllIF.Checked = reg.GetValue("UseAllInterfaces", "1").ToString() == "1" ? true : false;
                chkUseAllIF_CheckedChanged(sender, e);
                lstIF.Items.Clear();
                foreach (KeyValuePair<IPAddress, string> kvp in Program.GetNICsAndIPAddresses())
                {
                    lstIF.Items.Add(kvp.Value);
                }

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
        }

        private void chkUseAllIF_CheckedChanged(object sender, EventArgs e)
        {
            lstIF.Enabled = !chkUseAllIF.Checked;
            lblHint.Enabled = !chkUseAllIF.Checked;
        }
    }
}
