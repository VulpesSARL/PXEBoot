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

        static public void Load()
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot", false);
            if (reg == null)
                return;
            TFTPRootPath = reg.GetValue("RootPath", "").ToString();

            
            reg.Close();
        }

    }
}
