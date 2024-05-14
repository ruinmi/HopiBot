using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HopiBot.Game;
using HopiBot.LCU;
using HopiBot.LCU.bo;
using Newtonsoft.Json;
using Keyboard = HopiBot.Game.Keyboard;

namespace HopiBot
{
    public partial class MainWindow
    {
        public Timer SearchTimer;
        public Timer ChampSelectTimer;
        public Timer AcceptTimer;
        public Timer PlayAgainTimer;
        public Timer GameStatusTimer;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private async void Init()
        {
            SearchTimer = new Timer(AutoSearch, null, Timeout.Infinite, Timeout.Infinite);
            ChampSelectTimer = new Timer(AutoChampSelect, null, Timeout.Infinite, Timeout.Infinite);
            AcceptTimer = new Timer(AutoAccept, null, Timeout.Infinite, Timeout.Infinite);
            PlayAgainTimer = new Timer(AutoPlayAgain, null, Timeout.Infinite, Timeout.Infinite);
            GameStatusTimer = new Timer(GameStatusListening, null, 0, 1000);

            var champs = await ClientApi.GetAllChampions();
            ChampCb.ItemsSource = champs;
            ChampCb.SelectedItem = champs.Find(c => c.Name == "寒冰射手");
        }

        private async void AutoSearch(object state)
        {
            if (await ClientApi.GetGameStatus() == "Lobby")
            {
                await ClientApi.SearchMath();
                SearchTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void AutoAccept(object state)
        {
            if (await ClientApi.GetGameStatus() == "ReadyCheck")
            {
                await ClientApi.Accept();
                AcceptTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void AutoChampSelect(object state)
        {
            Champion champ = null;
            if (await ClientApi.GetGameStatus() == "ChampSelect")
            {
                // 使用Dispatcher来访问UI元素
                await Application.Current.Dispatcher.InvokeAsync(() => { champ = (Champion)ChampCb.SelectedItem; });

                if (champ != null)
                {
                    var suc = await ClientApi.ChampSelect(champ.Id);
                    if (!suc)
                    {
                        // 同样使用Dispatcher来显示消息框
                        Application.Current.Dispatcher.InvokeAsync(() => { MessageBox.Show("选英雄失败"); });
                    }
                }
                else
                {
                    // 同样使用Dispatcher来显示消息框
                    Application.Current.Dispatcher.InvokeAsync(() => { MessageBox.Show("未选中任何英雄"); });
                }
                ChampSelectTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void AutoPlayAgain(object state)
        {
            var status = await ClientApi.GetGameStatus();
            if (status == "EndOfGame")
            {
                Dispatcher.Invoke(() =>
                {
                    CurrRoundBlk.Text = (int.Parse(CurrRoundBlk.Text) + 1).ToString();
                });
                if (!await ClientApi.PlayAgain())
                {
                    MessageBox.Show("游戏结束, 重开失败！");
                }
                PlayAgainTimer.Change(Timeout.Infinite, 0);
            }
        }

        private async void GameStatusListening(object state)
        {
            var status = await ClientApi.GetGameStatus();
            Dispatcher.Invoke(() =>
            {
                GameStatusBlk.Text = status;
            });
            switch (status)
            {
                case "None":
                    break;
                case "Lobby":
                    SearchTimer.Change(0, 1000);
                    break;
                case "Matchmaking":
                    AcceptTimer.Change(0, 1000);
                    break;
                case "ReadyCheck":
                    ChampSelectTimer.Change(0, 1000);
                    break;
                case "ChampSelect":
                    break;
                case "InProgress":
                    if (!GameService.Instance.IsRunning)
                    {
                        _ = Task.Run(async () =>
                        {
                            var ent = await GameApi.GetGameEvent();
                            if (ent != null && ent.Contains("GameStart"))
                            {
                                GameService.Instance.Start();
                            }
                        });
                    }

                    break;
                case "WaitingForStats":
                    PlayAgainTimer.Change(0, 1000);
                    _ = Task.Run(() =>
                    {
                        GameService.Instance.Stop();
                    });
                    break;
                case "EndOfGame":
                    break;
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
                await ClientApi.CreateBotLobby();
            });
        }

        private void StopBtn_OnClick(object sender, RoutedEventArgs e)
        {
            TotalRoundTb.IsReadOnly = false;
            ChampCb.IsEnabled = true;
            StartupBtn.Content = "创建房间";
            StartupBtn.Click -= StopBtn_OnClick;
            StartupBtn.Click += StartupBtn_OnClick;
        }
    }
}
