using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrystalisAPI
{
    internal class NamedPipes
    {
        public static string luapipename = "uoQcySKXSUxxJNpVQyatpHQwYoGfhcbh";

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WaitNamedPipe(string name, int timeout);

        public static bool NamedPipeExist(string pipeName)
        {
            try
            {
                if (!NamedPipes.WaitNamedPipe("\\\\.\\pipe\\" + pipeName, 0))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 0:
                            return false;
                        case 2:
                            return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void LuaPipe(string script, int pid)
        {
            if (!NamedPipes.NamedPipeExist($"{NamedPipes.luapipename}_{pid}"))
                return;
            new Thread((ThreadStart)(() =>
            {
                try
                {
                    using (NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(".", $"{NamedPipes.luapipename}_{pid}", PipeDirection.Out))
                    {
                        pipeClientStream.Connect();
                        using (StreamWriter streamWriter = new StreamWriter((Stream)pipeClientStream, Encoding.Default, 999999))
                        {
                            ((TextWriter)streamWriter).Write(script);
                            ((TextWriter)streamWriter).Dispose();
                        }
                        pipeClientStream.Dispose();
                    }
                }
                catch (IOException ex)
                {
                }
                catch (Exception ex)
                {
                }
            })).Start();
        }
    }
}
