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
        static public bool UseAllInterfaces;
        static public List<string> Interfaces;

        static public void Load()
        {
            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot", false))
            {
                if (reg == null)
                    return;
                TFTPRootPath = reg.GetValue("RootPath", "").ToString();
                UseAllInterfaces = reg.GetValue("UseAllInterfaces", "1").ToString() == "1" ? true : false;
                Interfaces = new List<string>();
                using (RegistryKey IF = reg.OpenSubKey("Interfaces"))
                {
                    if (IF != null)
                    {
                        foreach (string vn in IF.GetValueNames())
                        {
                            string n = IF.GetValue(vn).ToString();
                            if (string.IsNullOrWhiteSpace(n) == true)
                                continue;
                            Interfaces.Add(n);
                        }
                        IF.Close();
                    }
                }
                reg.Close();
            }
        }

    }
}
