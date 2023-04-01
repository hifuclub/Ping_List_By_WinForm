using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PingWinForm
{
    class PingIP
    {
        Ping ping = new Ping();
        HostManager hostManager;
        public PingIP(HostManager hostManager)
        {
            this.hostManager = hostManager;
        }
        public bool PingIPOnce(object[] parmList)
        {
            DataGridView dataGridView = (DataGridView)parmList[2];
            if (!Regex.IsMatch((string)parmList[0],
                @"^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$"))
            {
                Console.WriteLine("不符合格式！");
                dataGridView.Rows[(int)parmList[1]].Cells[3].Value = "ip地址格式非法";
                dataGridView.Rows[(int)parmList[1]].Cells[3].Style.ForeColor = System.Drawing.Color.Orange;
                return false;
            }
            bool online = false; //是否在线
            if (hostManager.timeout < 500 || hostManager.timeout > 10 * 1000)
            {
                hostManager.SetMinTimeout();
            }
            PingReply pingReply = ping.Send((string)parmList[0], hostManager.timeout);
            if (pingReply.Status == IPStatus.Success)
            {
                online = true;
                Console.WriteLine("当前在线，已ping通！");
                dataGridView.Rows[(int)parmList[1]].Cells[3].Value = "连接成功" + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
                dataGridView.Rows[(int)parmList[1]].Cells[3].Style.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                Console.WriteLine("不在线，ping不通！");
                dataGridView.Rows[(int)parmList[1]].Cells[3].Value = "连接失败" + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
                dataGridView.Rows[(int)parmList[1]].Cells[3].Style.ForeColor = System.Drawing.Color.Red;
            }
            return online;
        }
    }
}
