using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Animation;
using Windows.Media.PlayTo;
using System.Threading;

using System.Runtime.InteropServices;


//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace MotorCade_2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //定义变量
        private StreamSocket clientSocket = null;
        private HostName serverHost = null;
        private bool connected = false;
        private bool closing = false;
        private int speed = 0;   //速度默认为0
        private int degree = 0;//
        private double rectanglescale = 0;
        private double distance = 20;
//        private static string Status_Parameter;// 机器人反馈参数
        Storyboard Turn_Storyboard = null;
        System.Threading.Timer tm = null;
        System.Threading.Timer tm0 = null;
        private int i = 1;
//        Timer tm = new Timer(TimerCall, 3, 1000, 1000);
        //Console.WriteLine("period is 1000");
        //        Timer tm = new Timer(new TimerCallback(TimerCall), 3, 1000, 1000);
        /*
                public Timer(
                   TimerCallback callback,//所需调用的方法
                   object state,//传递给callback的参数
                   int dueTime,//多久后开始调用callback
                   int period//调用此方法的时间间隔
                );// 如果 dueTime 为0，则 callback 立即执行它的首次调用。如果 dueTime 为 Infinite，则 callback 不调用它的方法。计时器被禁用，但使用 Change 方法可以重新启用它。如果 period 为0或 Infinite，并且 dueTime 不为 Infinite，则 callback 调用它的方法一次。计时器的定期行为被禁用，但使用 Change 方法可以重新启用它。如果 period 为零 (0) 或 Infinite，并且 dueTime 不为 Infinite，则 callback 调用它的方法一次。计时器的定期行为被禁用，但使用 Change 方法可以重新启用它。
                在创建计时器之后若想改变它的period和dueTime，我们可以通过调用Timer的Change方法来改变：
        */
        //        ThreadPoolBoundHandle lcthread;

        //System.Threading.Timer mTimer = null;
        //System.Threading.ThreadLocal<string> udplistener = new System.Threading.ThreadLocal<string>(StartUDPListener);
        //StartUDPListener();
        // private static Parameter Status_Parameter = new Parameter("aa");

        const int MOT_NUM = 19;
        const int FOR_NUM = 7;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ArisData
        {
            /* Motor Control */
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = MOT_NUM)]
            public int[] target_pos;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = MOT_NUM)]
            public int[] feedback_pos;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = MOT_NUM)]
            public int[] target_vel;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = MOT_NUM)]
            public int[] feedback_vel;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = MOT_NUM)]
            public Int16[] target_cur;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = MOT_NUM)]
            public Int16[] feedback_cur;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = MOT_NUM)]
            public byte[] cmd;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = MOT_NUM)]
            public byte[] mode;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = MOT_NUM)]
            public UInt16[] statusword;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = MOT_NUM)]
            public Int16[] ret;
            /* Force Sensor */
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] Fx;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] Fy;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] Fz;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] Mx;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] My;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = FOR_NUM)]
            public float[] Mz;

            /* IMU */
            [MarshalAs(UnmanagedType.R4)]
            public float yaw;
            [MarshalAs(UnmanagedType.R4)]
            public float pitch;
            [MarshalAs(UnmanagedType.R4)]
            public float roll;
            [MarshalAs(UnmanagedType.I4)]
            public int count;


        };



        [DllImport("ArisUDPClient.dll", EntryPoint = "StartUDPListener", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern void StartUDPListener();

        [DllImport("ArisUDPClient.dll", EntryPoint = "CloseUDPListener", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern void CloseUDPListener();

        [DllImport("ArisUDPClient.dll", EntryPoint = "GetExchangeData", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetExchangeData(ref ArisData data);

        static ArisData data;


        public MainPage()
        {

            this.InitializeComponent();

            Donghua(0, 0.5, 0, 15, "speed");//主要作用是将rectangle变成Height = 0。

        }
//        public void TimerCall(object b)
//        {

//             Status_Parameter =  b.ToString();
            //try
            //{
                //this.Parameters.Text = b.ToString();
                
            //}
            //catch (System.Exception)
            //{ }
//        }



        /// <summary>
        /// 动画函数。
        /// </summary>
        private void Donghua(double parameter, double time,int dspeed, int dangle, string i)
        {
            if(i == "speed")
            {
                Turn_Storyboard = new Storyboard();

                Turn_Storyboard.Children.Clear();
                // 创建时间线
                DoubleAnimation scale = new DoubleAnimation();
                // 动画持续5秒钟
                scale.Duration = TimeSpan.FromSeconds(time);
                // 设置动画的起始值与最终值
                scale.From = rectanglescale;
                rectanglescale -= parameter;
                speed += dspeed;
                scale.To = rectanglescale;

                // 设置动画的目标对象
                Storyboard.SetTarget(scale, this.scalerectangle);
                // 设置动画的目标属性
                Storyboard.SetTargetProperty(scale, "(ScaleTransform.ScaleY)");//TranslateTransform.X
                                                                               // 将时间线添加到Storyboard中
                Turn_Storyboard.Children.Add(scale);
                Turn_Storyboard.Begin();
            }
            if(i == "angle")
            {
                Turn_Storyboard = new Storyboard();

                Turn_Storyboard.Children.Clear();
                // 创建时间线
                DoubleAnimation rotate = new DoubleAnimation();
                // 动画持续5秒钟
                rotate.Duration = TimeSpan.FromSeconds(2);
                rotate.From = degree;
                degree -= dangle;
                rotate.To = degree;
                // 设置动画的目标对象
                Storyboard.SetTarget(rotate, this.rotatepanel);
                // 设置动画的目标属性
                Storyboard.SetTargetProperty(rotate, "(RotateTransform.Angle)");//TranslateTransform.X
                                                                                // 将时间线添加到Storyboard中
                Turn_Storyboard.Children.Add(rotate);
                Turn_Storyboard.Begin();
                this.AngleBox.Text = (degree).ToString();

            }
            if(i == "moveY")
            {
//                distance = 20;
                double dy = parameter;
                double YY = 0;
                Turn_Storyboard = new Storyboard();
                Turn_Storyboard.Children.Clear();
                // 创建时间线
                DoubleAnimation scale = new DoubleAnimation();
                // 动画持续5秒钟
                scale.Duration = TimeSpan.FromSeconds(time);
                // 设置动画的起始值与最终值
                YY = this.bottomstackpanel.Y;
                scale.From = YY;
                scale.To = YY - dy;
                // 设置动画的目标对象
                Storyboard.SetTarget(scale, this.bottomstackpanel);
                // 设置动画的目标属性
                Storyboard.SetTargetProperty(scale, "(TranslateTransform.Y)");//TranslateTransform.X                                                                              // 将时间线添加到Storyboard中
                Turn_Storyboard.Children.Add(scale);
                Turn_Storyboard.Begin();
            }
            if (i == "moveX")
            {
//                distance = 20;
                double dx = parameter;
//                double dy = -distance;
                double XX = 0;
//                double YY = 0;
                Turn_Storyboard = new Storyboard();
                Turn_Storyboard.Children.Clear();
                // 创建时间线
                DoubleAnimation scale = new DoubleAnimation();
                // 动画持续5秒钟
                scale.Duration = TimeSpan.FromSeconds(time);
                // 设置动画的起始值与最终值
                XX = this.bottomstackpanel.X;
                scale.From = XX;
                scale.To = XX + dx;
                // 设置动画的目标对象
                Storyboard.SetTarget(scale, this.bottomstackpanel);
                // 设置动画的目标属性
                Storyboard.SetTargetProperty(scale, "(TranslateTransform.X)");//TranslateTransform.X                                                                              // 将时间线添加到Storyboard中
                Turn_Storyboard.Children.Add(scale);
                Turn_Storyboard.Begin();
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            this.Send_Effect_Element.Play();//音效
            if (connected)
            {
                StatusText.Text = "Already connected";
                return;
            }

            try
            {

                StatusText.Text = "Trying to connect ...";
                clientSocket = new StreamSocket();
                serverHost = new HostName(ServerIP.Text);
                // 发起连接
                await clientSocket.ConnectAsync(serverHost, ServerPort.Text);
                connected = true;
                StatusText.Text = "Connection established";
                //开始接受数据
                if (clientSocket.Information != null)
                {
                    await BeginReceived();
                }
            }
            catch (Exception exception)
            {
                StatusText.Text = "Connect failed with error: " + exception.Message;
                // Could retry the connection, but for this simple example
                // just close the socket.
                //7.22 xue 冲刷 SteamSocket 内存
                clientSocket = new StreamSocket();
                closing = true;
                // the Close method is mapped to the C# Dispose
                clientSocket.Dispose();
                clientSocket = null;
                connected = false;
            }
        }
        /// <summary>
        /// 断开网络连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            this.Send_Effect_Element.Play();//音效
            clientSocket = new StreamSocket();
            closing = true;
            clientSocket.Dispose();
            clientSocket = null;
            connected = false;
            StatusText.Text = "Socket is disconnected.";
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns></returns>
        private async Task BeginReceived()
        {

            //绑定已连接的StreamSocket到DataReader
            DataReader reader = new DataReader(clientSocket.InputStream);
            while (true)
            {
                try
                {
                    byte[] tempByteArr;

                    //获取数据长度
                    tempByteArr = new byte[4];
                    await reader.LoadAsync(sizeof(uint));
                    reader.ReadBytes(tempByteArr);
                    uint dataLength = System.BitConverter.ToUInt32(tempByteArr, 0);
                    //StatusText.Text = dataLength.ToString();

                    //获取msgID
                    tempByteArr = new byte[4];
                    await reader.LoadAsync(sizeof(int));
                    reader.ReadBytes(tempByteArr);
                    int msgID = System.BitConverter.ToInt32(tempByteArr, 0);

                    //读完数据头
                    tempByteArr = new byte[32];
                    await reader.LoadAsync(32);
                    reader.ReadBytes(tempByteArr);

                    if (dataLength > 0)
                    {
                        //读取数据正文
                        tempByteArr = new byte[dataLength];
                        await reader.LoadAsync(dataLength);
                        reader.ReadBytes(tempByteArr);
                        StatusText.Text = System.Text.UnicodeEncoding.UTF8.GetString(tempByteArr, 0, int.Parse(dataLength.ToString()));
                    }
                    else
                    {
                        StatusText.Text = "";
                    }
                }
                catch (Exception exception)
                {
                    // If this is an unknown status, 
                    // it means that the error is fatal and retry will likely fail.
                    if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                    {
                        throw;
                    }

                    StatusText.Text = "Send data or receive failed with error: " + exception.Message;
                    // Could retry the connection, but for this simple example
                    // just close the socket.

                    closing = true;
                    clientSocket.Dispose();
                    clientSocket = null;
                    connected = false;
                }
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sData"></param>
        private async void SendMsg(byte[] sData)
        {
            if (!connected)
            {
                StatusText.Text = "Must be connected to send!";
                return;
            }

            try
            {
                //StatusText.Text = "Trying to send data ...";

                byte[] sendData = ConvSendMsg(sData, 0);
                DataWriter writer = new DataWriter(clientSocket.OutputStream);
                //把数据写入到发送流
                writer.WriteBytes(sendData);
                //异步发送
                await writer.StoreAsync();

                //显示发送的消息内容
                //StatusText.Text = System.Text.UnicodeEncoding.UTF8.GetString(sendData, 40, sData.Length) + " was sent";

                // detach the stream and close it
                writer.DetachStream();
                writer.Dispose();

            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                StatusText.Text = "Send data or receive failed with error: " + exception.Message;
                // Could retry the connection, but for this simple example
                // just close the socket.
                closing = true;
                clientSocket.Dispose();
                clientSocket = null;
                connected = false;
            }
        }

        /// <summary>
        /// 封装发送的数据包
        /// </summary>
        /// <param name="sendData">数据内容</param>
        /// <param name="dataLength">数据长度</param>
        /// <param name="msgID">msgID</param>
        /// <returns>返回封装好的数据包</returns>
        private byte[] ConvSendMsg(byte[] sendData, int msgID, Int64 msgType = 1)
        {
            //数据包格式为 数据大小 + msgID + type + 保留字段 + 保留字段 + 用户自定义数据 + 数据内容

            // 0-3  字节，  unsigned int 代表数据大小
            int dataSize = sendData == null ? 0 : sendData.Length + 1;
            byte[] bLength = System.BitConverter.GetBytes(dataSize);

            // 4-7  字节，  int          代表msgID
            byte[] bID = System.BitConverter.GetBytes(msgID);

            // 8-15 字节，  long long    代表type
            byte[] bType = System.BitConverter.GetBytes(msgType);

            // 16-23字节，  long long    目前保留，准备用于时间戳
            // 24-31字节，  long long    目前保留，准备用于时间戳
            // 32-39字节，  long long    用户可以自定义的8字节数据
            byte[] bReserved = new byte[24];

            //组装数据包`
            byte[] b = new byte[dataSize + 40];
            Array.Copy(bLength, 0, b, 0, 4);
            Array.Copy(bID, 0, b, 4, 4);
            Array.Copy(bType, 0, b, 8, 8);
            if (sendData != null)
            {
                Array.Copy(sendData, 0, b, 40, sendData.Length);
            }
            return b;
        }

        /// <summary>
        /// 发送用户输入的命令及参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            this.Send_Effect_Element.Play();//音效
            byte[] Pm = System.Text.UnicodeEncoding.UTF8.GetBytes(command.Text);
            SendMsg(Pm);
            this.command.Text = " ";
        }

        /// <summary>
        /// 发送Robots自带的基本命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Common_Click(object sender, RoutedEventArgs e)
        {
            this.Send_Effect_Element.Play();//音效
            Button btn = sender as Button;
            if (btn == null)
            {
                return;
            }
            string basicCmd;
            string btnContent = btn.Content.ToString();
            switch (btnContent)
            {
                case "Start":
                    basicCmd = "start";
                    break;
                case "Exit":
                    basicCmd = "exit";
                    break;
                case "Enable":
                    basicCmd = "en -a";
                    break;
                case "Disable":
                    basicCmd = "ds -a";
                    break;
                case "GoHome1":
                    basicCmd = "hm -f";
                    break;
                case "GoHome2":
                    basicCmd = "hm -s";
                    break;
                case "Recover1":
                    basicCmd = "rc -f";
                    break;
                case "Recover2":
                    basicCmd = "rc -s";
                    break;
                default:
                    basicCmd = "";
                    break;
            }
            byte[] sendBytes = System.Text.UnicodeEncoding.UTF8.GetBytes(basicCmd);
            SendMsg(sendBytes);
            //            wkNum.Value = 1;
        }

        /// <summary>
        /// 加速指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedUp_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0.3" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            Donghua(0.33, 2, 10, 0, "speed");//加速，2s
            this.SpeedBox.Text = speed.ToString();
        }

        /// <summary>
        /// 减速指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedDown_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=-0.3" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            Donghua(-0.33, 2, -10, 0, "speed");//减速，2s
            this.SpeedBox.Text = speed.ToString();

        }
        /// <summary>
        /// 顺时针旋转指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClockWise_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0 -b=-0.2618" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            Donghua(0, 2,0, -15, "angle");//顺时针旋转角度，2s
            this.AngleBox.Text = (degree).ToString();
        }

        /// <summary>
        /// 逆时针旋转指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnticlockWise_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0 -b=0.2618" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            Donghua(0, 2, 0, 15, "angle");//逆时针旋转角度，2s
            this.AngleBox.Text = (degree).ToString();
        }

        /// <summary>
        /// 前移动指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Foreward_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0.3" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            double rad = (degree) / 180.0 * 3.14159;
            double dx = distance * Math.Cos(rad);//= distance * (Math.Cos(degree-180));//弧度制
            double dy = -distance * (Math.Sin(rad));
            Donghua(-dy, 1, 0, 0, "moveX");
            Donghua(dx, 1, 0, 0, "moveY");
        }

        /// <summary>
        /// 后移动指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=-0.3" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            double rad = (degree) / 180.0 * 3.14159;
            double dx = distance * Math.Cos(rad);//= distance * (Math.Cos(degree-180));//弧度制
            double dy = -distance * (Math.Sin(rad));

            Donghua(dy, 1, 0, 0, "moveX");
            Donghua(-dx, 1, 0, 0, "moveY");
//            Donghua(-20, 1, 0, 0, "moveY");
        }

        /// <summary>
        /// 右侧移动指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightWard_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0.3" + " -a=-1.57" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            double rad = (degree) / 180.0 * 3.14159;
            double dx = distance * Math.Cos(rad);//= distance * (Math.Cos(degree-180));//弧度制
            double dy = -distance * (Math.Sin(rad));
            Donghua(dx, 1, 0, 0, "moveX");
            Donghua(dy, 1, 0, 0, "moveY");
        }

        /// <summary>
        /// 左侧移动指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftWard_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            string wkCmd;
            wkCmd = "wk -d=0.3" + " -a=1.57" + " -h=0.05" + " -n=1";
            command.Text = wkCmd;
            double rad = (degree) / 180.0 * 3.14159;
            double dx = -distance * Math.Cos(rad);//= distance * (Math.Cos(degree-180));//弧度制
            double dy = distance * (Math.Sin(rad));
            Donghua(dx, 1, 0, 0, "moveX");
            Donghua(dy, 1, 0, 0, "moveY");
        }

        /// <summary>
        /// 复位指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Effect_Element.Play();//音效
            degree = 0;
            Donghua(0, 0.1, 0, 0, "angle");//复位
            this.bottomstackpanel.X = 0;
            this.bottomstackpanel.Y = 0;
            this.AngleBox.Text = (degree).ToString();
            this.SpeedBox.Text = speed.ToString();

        }

        private void Turn_Back(object sender, RoutedEventArgs e)
        {
            this.Parameters.Text = "null";
            //StartUDPListener();

            this.tm = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallback), null, TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(1000));
            this.tm0 = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallback0), null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(0));
        }
        private async void TimerCallback(object state)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    Parameters.Text = "你好啊！";

                    GetExchangeData(ref data);
                    Parameters.Text = " Time :" + data.count.ToString(); //(i++).ToString();// 
                    Parameters.Text = data.cmd[i].ToString().PadLeft(6, ' ') + " ";
                    //Console.Write(data.statusword[i].ToString().PadLeft(6, ' ') + " ");
                }
            );

        }
        private void TimerCallback0(object state)
        {
            StartUDPListener();
        }
    }
}

