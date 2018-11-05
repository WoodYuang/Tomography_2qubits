using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    class ID800
    {
        [DllImport("tdcbase.dll")]
        private static extern double TDC_getVersion();
        [DllImport("tdcbase.dll")]
        private static extern double TDC_getTimebase();
        [DllImport("tdcbase.dll")]
        private static extern int TDC_switchTermination(int on);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_init(int deciceID);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_deInit();
        [DllImport("tdcbase.dll")]
        private static extern int TDC_enableChannels(int channelMask);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_setTimestampBufferSize(int size);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_getLastTimestamps(int reset, ref long timestamps, ref byte channels, ref int valid);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_getCoincCounters(ref int data);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_getHistogram(int chanA, int chanB, int reset, ref int data, ref int count, ref int tooSmall, ref int tooLarge, ref int eventsA, ref int eventsB, ref long expTime);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_setHistogramParams(int binWidth, int binCount);
        [DllImport("tdcbase.dll")]
        private static extern int TDC_clearAllHistograms();

        public double myTimebase { get; set; }
        public int myTermination { get; set; }               //1为50欧姆内阻；0为高阻
        public int myChannels { get; set; }
        public bool working { get; set; }
        public const int BufferSize = 1000000;
        public TimeSpan myIntegralTime { get; set; }
        private delegate void UIChange();


        //ID800初始化
        public ID800()
        {
            //TDC_deInit();
            myTermination = 1;    //默认为50欧姆内阻
            myChannels = 3;      //默认为打开2个通道 11
            working = false;
            myTimebase = TDC_getTimebase();       //@brief Get Time Base
            TDC_init(-1); //@brief Initialize and Start
        }   //构造函数

        public void close()
        {
            TDC_clearAllHistograms();
            TDC_deInit();//@brief Disconnect and uninitialize

        }

        public void start()
        {
            TDC_switchTermination(myTermination);    //@brief Switch Input Termination
            TDC_setTimestampBufferSize(BufferSize);  //@brief Set Timestamp Buffersize
            TDC_enableChannels(myChannels);    ///@brief Enable TDC Channels
        }

        public void clear()
        {
            long[] tem1 = new long[BufferSize];
            byte[] tem2 = new byte[BufferSize];
            int valid = 0;
            TDC_getLastTimestamps(1, ref tem1[0], ref tem2[0], ref valid);//@brief Retreive Last Timestamp Values
        }

        //将数据转化为单道、符合。
        public void trytry(ref int channel1, ref int channel2, ref int coin0, double delay0, double coinWin, int IntegralTime, ref int valid0)
        {
            coin0 = 0;
            valid0 = 0;
            long[] stamp = new long[BufferSize + 1];
            byte[] channels = new byte[BufferSize + 1];
            long[] stamp0 = new long[BufferSize + 1];
            byte[] channels0 = new byte[BufferSize + 1];

            requiredata(out stamp, out channels, out valid0, delay0, IntegralTime);
            channel1 = 0;
            channel2 = 0;
       //单道测量
            for (int i = 0; i < valid0; i++)
            {
                if (channels[i] == 0)
                    channel1++;
                if (channels[i] == 1)
                    channel2++;
            }


            stamp.CopyTo(stamp0, 0);
            channels.CopyTo(channels0, 0);

            long coinWinbin = (long)Math.Round(coinWin * 0.000000001 / myTimebase);    //符合窗口以纳秒为单位
      //符合测量
            int coin = 0;
            for (int i = 0; i < valid0; i++)
            {
                if (channels0[i] == 0)
                {
                    for (int j = i + 1; j < valid0; j++)
                    {
                        if (channels0[j] == 1)
                        {
                            if (stamp0[j] - stamp0[i] <= coinWinbin)
                            {
                                coin++;
                                channels0[j] = 255;
                                break;
                            }
                            else
                                break;
                        }
                    }
                }
            }
            coin0 = coin;
        }

        //数据获取
        public void requiredata(out long[] stamp, out byte[] channels, out int valid, double delay0, int IntegralTime)
        {
            working = true;

            
            stamp = new long[BufferSize + 1];
            channels = new byte[BufferSize + 1];
            valid = 0;
            long[] tem1 = new long[BufferSize];
            byte[] tem2 = new byte[BufferSize];
            long[] stamp1 = new long[BufferSize + 1];
            byte[] channels1 = new byte[BufferSize + 1];

            long periodticks = Convert.ToInt64(IntegralTime * TimeSpan.TicksPerMillisecond);
            TDC_getLastTimestamps(1, ref tem1[0], ref tem2[0], ref valid);      //清除数据
            DateTime StartTime = DateTime.Now;
            do
            {
                TimeSpan runLength = DateTime.Now.Subtract(StartTime);
                long runticks = runLength.Ticks;

                if (runticks > periodticks)
                {
                    break;
                }
                Application.DoEvents();
            }
            while (true);

            TDC_getLastTimestamps(1, ref stamp[0], ref channels[0], ref valid);//或得时间戳

            delaysort(stamp, channels, delay0, valid);
        }
        //数据获取结束

        public void delaysort(long[] st, byte[] ch, double delay0, long valid)

        {
            long pcs0 = (long)Math.Round(delay0 * 0.000000001 / myTimebase);
            long pcs00 = (long)Math.Round(-1.5 * 0.000000001 / myTimebase);//延迟修正项
            pcs0 = pcs0 + pcs00;
            int i;
            for (i = 0; i < valid; i++)
            {
                if (ch[i] == 0)             //CH1相对CH2的延迟
                    st[i] += pcs0;
            }


            sort(st, ch, 0, valid - 1);  //排序
        }

        //快速排序模块
        private void swap(ref long l, ref long r)
        {
            long temp;
            temp = l;
            l = r;
            r = temp;
        }

        private void swapb(ref byte l, ref byte r)
        {
            byte temp;
            temp = l;
            l = r;
            r = temp;
        }

        private void sort(long[] array1, byte[] array2, long low, long high)
        {
            long pivot;
            byte pivotb;
            long l, r;
            long mid;
            if (high <= low)
                return;
            else if (high == low + 1)
            {
                if (array1[low] > array1[high])
                {
                    swap(ref array1[low], ref array1[high]);
                    swapb(ref array2[low], ref array2[high]);
                }
                return;
            }
            mid = (low + high) >> 1;
            pivot = array1[mid];
            pivotb = array2[mid];
            swap(ref array1[low], ref array1[mid]);
            swapb(ref array2[low], ref array2[mid]);
            l = low + 1;
            r = high;
            do
            {
                while (l <= r && array1[l] < pivot)
                    l++;
                while (array1[r] >= pivot)
                    r--;
                if (l < r)
                {
                    swap(ref array1[l], ref array1[r]);
                    swapb(ref array2[l], ref array2[r]);
                }
            } while (l < r);
            array1[low] = array1[r];
            array2[low] = array2[r];
            array1[r] = pivot;
            array2[r] = pivotb;
            if (low + 1 < r)
                sort(array1, array2, low, r - 1);
            if (r + 1 < high)
                sort(array1, array2, r + 1, high);
        }
        //快速排序模块END

    }
}
