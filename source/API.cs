using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace CrystalisAPI
{
    public static class API
    {
        //----------------------------------------------------------

        public enum States
        {
            Attaching,
            Attached,
            NotAttached,
            NoProcessFound,
            TamperDetected,
            Error,
            Executed,
        }

        static HttpClient client = new HttpClient();
        private static string current_version_url = "https://realvelocity.xyz/assets/current_version.txt";
        private static string current_download_links_url = "https://realvelocity.xyz/assets/download_links.json";
        private static Process decompilerProcess;
        private static States Status = States.NotAttached;
        public static List<int> injected_pids = new List<int>();
        private static System.Timers.Timer CommunicationTimer;

        //-----------------------------------------------------------

        public async static void Inject(int pid)
        {
            await AutoUpdateAsync();
            StartCommunication();
            await Attach(pid);
        }

        //-----------------------------------------------------------

        private static async Task<States> Attach(int pid)
        {
            if (injected_pids.Contains(pid))
                return States.Attached;
            Status = States.Attaching;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Bin\\erto3e4rortoergn.exe";
            startInfo.Arguments = $"{pid}";
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardOutput = false;
            Process.Start(startInfo).WaitForExit();
            injected_pids.Add(pid);
            Status = States.Attached;
            return States.Attached;
        }

        public static States Execute(string script)
        {
            if (injected_pids.Count.Equals(0))
                return States.NotAttached;
            foreach (int injectedPid in injected_pids)
                NamedPipes.LuaPipe(Base64Encode(script), injectedPid);
            return States.Executed;
        }


        //-----------------------------------------------------------

        private static void StartCommunication()
        {
            if (!Directory.Exists("bin"))
                Directory.CreateDirectory("bin");
            if (!Directory.Exists("AutoExec"))
                Directory.CreateDirectory("AutoExec");
            if (!Directory.Exists("Workspace"))
                Directory.CreateDirectory("Workspace");
            if (!Directory.Exists("Scripts"))
                Directory.CreateDirectory("Scripts");
            StopCommunication();
            decompilerProcess = new Process();
            decompilerProcess.StartInfo.FileName = "bin\\Decompiler.exe";
            decompilerProcess.StartInfo.UseShellExecute = false;
            decompilerProcess.EnableRaisingEvents = true;
            decompilerProcess.StartInfo.RedirectStandardError = true;
            decompilerProcess.StartInfo.RedirectStandardInput = true;
            decompilerProcess.StartInfo.RedirectStandardOutput = true;
            decompilerProcess.StartInfo.CreateNoWindow = true;
            decompilerProcess.Start();
            CommunicationTimer = new System.Timers.Timer(100.0);
            CommunicationTimer.Elapsed += (ElapsedEventHandler)((source, e) =>
            {
                foreach (int injectedPid in injected_pids)
                {
                    if (!IsPidRunning(injectedPid))
                        injected_pids.Remove(injectedPid);
                }
                string plainText = $"setworkspacefolder: {Directory.GetCurrentDirectory()}\\Workspace";
                foreach (int injectedPid in injected_pids)
                    NamedPipes.LuaPipe(Base64Encode(plainText), injectedPid);
            });
            CommunicationTimer.Start();
        }

        private static void StopCommunication()
        {
            if (CommunicationTimer != null)
            {
                CommunicationTimer.Stop();
                CommunicationTimer = (System.Timers.Timer)null;
            }
            if (decompilerProcess != null)
            {
                decompilerProcess.Kill();
                decompilerProcess.Dispose();
                decompilerProcess = (Process)null;
            }
            injected_pids.Clear();
        }

        private static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        private static byte[] Base64Decode(string plainText) => Convert.FromBase64String(plainText);

        private static bool IsPidRunning(int pid)
        {
            try
            {
                Process.GetProcessById(pid);
                return true;
            }
            catch (ArgumentException ex)
            {
                return false;
            }
        }


        private static async Task AutoUpdateAsync()
        {
            Directory.CreateDirectory("bin");

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(current_download_links_url);
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                return;
            }

            byte[] jsonBytes = await response.Content.ReadAsByteArrayAsync();
            URLDATA.DownloadUrlData data;

            try
            {
                data = JsonSerializer.Deserialize<URLDATA.DownloadUrlData>(jsonBytes);
            }
            catch
            {
                return;
            }

            if (data == null)
                return;

            string link1, link2;
            try
            {
                link1 = Encryptor.Decrypt(data.L1, data.question);
                link2 = Encryptor.Decrypt(data.L2, data.question);
            }
            catch
            {
                return;
            }

            string remoteVersion;
            try
            {
                remoteVersion = (await client.GetStringAsync(current_version_url)).Trim();
            }
            catch
            {
                return;
            }

            string localVersionPath = Path.Combine("bin", "current_version.txt");
            string localVersion = File.Exists(localVersionPath)
                ? File.ReadAllText(localVersionPath).Trim()
                : string.Empty;

            if (remoteVersion == localVersion)
                return;

            await DownloadFileAsync(link2, Path.Combine("bin", "erto3e4rortoergn.exe"));
            await DownloadFileAsync(link1, Path.Combine("bin", "Decompiler.exe"));

            File.WriteAllText(localVersionPath, remoteVersion);
        }

        private static async Task DownloadFileAsync(string url, string path)
        {
            try
            {
                byte[] data = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(path, data);
            }
            catch
            {
                // ignore
            }
        }

        public static void print(string text, int type)
        {
            Console.ResetColor();
            switch(type)
            {

            }
        }
    }
}

