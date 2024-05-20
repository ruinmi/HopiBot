using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private Timer _updateTimer;
        private Thread _botThread;
        private Client _client;

        private int _xpOnStart = 0;
        private int _xp = 0;

        public MainWindow()
        {
            InitializeComponent();
            if (!ClientApi.CheckConnection())
            {
                MessageBox.Show("请先打开游戏客户端");
                Application.Current.Shutdown();
                return;
            }
            Init();
            UpdateInfo();
        }

        private void Init()
        {
            var champs = ClientApi.GetAllChampions();
            ChampCb.ItemsSource = champs;
            ChampCb.SelectedItem = champs.Find(c => c.Name == "麦林炮手");

            _xpOnStart = ClientApi.GetMyXp();
            _xp = _xpOnStart;

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
                        ScoreService.CalculateAvgScore(ClientApi.GetMyPuuid());
                    });
                }
            };
        }

        private void GetTeamInfo(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var scores = ScoreService.GetScores();
                Dispatcher.Invoke(() =>
                {
                    var allyInfo = scores["ally"].Select(s => $"{s.Item1}: {s.Item2}").ToList();
                    var enemyInfo = scores["enemy"].Select(s => $"{s.Item1}: {s.Item2}").ToList();
                    if (allyInfo.Count == 0 || enemyInfo.Count == 0)
                    {
                        MessageBox.Show("无法获取队伍信息");
                        return;
                    }
                    // 保留一位小数
                    TbAllyInfo.Text = string.Join("\n\n", allyInfo);
                    TbEnemyInfo.Text = string.Join("\n\n", enemyInfo);
                });
            });
        }

        private void UpdateInfo()
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += (sender, e) =>
            {
                var phase = ClientApi.GetGamePhase();
                if (phase == GamePhase.ChampSelect || phase == GamePhase.EndOfGame || phase == GamePhase.PreEndOfGame)
                {
                    var currXp = ClientApi.GetMyXp();
                    if (currXp == _xp) return;
                    Dispatcher.Invoke(() =>
                    {
                        LbXpEarnedLastRound.Content = (currXp - _xp).ToString();
                        LbXpEarnedTotal.Content = (currXp - _xpOnStart).ToString();
                        _xp = currXp;
                    });
                }
                Dispatcher.Invoke(() => GameStatusBlk.Text = phase.ToChinese());
            };
            _updateTimer.Start();
        }

        public void UpdatePlayerInfo(bool isUnderAttack, bool isLowHealth, string action, bool isDead, string position, string lastHealth)
        {
            Dispatcher.Invoke(() =>
            {
                LbUnderAttack.Content = isUnderAttack;
                LbLowHealth.Content = isLowHealth;
                LbAction.Content = action;
                LbDead.Content = isDead;
                LbChampPosition.Content = position;
                LbLastHealth.Content = lastHealth;
            });
        }

        private void StartupBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_botThread == null)
            {
                if (ChampCb.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择英雄");
                    return;
                }

                ChampCb.IsEnabled = false;
                StartupBtn.Content = "停止";
                _botThread = new Thread(() =>
                {
                    _client = new Client(this);
                    Dispatcher.Invoke(() => { _client.Champ = (Champion) ChampCb.SelectedItem; });
                    _client.Start();
                });
                _botThread.Start();
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            ChampCb.IsEnabled = true;
            StartupBtn.Content = "启动";
            _client?.Shutdown();
            _botThread?.Abort();
            _botThread?.Join();
            _botThread = null;
        }

        #region General UI Event


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

        #endregion
    }
}
