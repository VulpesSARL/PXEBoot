using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    static class Settings
    {
        static public string TFTPRootPath;
        static public string IPAddressOverride;
        static public string ListenAddress;

        static public void Load()
        {
            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot", false))
            {
                if (reg == null)
                    return;
                TFTPRootPath = reg.GetValue("RootPath", "").ToString();
                IPAddressOverride = reg.GetValue("IPAddressOverride", "").ToString();
                ListenAddress = reg.GetValue("ListenAddress", "").ToString();

                reg.Close();
            }
        }

    }
}
