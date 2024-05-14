using System.Threading;
using System.Threading.Tasks;
using HopiBot.LCU;

namespace HopiBot
{
    public partial class MainWindow
    {
        public static Timer AcceptTimer;
        public static Timer ChampSelectTimer;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            _ = Task.Run(async () =>
            {
                if (await ClientApi.CreateBotLobby() && await ClientApi.SearchMath())
                {
                    AcceptTimer.Change(0, 1000);
                    ChampSelectTimer.Change(0, 1000);
                }
            });
        }

        private static void Init()
        {
            LcuManager.Load();
            AcceptTimer = new Timer(AutoAcceptMatch, null, Timeout.Infinite, Timeout.Infinite);
            ChampSelectTimer = new Timer(AutoChampSelect, null, Timeout.Infinite, Timeout.Infinite);
        }

        private static async void AutoAcceptMatch(object state)
        {
            if (await ClientApi.GetGameStatus() == "ReadyCheck")
            {
                await ClientApi.Accept();
                AcceptTimer.Change(Timeout.Infinite, 0);
            }
        }

        private static async void AutoChampSelect(object state)
        {
            if (await ClientApi.GetGameStatus() == "ChampSelect")
            {
                await ClientApi.ChampSelect(22);
                ChampSelectTimer.Change(Timeout.Infinite, 0);
            }
        }
    }
}
