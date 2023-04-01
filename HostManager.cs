using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PingWinForm
{
    class HostManager
    {
        #region 定义变量
        public DataGridView dataGridView;
        List<List<string>> ipAList = new List<List<string>>();
        List<string> nameA = new List<string>();
        List<string> ipAddressA = new List<string>();
        List<string> remakeA = new List<string>();
        List<string> stateArray = new List<string>();
        List<List<string>> ipBList = new List<List<string>>();
        List<string> nameB = new List<string>();
        List<string> ipAddressB = new List<string>();
        List<string> remakeB = new List<string>();
        PingIP pingIP;
        bool isLock1 = false;
        bool isLock2 = false;
        bool isStop = false;
        bool isLoop = false;
        public int looptime { get; private set; }//循环模式间隔时间,单位:毫秒
        public int timeout { get; private set; }//ping超时时间,单位:毫秒
        //Thread t1;
        Action<object[]> pingAction;
        Action<int> pingAllAction;
        Action<int> loopModeAction;
        object[] parmList = new object[3] { null, null, null };
        ArrayList configRemakeArr = new ArrayList();
        Form1 form1;
        #endregion
        #region 构造函数
        public HostManager(DataGridView d, Form1 f)
        {
            timeout = 0;
            looptime = 0;
            dataGridView = d;
            form1 = f;
            Console.WriteLine("初始化数据表");
            LoadInf();
            LoadConfig();
            pingIP = new PingIP(this);
            pingAction = PingThisIPAction;
            pingAllAction = PingAllAction;
            loopModeAction = LoopModeAction;
            for (int i = 0; i < ipAddressA.Count; i++)
            {
                dataGridView.Rows.Add(ipAList[0][i], ipAList[1][i], ipAList[2][i], "未连接");
                dataGridView.Rows[i].Cells[3].ReadOnly = true;
            }

        }
        #endregion
        #region IO读写
        void LoadInf()//读取存档参数
        {
            ipAList.Add(ipAddressA);
            ipAList.Add(nameA);
            ipAList.Add(remakeA);
            ipAList.Add(stateArray);
            ipBList.Add(ipAddressB);
            ipBList.Add(nameB);
            ipBList.Add(remakeB);
            StreamReader sr = File.OpenText(@".\ipAddress.sav");
            string nextLine;

            while ((nextLine = sr.ReadLine()) != null)
            {
                ipAddressA.Add(nextLine);
                ipAddressB.Add(nextLine);
            }
            sr.Close();
            sr = File.OpenText(@".\name.sav");
            while ((nextLine = sr.ReadLine()) != null)
            {
                nameA.Add(nextLine);
                nameB.Add(nextLine);
            }
            sr.Close();
            sr = File.OpenText(@".\remake.sav");
            while ((nextLine = sr.ReadLine()) != null)
            {
                remakeA.Add(nextLine);
                remakeB.Add(nextLine);
            }

            sr.Close();
        }
        public void SaveInf()//写入存档参数
        {
            Console.WriteLine("开始保存");
            FileInfo myFile1 = new FileInfo(@".\ipAddress.sav");
            StreamWriter sw1 = myFile1.CreateText();
            int i = 0;
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                sw1.WriteLine(item.Cells[0].Value.ToString());
                ipBList[0][i] = item.Cells[0].Value.ToString();
                Console.WriteLine(item.Cells[0].Value.ToString());
                i += 1;
            }
            sw1.Close();
            i = 0;
            myFile1 = new FileInfo(@".\name.sav");
            sw1 = myFile1.CreateText();
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                sw1.WriteLine(item.Cells[1].Value.ToString());
                ipBList[1][i] = item.Cells[1].Value.ToString();
                i += 1;
            }
            sw1.Close();
            i = 0;
            myFile1 = new FileInfo(@".\remake.sav");
            sw1 = myFile1.CreateText();
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                sw1.WriteLine(item.Cells[2].Value.ToString());
                ipBList[2][i] = item.Cells[2].Value.ToString();
                i += 1;
            }
            sw1.Close();

            Console.WriteLine("保存完成");
        }
        public bool CheckChange()//检查数据是否发生修改
        {
            for (int i = 0; i < ipAddressA.Count; i++)
            {
                Console.WriteLine(dataGridView.Rows[i].Cells[0].Value.ToString());
                Console.WriteLine(ipBList[0][i]);
                if (dataGridView.Rows[i].Cells[0].Value.ToString() != ipBList[0][i])
                {
                    Console.WriteLine("ip地址发生变动   " + i);
                    Console.WriteLine("现:   " + dataGridView.Rows[i].Cells[0].Value.ToString());
                    Console.WriteLine("原:   " + ipBList[0][i]);
                    return true;
                }
                if (dataGridView.Rows[i].Cells[1].Value.ToString() != ipBList[1][i])
                {
                    Console.WriteLine("主机发生变动   " + i);
                    Console.WriteLine("现:   " + dataGridView.Rows[i].Cells[1].Value.ToString());
                    Console.WriteLine("原:   " + ipBList[1][i]);
                    return true;
                }
                if (dataGridView.Rows[i].Cells[2].Value.ToString() != ipBList[2][i])
                {
                    Console.WriteLine("备注发生变动   " + i);
                    Console.WriteLine("现:   " + dataGridView.Rows[i].Cells[2].Value.ToString());
                    Console.WriteLine("原:   " + ipBList[2][i]);
                    return true;
                }

            }
            return false;
        }
        void LoadConfig()//读取参数
        {
            Console.WriteLine("读取设置参数");
            StreamReader sr = File.OpenText(@".\config.ini");
            string nextLine1;
            string nextLine2;
            ArrayList arrayList = new ArrayList();
            while ((nextLine1 = sr.ReadLine()) != null && (nextLine2 = sr.ReadLine()) != null)
            {
                configRemakeArr.Add(nextLine1);
                Console.WriteLine(nextLine1 + "    " + nextLine2);
                arrayList.Add(nextLine2);
            }
            sr.Close();
            timeout = int.Parse(arrayList[0].ToString());
            looptime = int.Parse(arrayList[1].ToString());
            form1.Size = new Size(int.Parse(arrayList[2].ToString()), int.Parse(arrayList[3].ToString()));
            dataGridView.Columns[0].Width = int.Parse(arrayList[4].ToString());
            dataGridView.Columns[1].Width = int.Parse(arrayList[5].ToString());
            dataGridView.Columns[2].Width = int.Parse(arrayList[6].ToString());
            dataGridView.Columns[3].Width = int.Parse(arrayList[7].ToString());

        }
        public void SaveConfig()//写入参数
        {
            Console.WriteLine("开始保存设置参数");
            FileInfo myFile1 = new FileInfo(@".\config.ini");
            StreamWriter sw1 = myFile1.CreateText();
            //灌装参数
            ArrayList arrayList = new ArrayList();
            arrayList.Add(timeout);
            arrayList.Add(looptime);
            arrayList.Add(form1.Size.Width);
            arrayList.Add(form1.Size.Height);
            arrayList.Add(dataGridView.Columns[0].Width);
            arrayList.Add(dataGridView.Columns[1].Width);
            arrayList.Add(dataGridView.Columns[2].Width);
            arrayList.Add(dataGridView.Columns[3].Width);
            int i = 0;
            foreach (string item in configRemakeArr)
            {
                sw1.WriteLine(item);
                sw1.WriteLine(arrayList[i].ToString());
                i += 1;
            }
            sw1.Close();
        }
        #endregion
        #region 四大业务逻辑代码(单个,全部,循环,暂停)
        public void PingThisIP(string s)//ping单个IP
        {
            form1.LockGrid(true);
            //检查线程是否锁定
            if (!isLock1)
            {
                isLock1 = true;


                int i = dataGridView.SelectedRows[0].Index;
                parmList[0] = s;//IP地址
                parmList[1] = i;//选中行数
                parmList[2] = dataGridView;//表单对象
                Console.Out.WriteLine("行数" + parmList[1]);
                pingAction.BeginInvoke(parmList, pingEnd, parmList);
            }
            //线程被锁定时执行对象
            else if (isLock1 && (int)parmList[1] != dataGridView.SelectedRows[0].Index)
            {
                dataGridView.Rows[dataGridView.SelectedRows[0].Index].Cells[3].Value = "测试被占用";
                dataGridView.Rows[dataGridView.SelectedRows[0].Index].Cells[3].Style.ForeColor = System.Drawing.Color.Black;
            }
        }
        public void PingThisIP(string s, int i)//ping单个IP
        {
            form1.LockGrid(true);
            //检查线程是否锁定
            if (!isLock1)
            {
                isLock1 = true;

                parmList[0] = s;//IP地址
                parmList[1] = i;//选中行数
                parmList[2] = dataGridView;//表单对象
                Console.Out.WriteLine("行数" + parmList[1]);
                pingAction.BeginInvoke(parmList, pingEnd, parmList);
            }
            //线程被锁定时执行对象
            else if (isLock1)
            {
                Console.WriteLine("全ping时重复执行线程");
            }
        }
        private void pingEnd(IAsyncResult ar)//ping单个IP结束回调
        {
            Console.Out.WriteLine("完成单次ping");
            isLock1 = false;
            if (!isLock2 && !isLoop)
            {
                form1.LockGrid(false);
            }
        }
        void PingThisIPAction(object[] parmList)//ping单个IP多线程内容
        {
            dataGridView.Rows[(int)parmList[1]].Cells[3].Value = "测试中";
            pingIP.PingIPOnce(parmList);

        }
        public void PingAllIP()//ping所有IP
        {
            form1.LockGrid(true);
            pingAllAction.BeginInvoke(1, PingAllEnd, null);
        }
        void PingAllAction(int i)//ping所有IP委托内容
        {
            isLock2 = true;
            i = 0;
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                //Console.WriteLine(item.Cells[0].Value);
                PingThisIP(item.Cells[0].Value.ToString(), i);
                for (int j = 0; j < 500 && isLock1; j++)
                {
                    //Console.WriteLine("暂停");
                    Thread.Sleep(10);
                }
                Thread.Sleep(10);
                i++;
                isLock1 = false;
                if (isStop)
                {
                    break;
                }
            }
        }
        void PingAllEnd(IAsyncResult ar)//ping所有IP结束回调
        {
            Thread.Sleep(1000);
            isLock2 = false;
            isStop = false;
            if (!isLoop)
            {
                form1.ModeLabelChange("当前状态:待机");
                form1.LockGrid(false);
            }
            Console.WriteLine("\n完成一轮扫描");
        }
        public void LoopMode()//循环扫描模式
        {
            Console.WriteLine("打开循环模式");
            form1.LockGrid(true);
            if (looptime < 10 || looptime > 1000)
            {
                looptime = 10;
            }
            isLoop = true;
            loopModeAction.BeginInvoke(1, LoopModeEnd, null);
            form1.ModeLabelChange("当前状态:循环扫描(每" + looptime + "秒一轮)");
        }
        void LoopModeAction(int j)//循环扫描模式委托
        {
            while (isLoop)
            {
                if (isLock1 || isLock2)
                {
                    isLock2 = false;
                    Thread.Sleep(timeout);
                }
                else
                {
                    Console.WriteLine("Ping All PI");
                    PingAllIP();
                    for (int i = 0; (i < looptime * 10 || isLock2) && isLoop; i++)
                    {
                        Thread.Sleep(100);
                    }
                }

            }
            form1.ModeLabelChange("当前状态:待机");
        }
        void LoopModeEnd(IAsyncResult ar)//循环扫描模式结束
        {
            Thread.Sleep(1000);
            Console.WriteLine("循环模式结束");
            form1.LockGrid(false);
        }
        public void StopPing()//停止当前作业
        {
            if (isLock2)
            {
                isStop = true;
            }
            isLoop = false;
            form1.LockGrid(false);
        }
        #endregion
        #region 增加删除服务器
        public void AddHost()//添加服务器
        {
            ipAList[0].Add("127.0.0.1");
            ipAList[1].Add("");
            ipAList[2].Add("");
            ipBList[0].Add("");
            ipBList[1].Add("");
            ipBList[2].Add("");
            dataGridView.Rows[ipAddressA.Count - 1].Cells[3].ReadOnly = true;
        }
        public void RemoveHost(int index)//删除服务器
        {
            ipAList[0].RemoveAt(index);
            ipAList[1].RemoveAt(index);
            ipAList[2].RemoveAt(index);
            ipBList[0].RemoveAt(index);
            ipBList[1].RemoveAt(index);
            ipBList[2].RemoveAt(index);
        }
        #endregion
        public void SetMinTimeout()//给timeout设定最小值500毫秒
        {
            timeout = 500;
        }
        public void SetConfigData(int timeout, int looptime)//设置系统参数
        {
            this.timeout = timeout;
            this.looptime = looptime;
        }
    }
}
