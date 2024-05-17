using System.Threading;
using System.Windows;
using HopiBot.LCU;
using HopiBot.LCU.bo;
using HopiBot.Enum;

namespace HopiBot.Game
{
    public class Client
    {
        public MainWindow MainWindow;
        public Champion Champ;
        private Game _game;

        private bool _isAccepted;
        private bool _isChampSelected;

        public Client(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public void Start()
        {
            var phase = ClientApi.GetGamePhase();
            if (phase == GamePhase.None) ClientApi.CreateBotLobby();

            GamePhaseListening();
        }

        private void GamePhaseListening()
        {
            while (true)
            {
                var phase = ClientApi.GetGamePhase();
                switch (phase)
                {
                    case GamePhase.None:
                        break;
                    case GamePhase.Lobby:
                        ClientApi.SearchMath();
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
                        _game = new Game();
                        _game.Start();
                        break;
                    case GamePhase.WaitingForStats:
                        break;
                    case GamePhase.EndOfGame:
                        Application.Current.Dispatcher.Invoke(() =>
                            MainWindow.CurrRoundBlk.Text = (int.Parse(MainWindow.CurrRoundBlk.Text) + 1).ToString());
                        ClientApi.HonorPlayer();
                        Thread.Sleep(2000);
                        var isPlayAgain = ClientApi.PlayAgain();
                        if (!isPlayAgain) return;
                        Thread.Sleep(2000);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Abort()
        {
            _game?.AbortPlay();
        }
    }
}
