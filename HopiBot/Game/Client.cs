using System.Threading;
using System.Windows;
using HopiBot.LCU;
using HopiBot.LCU.bo;
using HopiBot.Enum;

namespace HopiBot.Game
{
    public class Client
    {
        private MainWindow _mainWindow;
        public Champion Champ;
        private Game _game;
        private bool _isStopped;

        private bool _isAccepted;
        private bool _isChampSelected;

        public int _roundCount;
        public int _roundLimit;

        public Client(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Start()
        {
            var phase = ClientApi.GetGamePhase();
            if (phase == GamePhase.None) ClientApi.CreateBotLobby();

            _roundCount = 0;
            Application.Current.Dispatcher.Invoke(() => {
                if (!int.TryParse(_mainWindow.roundLimit.Text, out _roundLimit))
                {
                    _roundLimit = 1;
                }
            });
            GamePhaseListening();
        }

        private async void GamePhaseListening()
        {
            while (true)
            {
                if (_isStopped || _roundCount >= _roundLimit) {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.Stop();
                    });
                    return; 
                }
                var phase = ClientApi.GetGamePhase();
                switch (phase)
                {
                    case GamePhase.None:
                        break;
                    case GamePhase.Lobby:
                        ClientApi.SearchMath();
                        _isAccepted = false;
                        break;
                    case GamePhase.Matchmaking:
                        _isAccepted = false;
                        break;
                    case GamePhase.ReadyCheck:
                        if (_isAccepted) break;
                        ClientApi.Accept();
                        _isAccepted = true;
                        _isChampSelected = false;
                        break;
                    case GamePhase.ChampSelect:
                        if (_isChampSelected) break;
                        ClientApi.ChampSelect(Champ);
                        _isChampSelected = true;
                        break;
                    case GamePhase.InProgress:
                        Logger.Log("===============================Start of Game===============================");
                        _game = new Game(_mainWindow);
                        await _game.Start();
                        break;
                    case GamePhase.PreEndOfGame:
                        Thread.Sleep(3000);
                        for (int i = 0; i < 12; i++)
                        {
                            Controller.LeftClickClient(800, 300 + i * 50);
                        }
                        Thread.Sleep(1000);
                        Controller.LeftClickClient(638, 680);
                        break;
                    case GamePhase.WaitingForStats:
                        break;
                    case GamePhase.EndOfGame:
                        Application.Current.Dispatcher.Invoke(() =>
                            _mainWindow.CurrRoundBlk.Text = (int.Parse(_mainWindow.CurrRoundBlk.Text) + 1).ToString());
                        Thread.Sleep(2000);
                        var isPlayAgain = ClientApi.PlayAgain();
                        if (!isPlayAgain) return;
                        _roundCount += 1;
                        Thread.Sleep(2000);
                        Logger.Log("===============================End of Game===============================");
                        break;
                }
                Thread.Sleep(500);
            }
        }

        public void Shutdown()
        {
            _isStopped = true;
            _game?.StopGame();
        }
    }
}
