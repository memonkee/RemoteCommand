using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteCommand
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Core.Server.isRunning)
            {
                Core.Server.Stop();
                label1.Text = "stopped";
            }
            else
            {
                Core.Server.Start();
                //label1.Text = Core.Server.server.GetContext().ToString();
            }
        }
    }
}
