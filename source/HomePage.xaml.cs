using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;
namespace Crystalis.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {

        [DllImport(@"bin\Xeno.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Attach();
        [DllImport(@"bin\Xeno.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Initialize(bool useConsole);

        private MainWindow _mainWindow;
        private DispatcherTimer timer;
        private bool isInjected = false;

        public HomePage()
        {
            InitializeComponent();
            webview2.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            webview2.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco\\index.html"));

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1500);
            timer.Tick += TimerTick;

            _mainWindow = (MainWindow)Application.Current.MainWindow;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Process[] process = Process.GetProcessesByName("RobloxPlayerBeta");

            if (process.Length > 0)
            {
                if (isInjected)
                {
                    _mainWindow.StatusEllipse.Fill = Brushes.Green;
                }
                else
                {
                    _mainWindow.StatusEllipse.Fill = Brushes.Red;
                }
            }
            else
            {
                isInjected = false;
                _mainWindow.StatusEllipse.Fill = Brushes.Red;
            }
        }

        private async void ExecuteBtnClick(object sender, RoutedEventArgs e)
        {
            string rawText = await webview2.CoreWebView2.ExecuteScriptAsync("editor.getValue();");
            string script = JsonConvert.DeserializeObject<string>(rawText);

            int pid = 0;

            Process[] rbx = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (var p in rbx)
            {
                pid = p.Id;
            }

            string finalMassive = $"[\"{pid.ToString()}\"]";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:3110");

                string clientsJson = JsonConvert.SerializeObject(finalMassive);

                using (var request = new HttpRequestMessage(HttpMethod.Post, "o"))
                {
                    request.Headers.Add("Clients", finalMassive);
                    request.Content = new StringContent(script, Encoding.UTF8, "text/plain");
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async void ClearBtnClick(object sender, RoutedEventArgs e)
        {
            await webview2.ExecuteScriptAsync($"editor.setValue({JsonConvert.SerializeObject((object)"")})");
        }

        private async void PasteBtnClick(object sender, RoutedEventArgs e)
        {
            string text = Clipboard.GetText();
            await webview2.ExecuteScriptAsync($"editor.setValue({JsonConvert.SerializeObject(text)})");

        }
        public async void OpenFileBtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            if (ofd.ShowDialog() == true)
            {
                string fileName = ofd.FileName;
                string text = System.IO.File.ReadAllText(fileName);

                await webview2.ExecuteScriptAsync($"editor.setValue({JsonConvert.SerializeObject(text)})");
            }
        }

        private void InjectBtnClick(object sender, RoutedEventArgs e)
        {
            if (!isInjected)
            {
                try
                {
                    Initialize(true);
                    Attach();
                    isInjected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                    isInjected = false;
                }
            }
            else
            {
                MessageBox.Show("Already injected");
            }
        }
    }
}
