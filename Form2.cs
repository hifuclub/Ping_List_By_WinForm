using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PingWinForm
{
    public partial class Form2 : Form
    {
        Form1 form1;
        HostManager hostManager;
        public Form2(Form1 form1,object hostManager)
        {

            this.hostManager = (HostManager)hostManager;
            this.form1 = form1;
            InitializeComponent();
            textBox1.Text = this.hostManager.timeout.ToString();
            textBox2.Text = this.hostManager.looptime.ToString();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!Regex.IsMatch(textBox1.Text+ textBox2.Text,@"^[0-9]*$"))
            {
                DialogResult result = MessageBox.Show("请填入整数", "提示：", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                hostManager.SetConfigData(int.Parse(textBox1.Text), int.Parse(textBox2.Text));
                hostManager.SaveConfig();
                form1.Enabled = true;
                this.Close();
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            form1.Enabled = true;
        }
    }
}
