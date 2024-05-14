using System.Collections.Generic;
using System.Threading.Tasks;
using HopiBot.LCU.bo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HopiBot.LCU
{
    public static class ClientApi
    {
        public static async Task<bool> CreateBotLobby()
        {
            var gameConfig = new
            {
                customLobbyName = "Custom Lobby",
                gameMode = "CLASSIC",
                isCustom = "false",
                queueId = 890,
            };
            await LcuManager.Instance.PostAsync("/lol-lobby/v2/lobby", gameConfig);
            return true;
        }

        public static async Task<bool> CreatePracticeLobby()
        {
            var gameConfig = new
            {
                customGameLobby =
                    new
                    {
                        configuration =
                            new
                            {
                                gameMode = "PRACTICETOOL",
                                gameMutator = "",
                                gameServerRegion = "",
                                mapId = 11,
                                mutators = new { id = 1 },
                                spectatorPolicy = "AllAllowed",
                                teamSize = 5
                            },
                        lobbyName = "LeagueLobby 5V5 PRACTICETOOL",
                        lobbyPassword = "null"
                    },
                isCustom = true
            };
            await LcuManager.Instance.PostAsync("/lol-lobby/v2/lobby", gameConfig);
            return true;
        }

        public static async Task<bool> SearchMath()
        {
            await LcuManager.Instance.PostAsync("/lol-lobby/v2/lobby/matchmaking/search");
            return true;
        }

        /// <summary>
        /// 查看游戏当前状态
        /// 游戏大厅:None
        /// 房间内:Lobby
        /// 匹配中:Matchmaking
        /// 找到对局:ReadyCheck
        /// 选英雄中:ChampSelect
        /// 游戏中:InProgress
        /// 游戏即将结束:PreEndOfGame
        /// 等待结算页面:WaitingForStats
        /// 游戏结束:EndOfGame
        /// 等待重新连接:Reconnect
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetGameStatus()
        {
            var response = await LcuManager.Instance.GetAsync("/lol-gameflow/v1/gameflow-phase");
            return JsonConvert.DeserializeObject<string>(response.Content);
        }

        public static async Task<bool> Accept()
        {
            var response = await LcuManager.Instance.PostAsync("/lol-matchmaking/v1/ready-check/accept");
            return true;
        }

        public static async Task<bool> PlayAgain()
        {
            var response = await LcuManager.Instance.PostAsync("/lol-lobby/v2/play-again");
            return true;
        }

        public static async Task<bool> ChampSelect(int champId)
        {
            var response = await LcuManager.Instance.GetAsync("/lol-champ-select/v1/session");
            var session = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            var actions = (JArray)session["actions"];
            var localPlayerCellId = session["localPlayerCellId"].ToString();
            foreach (var a in actions)
            {
                var action = (JObject)a[0];
                foreach (var keyValuePair in action)
                {
                    if (keyValuePair.Key != "actorCellId") continue;
                    if (localPlayerCellId != keyValuePair.Value.ToString()) continue;
                    var body = new
                    {
                        completed = true,
                        type = "pick",
                        championId = champId
                    };
                    await LcuManager.Instance.PatchAsync($"/lol-champ-select/v1/session/actions/{action["id"]}", body);
                    return true;
                }
            }

            return false;
        }

        public static async Task<List<Champion>> GetAllChampions()
        {
            var response = await LcuManager.Instance.GetAsync("/lol-game-data/assets/v1/champion-summary.json");
            return JsonConvert.DeserializeObject<List<Champion>>(response.Content);
        }
    }
}
