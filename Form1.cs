using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;

namespace stern
{
    public partial class Form1 : Form
    {
        private UdpClient udpServer;
        private IPEndPoint remoteEP;
        private HttpClient client;
        private string esp32Url = "http://10.150.149.101/";
        private string lastShockState = "";
        private bool shouldCheckState = false;

        public Form1()
        {
            InitializeComponent();
            udpServer = new UdpClient(11000);
            remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001); // 클라이언트의 로컬 IP와 포트
            client = new HttpClient();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button6.BackColor = Color.FromArgb(0, 0, 0, 0);
            button_enabled_f();
            button8.Visible = false;
            button10.Visible = false;
            button11.Visible = false;
            button12.Visible = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button_enabled_f();
            button6.Visible = false;
            button8.Visible = true;
            this.BackgroundImage = stern.Properties.Resources.MAP;
        }

        private void button_enabled_f()
        {
            button1.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button7.Visible = false;

            button1.BackColor = Color.FromArgb(0, 0, 0, 0);
            button4.BackColor = Color.FromArgb(0, 0, 0, 0);
            button5.BackColor = Color.FromArgb(0, 0, 0, 0);
            button7.BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        private void button_enabled_t()
        {
            button1.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button7.Visible = true;

            button1.BackColor = Color.Gray;
            button4.BackColor = Color.Gray;
            button5.BackColor = Color.Gray;
            button7.BackColor = Color.Gray;
        }
        private async void CheckState()
        {
            while (shouldCheckState)
            {
                try
                {
                    string response = await client.GetStringAsync(esp32Url);
                    response = response.Trim();

                    // 상태를 콤마로 구분해 읽기
                    if (response.Length < 2)
                    {
                        Console.WriteLine("Invalid response from ESP32");
                        continue;
                    }

                    string shockState = response;

                    if (shockState != lastShockState)
                    {
                        lastShockState = shockState;
                        UpdateState(shockState);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                await Task.Delay(500); // 500ms마다 체크
            }
        }


        private void UpdateState(string shockState)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateState), shockState);
            }
            else
            {
                if (shockState == "LOW")
                {
                    this.BackgroundImage = stern.Properties.Resources.구멍가게_문에_붙어있는_A4용지;
                    button_enabled_f();
                    button6.Visible = false;
                    button8.Visible = true;
                }
            }
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
            button6.Visible = true;
            button_enabled_t();
            //클릭된 후에 상태를 체크하도록 설정
            shouldCheckState = true;
            CheckState();
        }

        private void SendUdpMessage(string message)
        {
            byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            udpServer.Send(sendBytes, sendBytes.Length, remoteEP);
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            SendUdpMessage("go");
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            SendUdpMessage("stop");
        }

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            SendUdpMessage("back");
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            SendUdpMessage("stop");
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            SendUdpMessage("left");
        }

        private void button5_MouseUp(object sender, MouseEventArgs e)
        {
            SendUdpMessage("stop");
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            SendUdpMessage("right");
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {
            SendUdpMessage("stop");
        }
    }
}
