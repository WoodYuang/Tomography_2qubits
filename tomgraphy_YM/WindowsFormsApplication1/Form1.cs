using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Threading;
using System.IO;
using System.IO.Ports;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //串口通信声明
        string returnflag;
        bool tryagain; //防止串口不稳定出错

        //四个波片代号
        int rotnum;
        int point;  //判断id800读书是否有效的指针

        //波片架位置
        double oriposition1 = 49.2, oriposition2 = 100.15, oriposition3 = -24.3, oriposition4 = 19.8, position7H = 43.5, position7V = -1, position7HV = 21.1, oriposition5 = 153, oriposition6 =191;//0度为黑 //0度为亮
        double[] toposition1 = new double[16];
        double[] toposition2 = new double[16];
        double[] toposition3 = new double[16];
        double[] toposition4 =new double[16];
        double[] beposition1 = new double[16];
        double[] beposition2 = new double[16];
        double[] beposition3 = new double[16];
        double[] beposition4 = new double[16];
        double[] pocvnd=new double[12];
        double nowposition1, nowposition2, nowposition3, nowposition4, nowposition5, nowposition6, nowposition7;


        //ID800声明
        ID800 id800;
        private delegate void UIChange();  //委托类型
        double delay, coinWin;
        int valid0;
        private bool try0 = true;


        //ID800初始化函数
        private void ID800_ini()
        {
            int channel = 1 + 2;        //确定只用2个通道，二进制下为 11
            id800.myChannels = channel;
            id800.myTermination = 1;            //确定为50欧姆
            id800.myIntegralTime = new TimeSpan((long)Math.Round(double.Parse(coin_T.Text) * 10000));
        }


        /// <summary>
        /// 串口断开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SP_cut_Click(object sender, EventArgs e)
        {
            if (myserialPort.IsOpen == false)
                return;

            try
            {
                myserialPort.Close();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
            finally
            {
                if (myserialPort.IsOpen == true)
                {
                    SP_situation.Text = "串口已连接";
                    SP_situation.ForeColor = Color.Green;
                }
                else
                {
                    SP_situation.Text = "串口未连接";
                    SP_situation.ForeColor = Color.Red;
                }
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //参数读入
            delay = Convert.ToDouble(coin_delay.Text.Trim());
            coinWin = Convert.ToDouble(coin_win.Text.Trim());
            int IntegralTime = int.Parse(coin_T.Text);
            id800.start();
            long Integralticks = Convert.ToInt64(IntegralTime * TimeSpan.TicksPerMillisecond);
            //符合
            int channel1 = 0;
            int channel2 = 0;
            int coin = 0;
                       
            for (int i=0; i<16; i++) {
                //转动波片1
                nowposition1 = rotate(oriposition1+toposition1[i], 1);
                theta1.Text = nowposition1.ToString();

                ////转动波片2
                nowposition2 = rotate(oriposition2+toposition2[i], 2);
                theta2.Text = nowposition2.ToString();

                ////转动波片3
                nowposition3 = rotate(oriposition3+toposition3[i], 3);
                theta3.Text = nowposition3.ToString();

                ////转动波片4
                nowposition4 = rotate(oriposition4+toposition4[i], 4);
                theta4.Text = nowposition4.ToString();


                //波片转完等待0.1秒
                for (int ttt = 0; ttt <= 200000; ttt++)
                {
                    System.Windows.Forms.Application.DoEvents(); //一次DoEvents()大约0.5微秒
                }

                //ID800测量
                //符合                
                do
                {
                    try
                    {
                        id800.trytry(ref channel1, ref channel2, ref coin, delay, coinWin, IntegralTime, ref valid0);
                        point = 1;
                    }
                    catch
                    {
                        point = 0;
                    }

                    if (point == 1)
                    {
                        textBox1.Text = textBox1.Text + coin.ToString() + "\r\n";
                        progressBar1.Value = i;
                        break;
                    }
                    else { }
                } while (true);

            }//4次循环测量
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 串口数据采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myserialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int spn = myserialPort.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致  
            byte[] mybyte = new byte[spn];
            string returnflaghere;
            try
            {
                if (spn > 0)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    myserialPort.Read(mybyte, 0, spn);
                    myserialPort.DiscardInBuffer();
                    returnflaghere = "";
                    foreach (byte inbyte in mybyte)
                    {
                        returnflaghere += Convert.ToChar(inbyte);
                    }
                    returnflag += returnflaghere;
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
      
        
        
        
        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SP_con_Click(object sender, EventArgs e)
        {
            if (myserialPort.IsOpen == true)
                return;
            try
            {
                myserialPort.PortName = SP_com.Text;
                myserialPort.BaudRate = Convert.ToInt32(SP_baud.Text.Trim());//"57600"
                myserialPort.Parity = Parity.None;
                myserialPort.DataBits = 8;
                myserialPort.StopBits = StopBits.One;

                myserialPort.Open();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
            finally
            {
                if (myserialPort.IsOpen == true)
                {
                    SP_situation.Text = "串口已连接";
                    SP_situation.ForeColor = Color.Green;
                }
                else
                {
                    SP_situation.Text = "串口未连接";
                    SP_situation.ForeColor = Color.Red;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            id800.clear();
            for (int i = 0; i < 200000; i++)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            id800.close();
        }

        private void 调试部分_Enter(object sender, EventArgs e)
        {

        }

        private void theta1_TextChanged(object sender, EventArgs e)
        {

        }

        private void HH_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1, 1);
            nowposition2 = rotate(oriposition2, 2);
            

            theta1.Text = nowposition1.ToString();         
            theta2.Text = nowposition2.ToString();
            
        }

        private void start_try_Click(object sender, EventArgs e)
        {
            try0 = true;
            delay = Convert.ToDouble(coin_delay.Text.Trim());
            coinWin = Convert.ToDouble(coin_win.Text.Trim());
            int IntegralTime = int.Parse(coin_T.Text);
            id800.start();
            long Integralticks = Convert.ToInt64(IntegralTime * TimeSpan.TicksPerMillisecond);

            do
            {
                //符合
                int channel1 = 0;
                int channel2 = 0;
                int coin = 0;


                id800.trytry(ref channel1, ref channel2, ref coin, delay, coinWin, IntegralTime, ref valid0);
                num1.Text = channel1.ToString();
                num2.Text = channel2.ToString();
                coin_try.Text = coin.ToString();
                num_total.Text = valid0.ToString();

                DateTime StartTime = DateTime.Now;
                do
                {
                    TimeSpan runLength = DateTime.Now.Subtract(StartTime);
                    long runticks = runLength.Ticks;

                    if (runticks > Integralticks)
                    {
                        break;
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
                while (true);
            } while (try0 == true);
        }

        private void HV_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1+45, 1);
            nowposition2 = rotate(oriposition2, 2);


            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
        }

        private void VV_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1+45, 1);
            nowposition2 = rotate(oriposition2, 2);
            nowposition3 = rotate(oriposition3+45, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void VH_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1, 1);
            nowposition2 = rotate(oriposition2, 2);
            nowposition3 = rotate(oriposition3+45, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void HVH_V_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1+22.5, 1);
            nowposition2 = rotate(oriposition2, 2);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
        }

        private void HVHV_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1-22.5, 1);
            nowposition2 = rotate(oriposition2, 2);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();

        }

        private void HiVH_iV_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1+22.5, 1);
            nowposition2 = rotate(oriposition2+45, 2);


            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();

        }

        private void HiVHiV_Click(object sender, EventArgs e)
        {
            nowposition1 = rotate(oriposition1-22.5, 1);
            nowposition2 = rotate(oriposition2+45, 2);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();

        }

        private void stop_try_Click(object sender, EventArgs e)
        {
            try0 = false;
            for (int i = 0; i < 100000; i++)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            id800.close();
        }

        private void num2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Hb_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();

        }

        private void Vb_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3+45, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();


        }

        private void HVB_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3+22.5, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void H_VB_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3-22.5, 3);
            nowposition4 = rotate(oriposition4, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void HiV_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3+22.5, 3);
            nowposition4 = rotate(oriposition4+45, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void H_iV_Click(object sender, EventArgs e)
        {
            nowposition3 = rotate(oriposition3-22.5, 3);
            nowposition4 = rotate(oriposition4+45, 4);

            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //参数读入
            delay = Convert.ToDouble(coin_delay.Text.Trim());
            coinWin = Convert.ToDouble(coin_win.Text.Trim());
            int IntegralTime = int.Parse(coin_T.Text);
            id800.start();
            long Integralticks = Convert.ToInt64(IntegralTime * TimeSpan.TicksPerMillisecond);
            //符合
            int channel1 = 0;
            int channel2 = 0;
            int coin = 0;
            
            for (int i = 0; i < 16; i++)
            {
                //转动波片1
                nowposition1 = rotate(oriposition1 + beposition1[i], 1);
                theta1.Text = nowposition1.ToString();

                ////转动波片2
                nowposition2 = rotate(oriposition2 + beposition2[i], 2);
                theta2.Text = nowposition2.ToString();

                ////转动波片3
                nowposition3 = rotate(oriposition3 + beposition3[i], 3);
                theta3.Text = nowposition3.ToString();

                ////转动波片4
                nowposition4 = rotate(oriposition4 + beposition4[i], 4);
                theta4.Text = nowposition4.ToString();


                //波片转完等待0.1秒
                for (int ttt = 0; ttt <= 200000; ttt++)
                {
                    System.Windows.Forms.Application.DoEvents(); //一次DoEvents()大约0.5微秒
                }

                //ID800测量
                //符合
                do
                {
                    try
                    {
                        id800.trytry(ref channel1, ref channel2, ref coin, delay, coinWin, IntegralTime, ref valid0);
                        point = 1;
                    }
                    catch
                    {
                        point = 0;
                    }

                    if (point == 1)
                    {
                        textBox2.Text = textBox2.Text + coin.ToString() + "\r\n";
                        progressBar2.Value = i;
                        break;
                    }
                    else { }
                } while (true);

            }//4次循环测量
        }

        private void button5_Click(object sender, EventArgs e)
        {

            nowposition1 = rotate(oriposition1, 1);
            nowposition2 = rotate(oriposition2, 2);
            nowposition3 = rotate(oriposition3, 3);
            nowposition4 = rotate(oriposition4, 4);
            nowposition5 = rotate(oriposition5 + 90, 5);
            nowposition6 = rotate(oriposition6, 6);
            nowposition7 = rotate(position7HV, 7);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
            theta5.Text = nowposition5.ToString();
            theta6.Text = nowposition6.ToString();
            theta7.Text = nowposition7.ToString();

            System.Windows.Forms.MessageBox.Show("波片已归零！", "消息");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            id800.clear();
            for (int i = 0; i < 200000; i++)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            id800.close();
        }

        private void button7_Click(object sender, EventArgs e)
        {

            nowposition1 = rotate(oriposition1, 1);
            nowposition2 = rotate(oriposition2, 2);
            nowposition3 = rotate(oriposition3, 3);
            nowposition4 = rotate(oriposition4, 4);
            nowposition5=rotate(oriposition5, 5);
            nowposition6=rotate(oriposition6, 6);//
            nowposition7=rotate(position7V, 7);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
            theta5.Text = nowposition5.ToString();
            theta6.Text = nowposition6.ToString();
            theta7.Text = nowposition7.ToString();

            System.Windows.Forms.MessageBox.Show("波片已归零！", "消息");
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            id800.clear();
            for (int i = 0; i < 200000; i++)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            id800.close();
        }

       

        private void PumpA_Click(object sender, EventArgs e)
        {
             rotate(position7H, 7);
        }

        private void PumpB_Click(object sender, EventArgs e)
        {
            rotate(position7V, 7);
        }

        private void PumpHV_Click(object sender, EventArgs e)
        {
            rotate(position7HV, 7);
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            nowposition1 = rotate(oriposition1, 1);
            nowposition2 = rotate(oriposition2, 2);
            nowposition3 = rotate(oriposition3, 3);
            nowposition4 = rotate(oriposition4, 4);
            nowposition5=rotate(oriposition5+90, 5);
            nowposition6=rotate(oriposition6, 6);
            nowposition7=rotate(position7HV, 7);

            theta1.Text = nowposition1.ToString();
            theta2.Text = nowposition2.ToString();
            theta3.Text = nowposition3.ToString();
            theta4.Text = nowposition4.ToString();
            theta5.Text = nowposition5.ToString();
            theta6.Text = nowposition6.ToString();
            theta7.Text = nowposition7.ToString();

            System.Windows.Forms.MessageBox.Show("波片已归零！", "消息");

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// 转动波片架
        /// </summary>
        /// <param name="position"></param>//要转动的角度
        /// <param name="rotnumx"></param>//要转动哪个波片架
        private double rotate(double position, int rotnumx)
        {
            int rotnum = rotnumx;
            double myposition;
            tryagain = false;

            if (operation(rotnum, rotnum.ToString() + "PA" + position.ToString(), false) == "")
            {
                return -1;
            }

            do
            {
                for (int i = 0; i <= 1000000; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                string retval = operation(rotnum, rotnum.ToString() + "TP", true);
                if (tryagain == true)
                {
                    retval = operation(rotnum, rotnum.ToString() + "TP", true);
                }
                if (tryagain == true)
                {
                    retval = operation(rotnum, rotnum.ToString() + "TP", true);
                }


                if (retval == "")
                    return -1;
                myposition = Convert.ToDouble(retval.Substring((rotnum.ToString() + "TP").Length));
                if (Math.Abs(myposition - position) < 1)
                    break;
            } while (true);
            return myposition;
        }
        private string operation(int myrotnum, string mycmd, bool rtnneeded)
        {
            try
            {
                returnflag = "";
                myserialPort.Write(mycmd.Trim() + "\r" + "\n");
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }

            if (rtnneeded == false)
            {
                return "done";
            }

            int pp = 0;
            do
            {
                for (int i = 0; i <= 10000; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                if (returnflag.Length >= 2)
                {
                    if (returnflag.Substring(returnflag.Length - 2) == "\r" + "\n")
                    {
                        tryagain = false;
                        break;
                    }
                }

                pp++; //防止串口出错，转波片等待10余秒还没结果，自动跳出，再来一次
                if (pp > 2000)
                {
                    tryagain = true;
                    break;
                }

            } while (true);
            return returnflag.Trim();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            coin_T.Text = "1000";
            coin_delay.Text = "13.4";
            coin_win.Text = "3";

            //ID初始化
            id800 = new ID800();
            ID800_ini();


            //读入tom角度文件
            string path0 = AppDomain.CurrentDomain.BaseDirectory;
            string patht;
            string pathb;
            string pathp;
            if (path0.EndsWith("\\"))
            {
                patht = path0;
                pathb = path0;
                pathp = path0;
            }
            else
            {
                patht = path0 + "\\";
                pathb = path0 + "\\";
                pathp = path0 + "\\";
            }
            patht = patht + "\\" + "tom.para";
            pathb = pathb + "\\" + "bell.para";
            pathp = pathp + "\\" + "cvnd.para";
            StreamReader sr = new StreamReader(patht);
            StreamReader srr = new StreamReader(pathb);
            StreamReader srrr = new StreamReader(pathp);

            for (int i = 0; i < 16; i++)
            {

                toposition1[i] = double.Parse(sr.ReadLine().Trim());         //文件第1行波片1角度
                toposition2[i] = double.Parse(sr.ReadLine().Trim());         //文件第2行波片2角度
                toposition3[i] = double.Parse(sr.ReadLine().Trim());         //文件第3行波片3角度
                toposition4[i] = double.Parse(sr.ReadLine().Trim());         //文件第4行波片4角度

            }

            for (int i = 0; i < 16; i++)
            {

                beposition1[i] = double.Parse(srr.ReadLine().Trim());         //文件第1行波片1角度
                beposition2[i] = double.Parse(srr.ReadLine().Trim());         //文件第2行波片2角度
                beposition3[i] = double.Parse(srr.ReadLine().Trim());         //文件第3行波片3角度
                beposition4[i] = double.Parse(srr.ReadLine().Trim());         //文件第4行波片4角度

            }

            for (int i=0;i<12;i++)
            {
                pocvnd[i]= double.Parse(srrr.ReadLine().Trim());
            }

            //初始化串口参数
            string[] myports = SerialPort.GetPortNames();
            for (int i = 0; i <= myports.Length - 1; i++)
            {
                string myport = myports[i];
                SP_com.Items.Add(myport.Trim());
            }
            SP_com.SelectedIndex = 0;
            SP_baud.Items.Add("57600");
            SP_baud.SelectedIndex = SP_baud.Items.IndexOf("57600");
            myserialPort = new SerialPort();

            //初始化SerialPort对象  
            myserialPort.NewLine = "/r/n";
            myserialPort.RtsEnable = true; //放着先看看

            //添加事件注册  
            myserialPort.DataReceived += myserialPort_DataReceived;

            //进度条初始状态
            progressBar1.Value = 0;
            //Tom设置进度条
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 15;

            //进度条初始状态
            progressBar2.Value = 0;
            //bell设置进度条
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 15;
          
        }




    }
}
