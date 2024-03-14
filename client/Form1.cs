
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace client
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var port = 9050;

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(IPAddress.Parse("192.168.1.69"), port);
                var responseBytes = new byte[512];
                var bytes = await socket.ReceiveAsync();
                string response = Encoding.UTF8.GetString(responseBytes, 0, bytes);
                
            }
            catch (SocketException ex)
            {
            }
        }
    }
}