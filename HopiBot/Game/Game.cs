using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HopiBot.Enum;
using HopiBot.LCU;
using InputManager;

namespace HopiBot.Game
{
    public class Game
    {
        private MainWindow _mainWindow;

        #region Game State

        private CancellationTokenSource _ctsPlay;
        private bool _isStopped;
        private Task _healthTask;
        private Task _playTask;

        #endregion

        #region Player State

        private bool _isDead;
        private string _action;
        private bool _isUnderAttack;
        private bool _isLowHealth;
        private int _lastHealth;
        private Position _position;

        private bool IsDead
        {
            get => _isDead;
            set => SetProperty(ref _isDead, value);
        }

        private string Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        private bool IsUnderAttack
        {
            get => _isUnderAttack;
            set => SetProperty(ref _isUnderAttack, value);
        }

        private bool IsLowHealth
        {
            get => _isLowHealth;
            set => SetProperty(ref _isLowHealth, value);
        }

        private int LastHealth
        {
            get => _lastHealth;
            set => SetProperty(ref _lastHealth, value);
        }

        private Position Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        #endregion

        public Game(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Position = Position.InBase;
        }

        public async Task Start()
        {
            _healthTask = Task.Run(HealthCheck);


            // 游戏20s后开始
            var isLocked = false;
            while (!isLocked)
            {
                await Task.Delay(2000);
                var gameTime = GameApi.GetGameTime();
                if (gameTime < 20) continue;
                Controller.LockScreen();
                isLocked = true;
            }

            // 游戏开始，监听游戏是否结束、是否死亡
            while (true)
            {
                if (_isStopped) break;

                await Task.Delay(2000);
                var phase = ClientApi.GetGamePhase();
                if (phase != GamePhase.InProgress)
                {
                    Logger.Log("Game End");
                    StopGame();
                    break;
                }

                IsDead = _lastHealth <= 0;
                if (IsDead)
                {
                    StopPlay();
                }
                else if (_playTask == null && !_isStopped)
                {
                    _ctsPlay = new CancellationTokenSource();
                    _playTask = Task.Run(() => Play(_ctsPlay.Token), _ctsPlay.Token);
                }
            }
        }


        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="token"></param>
        private async Task Play(CancellationToken token)
        {
            try
            {
                Start:
                // 泉水出发
                await BuyItems(token);
                UpgradeAbilities();
                await MoveToTurretMid(25 * 1000, token);

                // 对线
                while (true)
                {
                    await MoveToFight(3 * 1000, token);
                    Action = "Fighting";
                    for (int i = 0; i < 40; i++)
                    {
                        if (_isLowHealth && _position != Position.InBase)
                        {
                            await ReturnToBase(token);
                            goto Start;
                        }

                        if (_isUnderAttack)
                        {
                            await MoveToTurretMid(2 * 1000, token);
                        }

                        await Attack(token);
                    }

                    await MoveToTurretMid(3 * 1000, token);
                }
            }
            catch (OperationCanceledException)
            {
                Action = "Dead, Stop Playing";
            }
        }

        private async Task MoveToPosition(int duration, CancellationToken token, string action, RatioPoint targetPosition, Position newPosition)
        {
            Action = action;
            Controller.RightClick(targetPosition);
            await SleepWithCancellation(duration, token);
            Position = newPosition;
        }

        private async Task MoveToTurretMid(int duration, CancellationToken token)
        {
            await MoveToPosition(duration, token, "Move To Turret Mid", Controller.MiniMapUnderTurretMid, Position.UnderTurretMid);
        }

        private async Task MoveToFight(int duration, CancellationToken token)
        {
            Action = "Move To Fight";
            var data = GameApi.GetGameEvents();
            if (data == null)
            {
                await MoveToTurretMid(duration, token);
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
                        await MoveToPosition(duration, token, "Move To Nexus Turret Mid", Controller.MiniMapEnemyNexus, Position.UnderNexusTurretMid);
                        return;
                    case Controller.MidInhibitorTurret:
                        await MoveToPosition(duration, token, "Move To Inhibitor Turret Mid", Controller.MiniMapUnderInhibitorTurretMid, Position.UnderInhibitorTurretMid);
                        return;
                    case Controller.MidInnerTurret:
                        await MoveToPosition(duration, token, "Move To Inner Turret Mid", Controller.MiniMapUnderInnderTurretMid, Position.UnderInnerTurretMid);
                        return;
                    case Controller.MidOuterTurret:
                        await MoveToPosition(duration, token, "Move To Outer Turret Mid", Controller.MiniMapUnderOuterTurretMid, Position.UnderOuterTurretMid);
                        return;
                }
            }
            await MoveToPosition(duration, token, "Move To Center Mid", Controller.MiniMapCenterMid, Position.CenterMid);
        }

        private async Task ReturnToBase(CancellationToken token)
        {
            Action = "Return To Base";
            Controller.RightClick(Controller.CenterOfScreen.Add(-500, 300));
            await SleepWithCancellation(200, token);
            Keyboard.KeyPress(Keys.D, 100);
            Keyboard.KeyPress(Keys.W, 100);
            await SleepWithCancellation(1300, token);
            Keyboard.KeyPress(Keys.F, 100);
            await SleepWithCancellation(100, token);
            await MoveToTurretMid(5 * 1000, token);
            Keyboard.KeyPress(Keys.B, 100);
            await SleepWithCancellation(12 * 1000, token);
            Position = Position.InBase;
            await SleepWithCancellation(3 * 1000, token); // recover time
        }

        /// <summary>
        /// 平A、用Q、W（耗时约500ms）
        /// </summary>
        private async Task Attack(CancellationToken token)
        {
            Keyboard.KeyPress(Keys.A, 50);
            Controller.LeftClick(Controller.CenterOfScreen.Add(20, -150));
            await SleepWithCancellation(100, token);
            Controller.LeftClick(Controller.CenterOfScreen.Add(380, -290));
            await SleepWithCancellation(100, token);
            Keyboard.KeyPress(Keys.E, 50);
            await SleepWithCancellation(100, token);
            Keyboard.KeyPress(Keys.Q, 50);
        }

        /// <summary>
        /// 血量检查
        /// </summary>
        private async Task HealthCheck()
        {
            while (true)
            {
                await Task.Delay(1000);
                var currentHealth = GameApi.GetCurrentHealth();
                var healthDiffPerSec = _lastHealth - currentHealth;
                IsUnderAttack = healthDiffPerSec > 40;
                LastHealth = currentHealth;

                var healthPercent = GameApi.GetHealthPercent();
                IsLowHealth = healthPercent < 35;
            }
        }

        /// <summary>
        /// 在泉水动作：购买装备
        /// </summary>
        private async Task BuyItems(CancellationToken token)
        {
            Action = "Buy Items";
            Keyboard.KeyPress(Keys.P, 100);
            await SleepWithCancellation(1000, token);
            foreach (var point in Controller.ShopItemButtons)
            {
                Controller.RightClick(point);
                await SleepWithCancellation(200, token);
                Controller.RightClick(point);
                await SleepWithCancellation(1000, token);
            }
            await SleepWithCancellation(1000, token);
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
            _playTask = null;
        }

        private static async Task SleepWithCancellation(int milliseconds, CancellationToken token)
        {
            const int interval = 100; // 每100毫秒检查一次取消请求
            int elapsed = 0;

            while (elapsed < milliseconds)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(interval, token);
                elapsed += interval;
            }
        }

        public void StopGame()
        {
            StopPlay();
            _isStopped = true;
        }

        private void UpdatePlayerInfo()
        {
            _mainWindow.UpdatePlayerInfo(_isUnderAttack, _isLowHealth, _action, _isDead, _position.ToString(), _lastHealth.ToString());
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (!Equals(field, value))
            {
                field = value;
                UpdatePlayerInfo();
            }
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
