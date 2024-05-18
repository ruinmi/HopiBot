using System;
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
                        var xp = ClientApi.GetMyXp();
                        // var puuid = "dfbf20ad-cf8c-52e2-9ec1-f225423321fe";
                        // var puuid = "2fda0cc5-1374-547d-a67d-e8da9379a324";
                        // var name = ClientApi.GetSummoner(puuid).DisplayName;
                        // var matches = ClientApi.GetMatchesByPuuid(puuid);
                        // foreach (var match in matches)
                        // {
                        //     if (match.GameMode != "CLASSIC" || match.GameType != "MATCHED_GAME") continue;
                        //     DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(match.GameCreation).UtcDateTime.ToLocalTime();
                        //     var m = ClientApi.GetMatch(match.GameId);
                        //     var participantId = 0;
                        //     foreach (var mParticipantIdentity in m.ParticipantIdentities)
                        //     {
                        //         if (puuid == mParticipantIdentity.Player.Puuid)
                        //         {
                        //             participantId = mParticipantIdentity.ParticipantId;
                        //         }
                        //     }
                        //     foreach (var participant in m.Participants)
                        //     {
                        //         if (participantId == participant.ParticipantId)
                        //         {
                        //             Logger.Log($"{name} {dateTime}: {participant.CalculateScore(match.GameDuration).ToString()}");
                        //         }
                        //     }
                        // }
                    });
                }
            };
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

        public void UpdatePlayerInfo(bool isUnderAttack, bool isLowHealth, string action, bool isDead, string position)
        {
            Dispatcher.Invoke(() =>
            {
                LbUnderAttack.Content = isUnderAttack;
                LbLowHealth.Content = isLowHealth;
                LbAction.Content = action;
                LbDead.Content = isDead;
                LbChampPosition.Content = position;
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
