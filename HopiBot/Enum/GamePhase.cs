namespace HopiBot.Enum
{
    public enum GamePhase
    {
        None,
        Lobby,
        Matchmaking,
        ReadyCheck,
        ChampSelect,
        InProgress,
        WaitingForStats,
        PreEndOfGame,
        EndOfGame,
        Reconnect,
    }

    public static class GamePhaseExtensions
    {
        public static string ToChinese(this GamePhase value)
        {
            switch (value)
            {
                case GamePhase.None:
                    return "...";
                case GamePhase.Lobby:
                    return "房间";
                case GamePhase.Matchmaking:
                    return "匹配中";
                case GamePhase.ReadyCheck:
                    return "找到对局";
                case GamePhase.ChampSelect:
                    return "选英雄中";
                case GamePhase.InProgress:
                    return "游戏中";
                case GamePhase.WaitingForStats:
                    return "等待结算页面";
                case GamePhase.PreEndOfGame:
                    return "游戏即将结束";
                case GamePhase.EndOfGame:
                    return "游戏结束";
                case GamePhase.Reconnect:
                    return "等待重新连接";
                default:
                    return "";
            }
        }

        public static string ToEnglish(this GamePhase value)
        {
            switch (value)
            {
                case GamePhase.None:
                    return "None";
                case GamePhase.Lobby:
                    return "Lobby";
                case GamePhase.Matchmaking:
                    return "Matchmaking";
                case GamePhase.ReadyCheck:
                    return "Ready Check";
                case GamePhase.ChampSelect:
                    return "Champ Select";
                case GamePhase.InProgress:
                    return "In Progress";
                case GamePhase.WaitingForStats:
                    return "Waiting For Stats";
                case GamePhase.PreEndOfGame:
                    return "Pre End Of Game";
                case GamePhase.EndOfGame:
                    return "End Of Game";
                case GamePhase.Reconnect:
                    return "Reconnect";
                default:
                    return "";
            }
        }
    }
}
