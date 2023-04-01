using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PingWinForm
{
    public partial class Form1 : Form
    {
        public delegate void ModeChanger(string s);
        public ArrayList ipAddressArray;
        HostManager hostManager;
        public static bool isWaitSave;
        bool isIni = false;
        bool isRemove = false;
        bool isLockGrid = false;
        Form2 form2;
        public Form1()
        {
            InitializeComponent();

            hostManager = new HostManager(dataGridView1, this);
            //string[] row1 = { "s1", "s2", "s3", "s4"};
            //listView1.Items.Add("Column1Text").SubItems.AddRange(row1);
            //listView1.Items.Add("row1");
            //Console.Out.WriteLine(hostManager.address[1]);
            isIni = true;
            CheckForIllegalCrossThreadCalls = false;//禁止跨线程调用检查

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        #region 数据表右键菜单
        //ping选中ip
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string ipS = dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[0].Value.ToString();
            Console.Out.WriteLine("ping this ip     " + ipS);
            hostManager.PingThisIP(ipS);


        }
        //删除选中主机
        private void Del_IP_Click(object sender, EventArgs e)
        {
            toolStripButton5_Click(sender, e);
        }
        #endregion
        private void dataGridView1_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {
            //Console.Out.WriteLine("111");
            //Console.Out.WriteLine(sender.ToString());
            //Console.Out.WriteLine(e.ToString());
            //hostManager.dataTableIP.Rows.Clear();
        }



        //确定鼠标点击的表格位置
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }
        #region 顶部按钮栏
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                Console.WriteLine("执行Ping所有IP");
                modeLabel.Text = "当前状态:扫描所有主机";
                hostManager.PingAllIP();
            }
            else
            {
                MessageBox.Show("正在扫描任务中", "扫描中...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("停止当前任务");
            hostManager.StopPing();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                isRemove = false;
                Console.WriteLine("保存变动");
                dataGridView1.EndEdit();
                Console.WriteLine();
                hostManager.SaveInf();
            }
            else
            {
                MessageBox.Show("正在扫描任务中", "扫描中...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                Console.WriteLine("新增一台主机");
                dataGridView1.Rows.Add("127.0.0.1", "", "", "未连接");
                hostManager.AddHost();
            }
            else
            {
                MessageBox.Show("正在扫描任务中", "扫描中...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                isRemove = true;
                int removeIndex = dataGridView1.SelectedCells[0].RowIndex;
                Console.WriteLine("删除一台主机,行号:" + removeIndex);
                dataGridView1.Rows.Remove(dataGridView1.Rows[removeIndex]);
                hostManager.RemoveHost(removeIndex);
            }
            else
            {
                MessageBox.Show("正在扫描任务中", "扫描中...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                Console.WriteLine("循环扫描IP模式");
                hostManager.LoopMode();
            }
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (!isLockGrid)
            {
                Console.WriteLine("打开设置面板");
                form2 = new Form2(this,hostManager);
                form2.Show();
                this.Enabled = false;
            }
            else
            {
                MessageBox.Show("正在扫描任务中", "扫描中...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #region 顶部按钮栏注释
        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            toolStripButton2_Click(sender, e);
        }
        private void toolStripLabel1_Click_1(object sender, EventArgs e)
        {
            toolStripButton1_Click(sender, e);
        }

        private void toolStripLabel3_Click(object sender, EventArgs e)
        {
            toolStripButton3_Click(sender, e);
        }
        private void toolStripLabel4_Click(object sender, EventArgs e)
        {
            toolStripButton4_Click(sender, e);
        }
        private void toolStripLabel5_Click(object sender, EventArgs e)
        {
            toolStripButton5_Click(sender, e);
        }
        private void toolStripLabel6_Click(object sender, EventArgs e)
        {
            toolStripButton6_Click(sender, e);
        }
        private void toolStripLabel7_Click(object sender, EventArgs e)
        {
            toolStripButton7_Click(sender, e);
        }
        #endregion
        #endregion
        #region 关闭程序相关
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//关闭程序时调用
        {
            hostManager.SaveConfig();//保存设置变动
            Console.WriteLine("FormClosing");
            if (hostManager.CheckChange() || isRemove)
            {
                DialogResult result = MessageBox.Show("保存修改吗？", "温馨提示：", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    hostManager.SaveInf();//保存数据变动
                    System.Environment.Exit(0);
                    //e.Cancel = false;          //这种也可以
                }
                else if (result == DialogResult.No)
                {
                    System.Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;            //取消事件的值
                }
            }
        }
        #endregion
        //编辑完成后执行
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            if (isIni)
            {
                Console.WriteLine("行数:" + e.RowIndex + "内容:" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = "";
                    Console.WriteLine("填充空行");

                }
                if (dataGridView1.Rows[e.RowIndex].Cells[1].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = "";
                    Console.WriteLine("填充空行");
                }
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[2].Value = "";
                    Console.WriteLine("填充空行");
                }


            }

        }
        //切换为待机模式
        public void ModeLabelChange(string s)
        {
            modeLabel.Text = s;

        }
        //锁定表格修改功能
        public void LockGrid(bool isLockGrid)
        {
            this.isLockGrid = isLockGrid;
            dataGridView1.Columns[0].ReadOnly = isLockGrid;
            dataGridView1.Columns[1].ReadOnly = isLockGrid;
            dataGridView1.Columns[2].ReadOnly = isLockGrid;
        }




    }
}
