using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using HopiBot.Game;
using HopiBot.LCU;
using HopiBot.LCU.bo;
using HopiBot.Enum;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace HopiBot
{
    public partial class MainWindow
    {
        private Timer _timer;
        private Thread _botThread;
        private Client _client;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            UpdateStatus();
        }

        private void Init()
        {
            var champs = ClientApi.GetAllChampions();
            ChampCb.ItemsSource = champs;
            ChampCb.SelectedItem = champs.Find(c => c.Name == "麦林炮手");

            var hook = Hook.GlobalEvents();
            hook.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.F1)
                {
                    Stop();
                }

                if (e.KeyCode == Keys.U)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Controller.RightClick(new RatioPoint(double.Parse(Xtb.Text), double.Parse(Ytb.Text)));
                        ClientApi.HonorPlayer();
                    });
                }
            };


        }

        private void UpdateStatus()
        {
            _timer = new Timer(1000); // 每5秒获取一次数据
            _timer.Elapsed += (sender, e) =>
            {
                var phase = ClientApi.GetGamePhase();
                Application.Current.Dispatcher.Invoke(() => GameStatusBlk.Text = phase.ToChinese());
            };
            _timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Close();
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void StartupBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_botThread == null)
            {
                if (TotalRoundTb.Text == "")
                {
                    MessageBox.Show("请输入总局数");
                    return;
                }

                if (ChampCb.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择英雄");
                    return;
                }

                TotalRoundTb.IsReadOnly = true;
                ChampCb.IsEnabled = false;
                StartupBtn.Content = "停止";
                _botThread = new Thread(() =>
                {
                    _client = new Client(this);
                    Application.Current.Dispatcher.Invoke(() => { _client.Champ = (Champion) ChampCb.SelectedItem; });
                    _client.Start();
                });
                _botThread.Start();
            }
            else
            {
                Stop();
            }
        }

        public void Stop()
        {
            TotalRoundTb.IsReadOnly = false;
            ChampCb.IsEnabled = true;
            StartupBtn.Content = "启动";
            _client?.Abort();
            _botThread?.Abort();
            _botThread?.Join();
            _botThread = null;
        }

    }
}
