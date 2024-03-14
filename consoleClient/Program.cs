// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;

class Program
{
    public class RF
    {
        public void ReceiveFile()
        {
            System.Threading.Thread WorkerThread = new System.Threading.Thread(() =>
            {
                System.Net.Sockets.TcpListener TcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 85);
                TcpListener.Start();
                System.Net.Sockets.Socket HandlerSocket = TcpListener.AcceptSocket();
                System.Net.Sockets.NetworkStream NetworkStream = new System.Net.Sockets.NetworkStream(HandlerSocket);
                int BlockSize = 1024;
                int DataRead = 0;
                Byte[] DataByte = new Byte[BlockSize];
                lock (this)
                {
                    System.IO.Stream FileStream = System.IO.File.OpenWrite("done.txt");

                    while (true)
                    {
                        DataRead = NetworkStream.Read(DataByte, 0, BlockSize);
                        FileStream.Write(DataByte, 0, DataRead);
                        if (DataRead == 0)
                        {
                            break;
                        }
                    }
                    FileStream.Close();
                }
            });
            WorkerThread.Start();
        }

    }
    static void Main()
    {
        RF rf = new RF();
        rf.ReceiveFile();
    }
}