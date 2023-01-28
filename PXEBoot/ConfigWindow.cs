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
                        MessageBox.Show(this, "Invalid root path", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    reg.SetValue("RootPath", txtRootPath.Text.Trim(), RegistryValueKind.String);

                    if (IPAddress.TryParse(txtListenAddress.Text.Trim(), out _) == true)
                    {
                        reg.SetValue("ListenAddress", txtListenAddress.Text.Trim(), RegistryValueKind.String);
                    }
                    else
                    {
                        reg.DeleteValue("ListenAddress");
                    }

                    if (IPAddress.TryParse(txtIPAddressOverride.Text.Trim(), out _) == true)
                    {
                        reg.SetValue("IPAddressOverride", txtIPAddressOverride.Text.Trim(), RegistryValueKind.String);
                    }
                    else
                    {
                        reg.DeleteValue("IPAddressOverride");
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
                txtIPAddressOverride.Text = reg.GetValue("IPAddressOverride", "").ToString();
                txtListenAddress.Text = reg.GetValue("ListenAddress", "").ToString();

                reg.Close();
            }
        }
    }
}
