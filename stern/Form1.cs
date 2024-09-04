﻿using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace stern
    {
        public partial class Form1 : Form
        {
            private UdpClient udpServer;
            private IPEndPoint remoteEP;
            private TcpListener listener;
            private TcpClient client1;
            private TcpClient client2;
            private TcpClient client3;
            private NetworkStream stream1;
            private NetworkStream stream2;
            private NetworkStream stream3;
            private bool shouldCheckState = false;
            private bool trigger1 = false;
            private bool trigger2 = false;
            private readonly string allowedIp1 = "192.168.8.236";
            private readonly string allowedIp2 = "192.168.8.158";
            private readonly string allowedIp3 = "192.168.8.101";
            private bool send = false;

        public Form1()
        {
            InitializeComponent();
            udpServer = new UdpClient(11000);
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001); // 클라이언트의 로컬 IP와 포트
            listener = new TcpListener(IPAddress.Parse("10.150.150.40"), 1234);
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //맵버튼
            button6.BackColor = Color.FromArgb(0, 0, 0, 0);
            //방향키들
            button1.BackColor = Color.FromArgb(0, 0, 0, 0);
            button4.BackColor = Color.FromArgb(0, 0, 0, 0);
            button5.BackColor = Color.FromArgb(0, 0, 0, 0);
            button7.BackColor = Color.FromArgb(0, 0, 0, 0);
            button_enabled_f();
            //인트로 버튼들
            button8.Visible = false;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
            //구멍 가게 트리거들
            button2.Visible = false;
            button3.Visible = false;
            button13.Visible = false;
            button14.Visible = false;
            button15.Visible = false;
            button21.Visible = false;
            //책방 트리거들
            button16.Visible = false;
            button17.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button22.Visible = false;
            button23.Visible = false;
            //사진관 트리거들
            button24.Visible = false;
            button25.Visible = false;
            button26.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button29.Visible = false;
            //숙소
            button30.Visible = false;
            button31.Visible = false;
            button32.Visible = false;
            button33.Visible = false;
            button34.Visible = false;
            button35.Visible = false;
            button36.Visible = false;
            button37.Visible = false;
            button38.Visible = false;
            //광장
            button39.Visible = false;
            button40.Visible = false;
            button41.Visible = false;
            //엔딩
            button42.Visible = false;
            button43.Visible = false;
            button44.Visible = false;
            button45.Visible = false;
            button46.Visible = false;
            button47.Visible = false;
            button48.Visible = false;
            button49.Visible = false;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
                byte[] receivedBytes = udpServer.EndReceive(ar, ref remoteEP);
                string receivedData = Encoding.ASCII.GetString(receivedBytes);

                // 받은 데이터 확인
                if (shouldCheckState)
                {
                    if (receivedData.Trim() == "black")
                    {
                        this.BackgroundImage = stern.Properties.Resources.구멍가게_트리거;
                        button_enabled_f();
                        button6.Visible = false;
                        button2.Visible = true;
                        button3.Visible = true;
                        button13.Visible = true;
                        button14.Visible = true;
                        button21.Visible = true;
                        SendUdpMessage("stop");
                    }
                }
            }
            catch (SocketException ex)
            {
                // 예외 처리 로직 추가
                MessageBox.Show($"UDP 수신 중 오류가 발생했습니다: {ex.Message}");
            }
            finally
            {
                // 다음 데이터그램을 수신하기 위해 대기
                udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            if (client1 == null && clientIp == allowedIp1)
            {
                client1 = client;
                stream1 = client1.GetStream();
                BeginRead(stream1, 1);
            }
            else if (client2 == null && clientIp == allowedIp2)
            {
                client2 = client;
                stream2 = client2.GetStream();
                BeginRead(stream2, 2);
            }
            else if (client3 == null && clientIp == allowedIp3)
            {
                client3 = client;
                stream3 = client3.GetStream();
                BeginRead(stream3, 3);
            }
            else
            {
                // 만약 클라이언트가 원하는 IP가 아니거나 모든 클라이언트 슬롯이 찼을 경우 클라이언트 닫기
                client.Close();
            }

            // 계속해서 새로운 연결을 기다림
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);
        }

        private void BeginRead(NetworkStream stream, int clientNumber)
        {
            byte[] buffer = new byte[1024];
            try
            {
                stream.BeginRead(buffer, 0, buffer.Length, ar => ReadCallback(ar, clientNumber), buffer);
            }
            catch (IOException ex)
            {
                // 예외 처리 로직 추가
                MessageBox.Show($"클라이언트 {clientNumber} 읽기 중 오류가 발생했습니다: {ex.Message}");
                HandleClientDisconnection(clientNumber);
            }
        }

        private void ReadCallback(IAsyncResult ar, int clientNumber)
        {
            byte[] buffer = (byte[])ar.AsyncState;
            NetworkStream stream = null;

            if (clientNumber == 1) stream = stream1;
            else if (clientNumber == 2) stream = stream2;
            else if (clientNumber == 3) stream = stream3;

            if (stream == null)
            {
                // 스트림이 null인 경우 예외 처리
                return;
            }

            try
            {
                int bytesRead = stream.EndRead(ar);

                if (bytesRead > 0)
                {
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Invoke(new Action(() => UpdateState(data.Trim(), clientNumber)));
                    BeginRead(stream, clientNumber);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리: 클라이언트가 연결을 끊었을 경우
                MessageBox.Show($"클라이언트 {clientNumber}의 연결이 끊어졌습니다: {ex.Message}");

                // 해당 클라이언트 및 스트림 해제
                if (clientNumber == 1)
                {
                    client1.Close();
                    client1 = null;
                    stream1 = null;
                }
                else if (clientNumber == 2)
                {
                    client2.Close();
                    client2 = null;
                    stream2 = null;
                }
                else if (clientNumber == 3)
                {
                    client3.Close();
                    client3 = null;
                    stream3 = null;
                }

                // 새로운 클라이언트 연결 대기
                listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);
            }
        }

        private void HandleClientDisconnection(int clientNumber)
        {
            // 클라이언트 연결 해제 로직 추가
            if (clientNumber == 1)
            {
                client1.Close();
                client1 = null;
                stream1 = null;
            }
            else if (clientNumber == 2)
            {
                client2.Close();
                client2 = null;
                stream2 = null;
            }
            else if (clientNumber == 3)
            {
                client3.Close();
                client3 = null;
                stream3 = null;
            }

            // 새로운 클라이언트 연결 대기
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);
        }

        private void UpdateState(string shockState, int clientNumber)
        {
            if (shouldCheckState)
            {
                if (clientNumber == 1 && shockState == "1")
                {
                    this.BackgroundImage = stern.Properties.Resources.사진관___트리거_버전2;
                    button_enabled_f();
                    button6.Visible = false;
                    button24.Visible = true;
                    button25.Visible = true;
                    button28.Visible = true;
                    SendUdpMessage("stop");
                }
                else if (clientNumber == 1 && shockState == "2")
                {
                    this.BackgroundImage = stern.Properties.Resources.항구___범인_선택;
                    // 구멍가게 트리거 처리
                    button_enabled_f();
                    button6.Visible = false;
                    button42.Visible = true;
                    button43.Visible = true;
                    button44.Visible = true;
                    SendUdpMessage("stop");
                }
                else if (clientNumber == 2 && shockState == "1")
                {
                    this.BackgroundImage = stern.Properties.Resources.책방___트리거_버전2;
                    // 책방 트리거 처리
                    button_enabled_f();
                    button6.Visible = false;
                    button16.Visible = true;
                    button17.Visible = true;
                    button18.Visible = true;
                    button19.Visible = true;
                    button22.Visible = true;
                    SendUdpMessage("stop");
                }
                else if (clientNumber == 2 && shockState == "2")
                {
                    this.BackgroundImage = stern.Properties.Resources.엄덕춘과의_대화;
                    button_enabled_f();
                    button6.Visible = false;
                    button30.Visible = true;
                    SendUdpMessage("stop");
                }
                else if (clientNumber == 3 && shockState == "1")
                {
                    this.BackgroundImage = stern.Properties.Resources.광장___트리거_버전2;
                    button_enabled_f();
                    button6.Visible = false;
                    button39.Visible = true;
                    button40.Visible = true;
                    SendUdpMessage("stop");
                }
            }
        }

        private void button_enabled_f()
        {
            button1.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button7.Visible = false;
            shouldCheckState = false;
        }

        private void button_enabled_t()
        {
            button1.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;
            button7.Visible = true;
            shouldCheckState = true;    
        }

        private void button_g()
            {
                button2.Visible = false;
                button3.Visible = false;
                button13.Visible = false;
                button14.Visible = false;
                button15.Visible = true;
                button21.Visible = false;
            }
            private void button_b()
            {
                button16.Visible = false;
                button17.Visible = false;
                button18.Visible = false;
                button19.Visible = false;
                button20.Visible = true;
                button22.Visible = false;
            }
            private void button_p()
            {
                button24.Visible = false;
                button25.Visible = false;
                button26.Visible = false;
                button27.Visible = true;
                button28.Visible = false;
            }
            private void button_s()
            {
                button38.Visible = false;
                button31.Visible = false;
                button32.Visible = false;
                button33.Visible = false;
                button34.Visible = false;
                button35.Visible = false;
                button36.Visible = true;
                button37.Visible = false; 
            }
            private void button_h()
            {
                button42.Visible = false;
                button43.Visible = false;
                button44.Visible = false;
            }
            private void button_h2()
            {
                button45.Visible = false;
                button46.Visible = false;
                button47.Visible = false;
            }
            private void button6_Click(object sender, EventArgs e)
            {
                button_enabled_f();
                button6.Visible = false;
                button8.Visible = true;
                shouldCheckState = false;
                this.BackgroundImage = stern.Properties.Resources.MAP;
            }


            private void button8_Click(object sender, EventArgs e)
            {
                button8.Visible = false;
                button6.Visible = true;
                button_enabled_t();
                this.BackgroundImage = stern.Properties.Resources.MAIN;
            }

            private void button9_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사건파일2;
                button9.Visible = false;
                button10.Visible = true;
            }

            private void button10_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사건파일2_2;
                button10.Visible = false;
                button11.Visible = true;
            }

            private void button11_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사건파일2_3;
                button11.Visible = false;
                button12.Visible = true;
            }

            private void button12_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button12.Visible = false;
                button_enabled_t();
            }

            private void SendUdpMessage(string message)
            {
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                udpServer.Send(sendBytes, sendBytes.Length, remoteEP);
            }

            private void button1_MouseDown(object sender, MouseEventArgs e)
            {
                button1.Image = stern.Properties.Resources.r_button_t;
                send = true;
                SendUdpMessage("go");
        }

            private void button1_MouseUp(object sender, MouseEventArgs e)
            {
                button1.Image = stern.Properties.Resources.button_t;
                SendUdpMessage("stop");
                send = false;
            }

            private void button4_MouseDown(object sender, MouseEventArgs e)
            {
                button4.Image = stern.Properties.Resources.r_button_b;
                SendUdpMessage("back");
            }

            private void button4_MouseUp(object sender, MouseEventArgs e)
            {
                button4.Image = stern.Properties.Resources.button_b;
                SendUdpMessage("stop");
            }

            private void button5_MouseDown(object sender, MouseEventArgs e)
            {
                button5.Image = stern.Properties.Resources.r_lt_button_t;
                SendUdpMessage("left");
            }

            private void button5_MouseUp(object sender, MouseEventArgs e)
            {
                button5.Image = stern.Properties.Resources.lt_button_t;
                SendUdpMessage("stop");
            }

            private void button7_MouseDown(object sender, MouseEventArgs e)
            {
                button7.Image = stern.Properties.Resources.r_rt_button_t;
                SendUdpMessage("right");
            }

            private void button7_MouseUp(object sender, MouseEventArgs e)
            {
                button7.Image = stern.Properties.Resources.rt_button_t;
                SendUdpMessage("stop");
            }
            private void button1_MouseHover(object sender, EventArgs e)
            {
                button1.BackColor = Color.FromArgb(0, 0, 0, 0);
            }

            private void button4_MouseHover(object sender, EventArgs e)
            {
                button4.BackColor = Color.FromArgb(0, 0, 0, 0);
            }

            private void button2_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.구멍가게_뜨개_인형;
                button_g();
            }

            private void button3_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.구멍가게_시체;
                button_g();
            }

            private void button13_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.구멍가게_외부진열대;
                button_g();
            }
            private void button14_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.구멍가게_문에_붙어있는_A4용지;
                button_g();
            }

            private void button15_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.구멍가게_트리거;
                button15.Visible = false;
                button2.Visible = true;
                button3.Visible = true;
                button13.Visible = true;
                button14.Visible = true;
                button21.Visible = true;
            }

            private void button20_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.책방___트리거_버전2;
                button20.Visible = false;
                button16.Visible = true;
                button17.Visible = true;
                button18.Visible = true;
                button19.Visible = true;
                button22.Visible = true;
            }

            private void button21_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button_enabled_t();
                button2.Visible = false;
                button3.Visible = false;
                button14.Visible = false;
                button13.Visible = false;
                button21.Visible=false;
            }

            private void button22_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button_enabled_t();
                button16.Visible = false;
                button17.Visible = false;
                button18.Visible = false;
                button19.Visible = false;
                button22.Visible=false;
            }

            private void button16_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.책방___잡지;
                button_b();
            }
            private void button17_Click(object sender, EventArgs e)
            {
                if (trigger1)
                {
                    this.BackgroundImage = stern.Properties.Resources.책방___쪽지와_구멍가게_공지의_필체_감별_요청;
                    button_b();
                }
                else
                {
                    this.BackgroundImage = stern.Properties.Resources.물건을_가져오지_않았을_시___필체;
                    button_b();
                }
            }

            private void button18_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.책방___쇠바늘;
                button_b();
            }

            private void button19_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.최석동과의_대화;
                button_b();
                button22.Visible = false;
                button23.Visible = true;
            }

            private void button23_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.최석동과의_대화2;
                button23.Visible = false;
                button20.Visible=true;
            }

            private void button24_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.한혜지와의_대화;
                button_p();
                button29.Visible = true;
            }
            private void button25_Click(object sender, EventArgs e)
            {
                if(trigger2)
                {
                    this.BackgroundImage = stern.Properties.Resources.사진관___카메라_필름_인화_요청;
                    button_p();
                    button27.Visible = false;
                    button26.Visible = true;
                }
                else
                {
                    this.BackgroundImage = stern.Properties.Resources.물건을_가져오지_않았을_시___인화;
                    button_p();
                }
            }

            private void button26_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사진관___카메라_필름_인화_요청2;
                button_p();
            }
            private void button27_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사진관___트리거_버전2;
                button27.Visible = false;
                button24.Visible = true;
                button25.Visible = true;
                button28.Visible = true;
            }

            private void button28_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button_enabled_t();
                button_p();
                button27.Visible = false;

            }

            private void button29_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.한혜지와의_대화2;
                button29.Visible = false;
                button_p();
            }

            private void button30_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엄덕춘과의_대화2;
                button30.Visible = false;
                button_s();
            }

            private void button31_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.숙소___탁자;
                button_s();
                trigger1 = true;
            }

            private void button32_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.숙소___공용_음식물_쓰레기통;
                trigger2 = true;
                button_s();
            }

            private void button33_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.숙소___엄덕춘의_가방;
                button_s();
            }

            private void button34_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.숙소___벽;
                button_s();
            }

            private void button35_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엄덕춘과의_대화3;
                button_s();
            }

            private void button36_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.숙소___트리거_버전2;
                button36.Visible = false;
                button31.Visible = true;
                button32.Visible = true;
                button33.Visible = true;
                button34.Visible = true;
                button35.Visible = true;
                button37.Visible = true;
                button38.Visible = true;
            }

            private void button37_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button_s();
                button36.Visible = false;
                button_enabled_t();
            }

            private void button38_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엄덕춘과의_대화;
                button_s();
                button36.Visible=false;
                button30.Visible = true;
            }

            private void button39_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.MAIN;
                button_enabled_t();
                button39.Visible=false;
            }

            private void button40_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.광장___투표함__1_;
                button41.Visible = true;
                button40.Visible = false;
                button39.Visible = false;
            }

            private void button41_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.광장___트리거_버전2;
                button41.Visible = false;
                button40.Visible = true;
                button39.Visible = true;
            }
            private void button42_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩2;
                button_h();
                button49.Visible = true;
            }

            private void button43_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩2;
                button_h();
                button49.Visible = true;
            }

            private void button44_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.항구___선박표_구매;
                button_h();
                button45.Visible = true;
                button46.Visible = true;
                button47.Visible = true;
            }

            private void button45_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩1;
                button_h2();
                button48.Visible = true;
            }

            private void button46_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩2;
                button_h2();
                button49.Visible = true;
            }

            private void button47_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩2;
                button_h2();
                button49.Visible = true;
            }

            private void button48_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩1__1_;
                button48.Visible = false;
            }

            private void button49_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.엔딩3;
                button49.Visible = false;
            }

            private void button50_Click(object sender, EventArgs e)
            {
                this.BackgroundImage = stern.Properties.Resources.사건파일;
                button9.Visible = true;
                button50.Visible = false;
            }
        }
    }
