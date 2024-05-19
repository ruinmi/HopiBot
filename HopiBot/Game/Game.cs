using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using HopiBot.Enum;
using HopiBot.LCU;
using InputManager;

namespace HopiBot.Game
{
    public class Game
    {
        private MainWindow _mainWindow;

        #region Game Thread

        private Thread _playThread;
        private CancellationTokenSource _ctsPlay;
        private CancellationTokenSource _cts;
        private Thread _healthThread;

        #endregion

        #region Player State

        private bool _isDead;
        private bool IsDead
        {
            get => _isDead;
            set
            {
                if (_isDead == value) return;
                _isDead = value;
                UpdatePlayerInfo();
            }
        }

        private string _action;
        private string Action
        {
            get => _action;
            set
            {
                if (_action == value) return;
                _action = value;
                UpdatePlayerInfo();
            }
        }

        private bool _isUnderAttack;
        private bool IsUnderAttack
        {
            get => _isUnderAttack;
            set
            {
                if (_isUnderAttack == value) return;
                _isUnderAttack = value;
                UpdatePlayerInfo();
            }
        }

        private bool _isLowHealth;
        private bool IsLowHealth
        {
            get => _isLowHealth;
            set
            {
                if (_isLowHealth == value) return;
                _isLowHealth = value;
                UpdatePlayerInfo();
            }
        }

        private int _lastHealth;
        private int LastHealth
        {
            get => _lastHealth;
            set
            {
                if (_lastHealth == value) return;
                _lastHealth = value;
                UpdatePlayerInfo();
            }
        }

        private Position _position;
        private Position Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                UpdatePlayerInfo();
            }
        }

        #endregion

        public Game(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Position = Position.InBase;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _healthThread = new Thread(HealthCheck);
            _healthThread.Start();
            // 游戏20s后开始
            var isLocked = false;
            while (!isLocked)
            {
                Thread.Sleep(2000);
                var gameTime = GameApi.GetGameTime();
                if (gameTime < 20) continue;
                Controller.LockScreen();
                isLocked = true;
            }

            // 游戏开始，监听游戏是否结束、是否死亡
            while (true)
            {
                Thread.Sleep(2000);
                var phase = ClientApi.GetGamePhase();
                if (phase != GamePhase.InProgress)
                {
                    Logger.Log("Game End");
                    StopGame();
                    break;
                }

                _isDead = _lastHealth <= 0;
                if (_isDead)
                {
                    if (_playThread == null) continue;
                    StopPlay();
                }
                else
                {
                    if (_playThread != null) continue;
                    _ctsPlay = new CancellationTokenSource();
                    _playThread = new Thread(() => Play(_ctsPlay.Token));
                    _playThread.Start();
                }

                if (_cts.IsCancellationRequested) break;
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="token"></param>
        private void Play(CancellationToken token)
        {
            try
            {
                Start:
                while (true)
                {
                    // 泉水出发
                    BuyItems(token);
                    UpgradeAbilities();
                    MoveToTurretMid(25 * 1000, token);

                    // 对线
                    while (true)
                    {
                        MoveToFight(3 * 1000, token);
                        Action = "Fighting";
                        for (int i = 0; i < 40; i++)
                        {
                            if (_isLowHealth && _position != Position.InBase)
                            {
                                ReturnToBase(token);
                                goto Start;
                            }

                            if (_isUnderAttack)
                            {
                                MoveToTurretMid(2 * 1000, token);
                            }

                            Attack();
                        }

                        MoveToTurretMid(3 * 1000, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Action = "Dead, Stop Playing";
            }
        }

        private void MoveToTurretMid(int duration, CancellationToken token)
        {
            Action = "Move To Turret Mid";
            Controller.RightClick(Controller.MiniMapUnderTurretMid);
            SleepWithCancellation(duration, token);
            Position = Position.UnderTurretMid;
        }

        private void MoveToCenterMid(int duration, CancellationToken token)
        {
            Action = "Move To Center Mid";
            Controller.RightClick(Controller.MiniMapCenterMid);
            SleepWithCancellation(duration, token);
            Position = Position.CenterMid;
        }

        private void MoveToOuterTurretMid(int duration, CancellationToken token)
        {
            Action = "Move To Outer Turret Mid";
            Controller.RightClick(Controller.MiniMapUnderOuterTurretMid);
            SleepWithCancellation(duration, token);
            Position = Position.UnderOuterTurretMid;
        }

        private void MoveToInnerTurretMid(int duration, CancellationToken token)
        {
            Action = "Move To Inner Turret Mid";
            Controller.RightClick(Controller.MiniMapUnderInnderTurretMid);
            SleepWithCancellation(duration, token);
            Position = Position.UnderInnerTurretMid;
        }

        private void MoveToInhibitorTurretMid(int duration, CancellationToken token)
        {
            Action = "Move To Inhibitor Turret Mid";
            Controller.RightClick(Controller.MiniMapUnderInhibitorTurretMid);
            SleepWithCancellation(duration, token);
            Position = Position.UnderInhibitorTurretMid;
        }

        private void MoveToNexusTurretMid(int duration, CancellationToken token)
        {
            Action = "Move To Nexus Turret Mid";
            Controller.RightClick(Controller.MiniMapEnemyNexus);
            SleepWithCancellation(duration, token);
            Position = Position.UnderNexusTurretMid;
        }

        private void MoveToNexus(int duration, CancellationToken token)
        {
            Action = "Move To Nexus";
            Controller.RightClick(Controller.MiniMapEnemyNexus);
            SleepWithCancellation(duration, token);
            Position = Position.UnderNexusTurretMid;
        }

        private void MoveToFight(int duration, CancellationToken token)
        {
            Action = "Move To Fight";
            var data = GameApi.GetGameEvents();
            if (data == null)
            {
                MoveToTurretMid(duration, token);
                return;
            }
            var events = data.ToList();
            events.Sort((a, b) => b["EventID"].ToObject<int>() - a["EventID"].ToObject<int>());
            foreach (var turret in from e in events
                     where e["EventName"]?.ToString() == "TurretKilled"
                     select e["TurretKilled"].ToString())
            {
                switch (turret)
                {
                    case Controller.NexusTurret1:
                    case Controller.NexusTurret2:
                        MoveToNexusTurretMid(duration, token);
                        return;
                    case Controller.MidInhibitorTurret:
                        MoveToInhibitorTurretMid(duration, token);
                        return;
                    case Controller.MidInnerTurret:
                        MoveToInnerTurretMid(duration, token);
                        return;
                    case Controller.MidOuterTurret:
                        MoveToOuterTurretMid(duration, token);
                        return;
                }
            }
            MoveToCenterMid(duration, token);
        }

        private void ReturnToBase(CancellationToken token)
        {
            Action = "Return To Base";
            Controller.RightClick(Controller.CenterOfScreen.Add(-500, 300));
            SleepWithCancellation(200, token);
            Keyboard.KeyPress(Keys.D, 100);
            Keyboard.KeyPress(Keys.W, 100);
            SleepWithCancellation(1300, token);
            Keyboard.KeyPress(Keys.F, 100);
            SleepWithCancellation(100, token);
            MoveToTurretMid(5 * 1000, token);
            Keyboard.KeyPress(Keys.B, 100);
            SleepWithCancellation(12 * 1000, token);
            Position = Position.InBase;
            SleepWithCancellation(3 * 1000, token); // recover time
        }

        /// <summary>
        /// 平A、用Q、W（耗时约500ms）
        /// </summary>
        private void Attack()
        {
            Keyboard.KeyPress(Keys.A, 50);
            Controller.LeftClick(Controller.CenterOfScreen.Add(20, -150));
            Thread.Sleep(100);
            Controller.LeftClick(Controller.CenterOfScreen.Add(380, -290));
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.E, 50);
            Thread.Sleep(100);
            Keyboard.KeyPress(Keys.Q, 50);
        }

        /// <summary>
        /// 血量检查
        /// </summary>
        private void HealthCheck()
        {
            while (true)
            {
                Thread.Sleep(1000);
                var currentHealth = GameApi.GetCurrentHealth();
                var healthDiffPerSec = _lastHealth - currentHealth;
                if (healthDiffPerSec > 40)
                {
                    IsUnderAttack = true;
                }
                else
                {
                    IsUnderAttack = false;
                }

                _lastHealth = currentHealth;

                var healthPercent = GameApi.GetHealthPercent();
                if (healthPercent < 35)
                {
                    IsLowHealth = true;
                }
                else
                {
                    IsLowHealth = false;
                }
            }
        }

        /// <summary>
        /// 在泉水动作：购买装备
        /// </summary>
        private void BuyItems(CancellationToken token)
        {
            Action = "Buy Items";
            Keyboard.KeyPress(Keys.P, 100);
            SleepWithCancellation(1000, token);
            foreach (var point in Controller.ShopItemButtons)
            {
                Controller.RightClick(point);
                SleepWithCancellation(200, token);
                Controller.RightClick(point);
                SleepWithCancellation(1000, token);
            }

            SleepWithCancellation(1000, token);
            Keyboard.KeyPress(Keys.P, 100);
        }

        /// <summary>
        /// 在泉水动作：升级技能
        /// </summary>
        private void UpgradeAbilities()
        {
            Action = "Upgrade Abilities";
            var abilities = GameApi.GetAbilities();
            if (abilities == null) return;
            var totalPoints = abilities.Q.AbilityLevel + abilities.W.AbilityLevel + abilities.E.AbilityLevel + abilities.R.AbilityLevel;
            var level = GameApi.GetLevel();
            var availablePoints = level - totalPoints;
            Keyboard.KeyDown(Keys.LControlKey);
            if (abilities.Q.AbilityLevel == 0)
            {
                Keyboard.KeyPress(Keys.Q, 100);
                availablePoints -= 1;
            }
            if (abilities.W.AbilityLevel == 0)
            {
                Keyboard.KeyPress(Keys.W, 100);
                availablePoints -= 1;
            }
            if (abilities.E.AbilityLevel == 0)
            {
                Keyboard.KeyPress(Keys.E, 100);
                availablePoints -= 1;
            }

            for (int i = 0; i < availablePoints; i++)
            {
                Keyboard.KeyPress(Keys.R, 100);
                Thread.Sleep(100);
                Keyboard.KeyPress(Keys.Q, 100);
                Thread.Sleep(100);
                Keyboard.KeyPress(Keys.W, 100);
                Thread.Sleep(100);
                Keyboard.KeyPress(Keys.E, 100);
                Thread.Sleep(100);
            }
            Keyboard.KeyUp(Keys.LControlKey);
        }

        private void StopPlay()
        {
            _ctsPlay?.Cancel();
            _playThread = null;
        }

        private static void SleepWithCancellation(int milliseconds, CancellationToken token)
        {
            const int interval = 100; // 每100毫秒检查一次取消请求
            int elapsed = 0;

            while (elapsed < milliseconds)
            {
                token.ThrowIfCancellationRequested();
                Thread.Sleep(interval);
                elapsed += interval;
            }
        }

        public void StopGame()
        {
            StopPlay();
            _healthThread?.Abort();
            _healthThread?.Join();
            _cts?.Cancel();
        }

        private void UpdatePlayerInfo()
        {
            _mainWindow.UpdatePlayerInfo(_isUnderAttack, _isLowHealth, _action, _isDead, _position.ToString(), _lastHealth.ToString());
        }
    }

    public enum Position
    {
        InBase,
        UnderTurretMid,
        CenterMid,
        UnderInnerTurretMid,
        UnderOuterTurretMid,
        UnderInhibitorTurretMid,
        UnderNexusTurretMid,
    }
}
