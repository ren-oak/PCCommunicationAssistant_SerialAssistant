using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using INIFILE;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace myserialassistant
{
    public partial class Form1 : Form
    {
        public Socket newclient;
        public bool Connected;
        public Thread myThread;
        public delegate void MyInvoke(string str);       
        SerialPort sp1 = new SerialPort();
        public Form1()
        {
            InitializeComponent();
        }
        public void Connect()
        {
            byte[] data = new byte[1024];
            newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ipadd = serverIP.Text.Trim();
            int port = Convert.ToInt32(serverPort.Text.Trim());
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipadd), port);
            try
            {
                newclient.Connect(ie);
                connect.Enabled = false;
                Connected = true;
            }
            catch (SocketException e)
            {
                MessageBox.Show("连接服务器失败  " + e.Message);
                return;
            }
            ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            myThread = new Thread(myThreaddelegate);
            myThread.Start();
        }
        public void ReceiveMsg()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int recv = newclient.Receive(data);
                    string stringdata = Encoding.UTF8.GetString(data, 0, recv);                                              
                    showMsg("get mesg"+":"+ stringdata + " ." + "\r\n");
                }            
                catch 
                {
                    newclient.Shutdown(SocketShutdown.Both);
                    newclient.Close();
                    break;
                }           
            }
        }
        public void showMsg(string msg)
        {
            {
                //在线程里以安全方式调用控件
                if (recMsg.InvokeRequired)
                {
                    MyInvoke _myinvoke = new MyInvoke(showMsg);
                    recMsg.Invoke(_myinvoke, new object[] { msg });
                }
                else
                {
                    recMsg.AppendText(msg);
                }
            }
        }
        private void connect_Click(object sender, EventArgs e)
        {
            Connect();

            txtSend.Text = "@serialassistant@";
            int m_length = txtSend.Text.Length;
            byte[] data = new byte[m_length];
            data = Encoding.UTF8.GetBytes(txtSend.Text);
            int i = newclient.Send(data);
            showMsg(txtSend.Text + "\r\n");
            txtSend.Text = "";
        }
        //加载
        private void Form1_Load(object sender, EventArgs e)
        {
            INIFILE.Profile.LoadProfile();//加载所有
            // 预置波特率
            switch (Profile.G_BAUDRATE)
            {
                case "300":
                    cbBaudRate.SelectedIndex = 0;
                    break;
                case "600":
                    cbBaudRate.SelectedIndex = 1;
                    break;
                case "1200":
                    cbBaudRate.SelectedIndex = 2;
                    break;
                case "2400":
                    cbBaudRate.SelectedIndex = 3;
                    break;
                case "4800":
                    cbBaudRate.SelectedIndex = 4;
                    break;
                case "9600":
                    cbBaudRate.SelectedIndex = 5;
                    break;
                case "19200":
                    cbBaudRate.SelectedIndex = 6;
                    break;
                case "38400":
                    cbBaudRate.SelectedIndex = 7;
                    break;
                case "115200":
                    cbBaudRate.SelectedIndex = 8;
                    break;
                default:
                    {
                        MessageBox.Show("波特率预置参数错误。");
                        return;
                    }
            }
            //预置波特率
            switch (Profile.G_DATABITS)
            {
                case "5":
                    cbDataBits.SelectedIndex = 0;
                    break;
                case "6":
                    cbDataBits.SelectedIndex = 1;
                    break;
                case "7":
                    cbDataBits.SelectedIndex = 2;
                    break;
                case "8":
                    cbDataBits.SelectedIndex = 3;
                    break;
                default:
                    {
                        MessageBox.Show("数据位预置参数错误。");
                        return;
                    }
            }
            //预置停止位
            switch (Profile.G_STOP)
            {
                case "1":
                    cbStop.SelectedIndex = 0;
                    break;
                case "1.5":
                    cbStop.SelectedIndex = 1;
                    break;
                case "2":
                    cbStop.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("停止位预置参数错误。");
                        return;
                    }
            }
            //预置校验位
            switch (Profile.G_PARITY)
            {
                case "NONE":
                    cbParity.SelectedIndex = 0;
                    break;
                case "ODD":
                    cbParity.SelectedIndex = 1;
                    break;
                case "EVEN":
                    cbParity.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("校验位预置参数错误。");
                        return;
                    }
            }
            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }
            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {//获取有多少个COM口          
                cbSerial.Items.Add(s);
            }
            //串口设置默认选择项
            cbSerial.SelectedIndex = 3;         
            sp1.BaudRate = 9600;
            Control.CheckForIllegalCrossThreadCalls = false; 
            sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived);
            //sp1.ReceivedBytesThreshold = 1;

            radio1.Checked = true;  //单选按钮默认是选中的
            rbRcvStr.Checked = true;

            //准备就绪              
            sp1.DtrEnable = true;
            sp1.RtsEnable = true;
            //设置数据读取超时为1秒
            sp1.ReadTimeout = 1000;

            sp1.Close();
        }
        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {                  
           if (rbRcv16.Checked == true)
                    {

                        try
                        {
                            Byte[] receivedData = new Byte[sp1.BytesToRead];        //创建接收字节数组
                            sp1.Read(receivedData, 0, receivedData.Length);         //读取数据                        
                            sp1.DiscardInBuffer();                                  //清空SerialPort控件的Buffer                           
                            string strRcv = null;                          
                            for (int i = 0; i < receivedData.Length; i++) //窗体显示
                            {
                                  strRcv += receivedData[i].ToString("X2");  //16进制显示
                                //strRcv += receivedData[i].ToString();  //////////////注意！！！！！此时是10进制显示
                            }
                                                   
                          if(sendornot==true)
                          {
                              int senddatalen = strRcv.Length;
                              byte[] datarbr16=new byte[senddatalen];
                              datarbr16 = Encoding.UTF8.GetBytes(strRcv);           
                              newclient.Send(datarbr16);
                          }
                             txtReceive.Text += strRcv+"\r\n";     
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.Message, "出错提示");                     
                        }
                    }                
                    else 
                    {
                        try
                        {
                            Byte[] receivedData = new Byte[sp1.BytesToRead];        //创建接收字节数组
                            sp1.Read(receivedData, 0, receivedData.Length);         //读取数据                        
                            sp1.DiscardInBuffer();
                            int recdatalen = receivedData.Length;
                            string chs = Encoding.ASCII.GetString(receivedData);
                            if (sendornot == true)
                            {
                                int senddatalen = chs.Length;
                                byte[] datarbr16 = new byte[senddatalen];
                                datarbr16 = Encoding.UTF8.GetBytes(chs);                            
                                newclient.Send(datarbr16);
                               // string showdata = "";
                               // showdata += chs;
                               // showMsg(showdata);
                            }
                            txtReceive.Text += chs;
                        }
                        catch(System.Exception ex)
                        {
                            MessageBox.Show(ex.Message,"出错提示");
                        }
                     }                           
                }
        private void btnSend_Click(object sender, EventArgs e)
        {

            if (cbTimeSend.Checked)
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }

            if (!sp1.IsOpen) 
            {
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }

            String strSend = txtSend.Text;
            if (radio1.Checked == true)
            {
                //处理数字转换
                string sendBuf = strSend;
                string sendnoNull = sendBuf.Trim();
                string sendNOComma = sendnoNull.Replace(',', ' ');   
                string sendNOComma1 = sendNOComma.Replace('，', ' '); 
                string strSendNoComma2 = sendNOComma1.Replace("0x", "");
                strSendNoComma2.Replace("0X", ""); 
                string[] strArray = strSendNoComma2.Split(' ');

                int byteBufferLength = strArray.Length;
                for (int d = 0; d < strArray.Length; d++)
                {
                    if (strArray[d] == "")
                    {
                        byteBufferLength--;
                    }
                }
                byte[] byteBuffer = new byte[byteBufferLength];
                int ii = 0;
                for (int c = 0; c< strArray.Length; c++)        //对获取的字符做相加运算
                {

                    Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[c]);

                    int decNum = 0;
                    if (strArray[c] == "")
                    {
                        continue;
                    }
                    else
                    {
                        decNum = Convert.ToInt32(strArray[c], 16);
                    }

                    try 
                    {
                        byteBuffer[ii] = Convert.ToByte(decNum);
                    }
                    catch
                    {
                        MessageBox.Show("字节越界，请逐个字节输入！", "Error");
                        tmSend.Enabled = false;
                        return;
                    }

                    ii++;
                }
                sp1.Write(byteBuffer, 0, byteBuffer.Length);
            }
            else 
            {
                sp1.WriteLine(txtSend.Text);    //写入数据
            }
        }

        //开关按钮
        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (!sp1.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cbSerial.SelectedItem.ToString();
                    sp1.PortName = serialName;
                    //设置各“串口设置”
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    sp1.BaudRate = iBaudRate;      
                    sp1.DataBits = iDateBits;      
                    switch (cbStop.Text)         
                    {
                        case "1":
                            sp1.StopBits = StopBits.One;
                            break;
                        case "1.5":
                            sp1.StopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            sp1.StopBits = StopBits.Two;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }
                    switch (cbParity.Text)      
                    {
                        case "无":
                            sp1.Parity = Parity.None;
                            break;
                        case "奇校验":
                            sp1.Parity = Parity.Odd;
                            break;
                        case "偶校验":
                            sp1.Parity = Parity.Even;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }
                    if (sp1.IsOpen == true)
                    {
                        sp1.Close();
                    }
                    //状态栏设置
                    tsSpNum.Text = "串口号：" + sp1.PortName + "|";
                    tsBaudRate.Text = "波特率：" + sp1.BaudRate + "|";
                    tsDataBits.Text = "数据位：" + sp1.DataBits + "|";
                    tsStopBits.Text = "停止位：" + sp1.StopBits + "|";
                    tsParity.Text = "校验位：" + sp1.Parity + "|";
                    //设置必要控件不可用
                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbStop.Enabled = false;
                    cbParity.Enabled = false;
                    sp1.Open();     //打开串口
                    btnSwitch.Text = "关闭串口";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;
                    return;
                }
            }
            else
            {
                //状态栏设置
                tsSpNum.Text = "串口号：未指定|";
                tsBaudRate.Text = "波特率：未指定|";
                tsDataBits.Text = "数据位：未指定|";
                tsStopBits.Text = "停止位：未指定|";
                tsParity.Text = "校验位：未指定|";
                //恢复控件功能
                //设置必要控件不可用
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;
                sp1.Close();                    //关闭串口
                btnSwitch.Text = "打开串口";
                tmSend.Enabled = false;         //关闭计时器
            }
        }
        //清空按钮
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";      
        }
        //退出按钮
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

       //关闭时事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIFILE.Profile.SaveProfile();
            sp1.Close();
        }
        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (radio1.Checked == true)
            {
                //正则匹配
                string patten = "[0-9a-fA-F]|\b|0x|0X| "; 
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if (m.Success)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = false;
            }
        }
        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }
        private void btnSave_Click(object sender, EventArgs e)
        {

            //设置各“串口设置”
            string strBaudRate = cbBaudRate.Text;
            string strDateBits = cbDataBits.Text;
            Int32 iBaudRate = Convert.ToInt32(strBaudRate);
            Int32 iDateBits = Convert.ToInt32(strDateBits);

           Profile.G_BAUDRATE = iBaudRate + "";     
            Profile.G_DATABITS = iDateBits + "";    
            switch (cbStop.Text)        
            {
                case "1":
                    Profile.G_STOP = "1";
                    break;
                case "1.5":
                    Profile.G_STOP = "1.5";
                    break;
                case "2":
                    Profile.G_STOP = "2";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }
            switch (cbParity.Text)       
            {
                case "无":
                    Profile.G_PARITY = "NONE";
                    break;
                case "奇校验":
                    Profile.G_PARITY = "ODD";
                    break;
                case "偶校验":
                    Profile.G_PARITY = "EVEN";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }
            Profile.SaveProfile();
        }
        //定时器
        private void tmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
            string strSecond = txtSecond.Text;
            try
            {
                int isecond = int.Parse(strSecond) * 1000;//Interval以微秒为单位
                tmSend.Interval = isecond;
                if (tmSend.Enabled == true)
                {
                    btnSend.PerformClick();
                }
            }
            catch
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！", "Error");
            }
        }
        private void txtSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            string patten = "[0-9]|\b"; //“\b”：退格键            
            Regex r = new Regex(patten);
            Match m = r.Match(e.KeyChar.ToString());

            if (m.Success)
            {
                e.Handled = false; 
            }
            else
            {
                e.Handled = true;
            }
        }
        bool sendornot;
        private void btnSendCli_Click(object sender, EventArgs e)
        {
            sendornot = true;            
        }
        private void btnStopCli_Click(object sender, EventArgs e)
        {
            sendornot = false;
        }
        private void SendCle_Click(object sender, EventArgs e)
        {
            recMsg.Text = "";
        }
    }
}
