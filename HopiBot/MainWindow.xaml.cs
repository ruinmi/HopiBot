using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HopiBot.LCU;

namespace HopiBot
{
    public partial class MainWindow
    {
        public Timer AcceptTimer;
        public Timer ChampSelectTimer;
        public Timer GameTimer;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            AcceptTimer = new Timer(AutoAcceptMatch, null, Timeout.Infinite, Timeout.Infinite);
            ChampSelectTimer = new Timer(AutoChampSelect, null, Timeout.Infinite, Timeout.Infinite);
            GameTimer = new Timer(AutoGame, null, 0, 1000);
        }

        private async void AutoAcceptMatch(object state)
        {
            if (await ClientApi.GetGameStatus() == "ReadyCheck")
            {
                await ClientApi.Accept();
                AcceptTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void AutoChampSelect(object state)
        {
            if (await ClientApi.GetGameStatus() == "ChampSelect")
            {
                await ClientApi.ChampSelect(22);
                ChampSelectTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void AutoGame(object state)
        {
            if (await ClientApi.GetGameStatus() == "WaitingForStats")
            {
                CurrRoundBlk.Text = (int.Parse(CurrRoundBlk.Text) + 1).ToString();
                await ClientApi.PlayAgain();
                MessageBox.Show("当前局数: " + CurrRoundBlk.Text + " / " + TotalRoundTb.Text);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
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
            StartupBtn.Click -= StartupBtn_OnClick;
            StartupBtn.Click += StopBtn_OnClick;
            _ = Task.Run(async () =>
            {
                if (await ClientApi.CreateBotLobby() && await ClientApi.SearchMath())
                {
                    // AcceptTimer.Change(0, 1000);
                    // ChampSelectTimer.Change(0, 1000);
                }
            });
        }

        private void StopBtn_OnClick(object sender, RoutedEventArgs e)
        {
            TotalRoundTb.IsReadOnly = false;
            ChampCb.IsEnabled = true;
            StartupBtn.Content = "启动";
            StartupBtn.Click -= StopBtn_OnClick;
            StartupBtn.Click += StartupBtn_OnClick;
            AcceptTimer.Change(Timeout.Infinite, 0);
            ChampSelectTimer.Change(Timeout.Infinite, 0);
        }
    }
}
