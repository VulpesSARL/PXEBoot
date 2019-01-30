using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    partial class PXEService : ServiceBase
    {
        public PXEService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (Program.SMain() != 0)
            {
                this.ExitCode = 1;
                this.Stop();
            }
        }

        protected override void OnStop()
        {
            Program.StopService();
        }
    }
}
