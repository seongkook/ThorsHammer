using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;

namespace FlypticControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serial;
        double maxThrottle = 0;
        double minThrottle = 0;
        double xPos = 0; double yPos = 0; double zPos = 0;
        double x1 = 0; double x2 = 0;
        double y1 = 0; double y2 = 0;
        double z1 = 0; double z2 = 0;
        bool isRunning = false;

        static int[] motorIdx = new int[] {3,2,1,4,6,5}; // Order of the motor connections

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UITimer_Tick(object sender, EventArgs e)
        {
            if(UDPDebug != null)
            {
                UDPDebug.Content = UDPDebugMsg + "\n" + forceVector.X.ToString("0.00") + "\t" + forceVector.Y.ToString("0.00") + "\t" + forceVector.Z.ToString("0.00");
            }

            if(XYForceLine != null)
            {
                double boxSize = 150;
                XYForceLine.X2 = XYForceLine.X1 + forceVector.X * boxSize / 2;
                XYForceLine.Y2 = XYForceLine.Y1 + forceVector.Y * boxSize / 2;
                XZForceLine.X2 = XZForceLine.X1 + forceVector.X * boxSize / 2;
                XZForceLine.Y2 = XZForceLine.Y1 + forceVector.Z * boxSize / 2;
            }

            if(sendCheck.IsChecked.Value)
            {
                xPos = Math.Min(1, Math.Max(-1, forceVector.X));
                yPos = Math.Min(1, Math.Max(-1, forceVector.Y));
                zPos = Math.Min(1, Math.Max(-1, forceVector.Z));
                
                ProcessUDPThrottle();
            }
        }

        #region Serial Connection
        private void OpenSerialport()
        {
            serial = new SerialPort();
            serial.PortName = portBox.Text;
            serial.BaudRate = 115200;

            serial.DtrEnable = true;
            serial.RtsEnable = true;            

            if (!serial.IsOpen)
            {
                serial.Open();
                isRunning = true;
            }
            else
            {           
                serial.Close();
                isRunning = false;
            }
        }        
        #endregion

        #region Throttle Control    
        void ProcessThrottle()
        {
            if(isRunning)
            {
                // Organize message and send through serialport
                xPos = (Xslider.Value - 50) * 0.02;
                yPos = (Yslider.Value - 50) * 0.02;
                zPos = (Zslider.Value - 50) * 0.02;

                x1 = xPos < 0 ? GetThrottle(-xPos) : minThrottle;
                x2 = xPos > 0 ? GetThrottle(xPos) : minThrottle;
                y1 = yPos < 0 ? GetThrottle(-yPos) : minThrottle;
                y2 = yPos > 0 ? GetThrottle(yPos) : minThrottle;
                z1 = zPos < 0 ? GetThrottle(-zPos) : minThrottle;
                z2 = zPos > 0 ? GetThrottle(zPos) : minThrottle;

                SendThrottle();
            }
        }

        void ProcessUDPThrottle()
        {
            if (isRunning)
            {
                // Organize message and send through serialport
                x1 = xPos < 0 ? GetThrottle(-xPos) : minThrottle;
                x2 = xPos > 0 ? GetThrottle(xPos) : minThrottle;
                y1 = yPos < 0 ? GetThrottle(-yPos) : minThrottle;
                y2 = yPos > 0 ? GetThrottle(yPos) : minThrottle;
                z1 = zPos < 0 ? GetThrottle(-zPos) : minThrottle;
                z2 = zPos > 0 ? GetThrottle(zPos) : minThrottle;

                SendThrottle();
            }
        }

        void SetIdle()
        {
            x1 = minThrottle;
            x2 = minThrottle;
            y1 = minThrottle;
            y2 = minThrottle;
            z1 = minThrottle;
            z2 = minThrottle;
            SendThrottle();
        }

        void SetStop()
        {
            x1 = 0; x2 = 0;
            y1 = 0; y2 = 0;
            z1 = 0; z2 = 0;
            SendThrottle();
        }
        
        private void SendThrottle()
        {
            byte[] byteMsg = new byte[7];
            byteMsg[0] = 255;
            byteMsg[motorIdx[0]] = (byte)x1;
            byteMsg[motorIdx[1]] = (byte)x2;
            byteMsg[motorIdx[2]] = (byte)y1;
            byteMsg[motorIdx[3]] = (byte)y2;
            byteMsg[motorIdx[4]] = (byte)z1;
            byteMsg[motorIdx[5]] = (byte)z2;            

            if (serial.IsOpen)
            {
                serial.Write(byteMsg, 0, byteMsg.Length);                                
            }
        }

        double GetThrottle(double portion)
        {
            return minThrottle + portion * (maxThrottle - minThrottle);
        }
        #endregion
        
        #region Mouse Event Handlers
        bool box1Selected = false;
        bool box2Selected = false;
        private void box1_MouseDown(object sender, MouseButtonEventArgs e)
        {        
            box1Selected = true;
            box2Selected = false;
        }

        private void box2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            box1Selected = false;
            box2Selected = true;
        }

        private void box1_MouseMove(object sender, MouseEventArgs e)
        {
            if (box1Selected)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point pt = e.GetPosition((Rectangle)sender);
                    double xPos = pt.X / box1.Width;
                    double yPos = pt.Y / box1.Height;
                    Xslider.Value = (int)(xPos * 100);
                    Yslider.Value = (int)(yPos * 100);
                    ProcessThrottle();
                }
            }
        }

        private void box2_MouseMove(object sender, MouseEventArgs e)
        {
            if (box2Selected)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point pt = e.GetPosition((Rectangle)sender);
                    double xPos = pt.X / box2.Width;
                    double yPos = pt.Y / box2.Height;
                    Xslider.Value = (int)(xPos * 100);
                    Zslider.Value = (int)(yPos * 100);
                    ProcessThrottle();
                }
            }
        }

        private void box1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((Rectangle)sender);
            Xslider.Value = (Xslider.Maximum - Xslider.Minimum) / 2;
            Yslider.Value = (Yslider.Maximum - Yslider.Minimum) / 2;
            box1Selected = false;
            if (isRunning)
                SetIdle();
        }

        private void box2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point  pt = e.GetPosition((Rectangle)sender);
            Xslider.Value = (Xslider.Maximum - Xslider.Minimum) / 2;
            Zslider.Value = (Zslider.Maximum - Zslider.Minimum) / 2;
            box2Selected = false;
            if (isRunning)
                SetIdle();
        }
        #endregion

        #region Button Event Handlers

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSerialport();
        }

        private void setButton_Click(object sender, RoutedEventArgs e)
        {
            minThrottle = int.Parse(idleBox.Text);
            maxThrottle = int.Parse(maxBox.Text);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
                SetIdle();
        }

        private void stoputton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                SetStop();
                sendCheck.IsChecked = false;
            }                
        }

        private void UDPConnectButton_Click(object sender, RoutedEventArgs e)
        {
            InitUDP();
        }
        #endregion

        #region UDP Connection
        Thread thdUDPServer;
        string UDPDebugMsg = "";
        void InitUDP()
        {
            UDPDebugMsg = "Opening UDP Socket";
            runServer = true;
            thdUDPServer = new Thread(new ThreadStart(serverThread));
            thdUDPServer.Start();     
        }

        Vector3D forceVector = new Vector3D(0, 0, 0);
        int port = 8888;
        UdpClient udpClient;
        bool runServer = false;
        public void serverThread()
        {
            udpClient = new UdpClient(port);
            UDPDebugMsg = "UDP Socket open at " + port.ToString();
            while (runServer)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string receivedString = Encoding.ASCII.GetString(receiveBytes);
                UDPDebugMsg = receivedString;
                string[] msgTokens = receivedString.Split(':');
                if (msgTokens.Length == 2)
                {
                    string[] msgs = msgTokens[1].Split(',');
                    if (msgs.Length == 3)
                        forceVector = new Vector3D(-double.Parse(msgs[0]),
                                                    -double.Parse(msgs[1]),
                                                    - double.Parse(msgs[2]));
                }
            }
        }

        #endregion

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProcessThrottle();
        }

        private void CheckValueChanged(object sender, RoutedEventArgs e)
        {
            ProcessThrottle();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer UITimer = new DispatcherTimer();
            UITimer.Interval = TimeSpan.FromMilliseconds(16);
            UITimer.Tick += UITimer_Tick;
            UITimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            runServer = false;
            udpClient.Close();
            thdUDPServer.Abort();
        }        
    }
}
