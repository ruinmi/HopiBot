using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;
using HopiBot.Enum;
using HopiBot.LCU;
using InputManager;
using Application = System.Windows.Application;

namespace HopiBot.Game
{
    public class Game
    {
        private bool _isLocked;

        public static RatioPoint MINI_MAP_UNDER_TURRET_MID = new RatioPoint(0.915, 0.89);
        public static RatioPoint MINI_MAP_CENTER_MID = new RatioPoint(0.928, 0.875);
        public static RatioPoint MINI_MAP_UNDER_OUTER_TURRET_MID = new RatioPoint(0.94, 0.85);
        public static RatioPoint MINI_MAP_UNDER_INNDER_TURRET_MID = new RatioPoint(0.95, 0.84);
        public static RatioPoint MINI_MAP_UNDER_INHIBITOR_TURRET_MID = new RatioPoint(0.96, 0.82);
        public static RatioPoint MINI_MAP_ENEMY_NEXUS = new RatioPoint(0.9787, 0.78);

        public static RatioPoint ULT_DIRECTION = new RatioPoint(0.7298, 0.2689);
        public static RatioPoint CENTER_OF_SCREEN = new RatioPoint(0.5, 0.5);

        public static RatioPoint AFK_OK_BUTTON = new RatioPoint(0.4981, 0.4647);

        public static List<RatioPoint> SHOP_ITEM_BUTTONS = new List<RatioPoint>
            { new RatioPoint(0.26, 0.4), new RatioPoint(0.37, 0.4), new RatioPoint(0.47, 0.4) };

        public const string MidOuterTurret = "Turret_T2_C_05_A";
        public const string MidInnerTurret = "Turret_T2_C_04_A";
        public const string MidInhibitorTurret = "Turret_T2_C_03_A";
        public const string NexusTurret1 = "Turret_T2_C_01_A";
        public const string NexusTurret2 = "Turret_T2_C_02_A";


        private Thread _playThread;
        private Thread _phaseListeningThread;
        private Random _random = new Random();

        public void Start()
        {
            while (true)
            {
                Thread.Sleep(2000);
                var gameTime = GameApi.GetGameTime();
                if (gameTime < 20) continue;
                if (!_isLocked)
                {
                    Controller.LockScreen();
                    _isLocked = true;
                }

                break;
            }

            while (true)
            {
                var phase = ClientApi.GetGamePhase();
                if (phase != GamePhase.InProgress)
                {
                    Logger.Log("Game End");
                    AbortPlay();
                    break;
                }

                if (GameApi.IsDead())
                {
                    if (_playThread == null) continue;
                    Logger.Log("Dead");
                    AbortPlay();
                    _playThread = null;
                }
                else
                {
                    if (_playThread != null) continue;
                    Logger.Log("Alive");
                    _playThread = new Thread(Play);
                    Thread.Sleep(1000);
                    _playThread.Start();
                }

                Thread.Sleep(2000);
            }
        }

        private void Play()
        {
            var finalPoint = GetFinalPoint();
            UpgradeAbilities();
            BuyItems();
            Controller.RightClick(MINI_MAP_UNDER_TURRET_MID);
            Thread.Sleep(25 * 1000);
            while (true)
            {
                Controller.RightClick(finalPoint);
                Thread.Sleep(10 * 1000);
                for (int i = 0; i < 40; i++)
                {
                    Keyboard.KeyPress(Keys.A, 100);
                    Controller.LeftClick(CENTER_OF_SCREEN.Add(20, -150));
                    Thread.Sleep(500);
                    Keyboard.KeyPress(Keys.Q, 100);
                }
                Controller.RightClick(MINI_MAP_UNDER_TURRET_MID);
                Thread.Sleep(10 * 1000);
                finalPoint = GetFinalPoint();
            }
        }

        private RatioPoint GetFinalPoint()
        {
            var data = GameApi.GetGameEvents();
            if (data == null) return MINI_MAP_CENTER_MID;
            var events = data.ToList();
            events.Sort((a, b) => b["EventID"].ToObject<int>() - a["EventID"].ToObject<int>());
            foreach (var e in events)
            {
                if (e["EventName"]?.ToString() == "TurretKilled")
                {
                    var turret = e["TurretKilled"].ToString();
                    switch (turret)
                    {
                        case NexusTurret1:
                        case NexusTurret2:
                            return MINI_MAP_ENEMY_NEXUS;
                        case MidInhibitorTurret:
                            return MINI_MAP_UNDER_INHIBITOR_TURRET_MID;
                        case MidOuterTurret:
                            return MINI_MAP_UNDER_OUTER_TURRET_MID;
                        case MidInnerTurret:
                            return MINI_MAP_UNDER_INNDER_TURRET_MID;
                    }
                }
            }

            return MINI_MAP_CENTER_MID;
        }

        private bool IsNexusKilled()
        {
            var events = GameApi.GetGameEvents();
            foreach (var e in events)
            {
                if (e["EventName"]?.ToString() == "TurretKilled")
                {
                    var turret = e["TurretKilled"].ToString();
                    if (turret == NexusTurret1 || turret == NexusTurret2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void BuyItems()
        {
            Keyboard.KeyPress(Keys.P, 100);
            Thread.Sleep(1000);
            foreach (var point in SHOP_ITEM_BUTTONS)
            {
                Controller.RightClick(point);
                Thread.Sleep(200);
                Controller.RightClick(point);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            Keyboard.KeyPress(Keys.P, 100);
        }

        private void UpgradeAbilities()
        {
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.R, 100);
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.Q, 100);
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.W, 100);
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.E, 100);
            Thread.Sleep(100);
            Keyboard.KeyUp(Keys.LControlKey);
        }

        public void AbortPlay()
        {
            _playThread?.Abort();
            _playThread?.Join();
        }
    }
}
