using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HopiBot.LCU.bo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HopiBot.LCU
{
    public static class ClientApi
    {
        private static Logger logger = new Logger("log.txt");
        public static async Task<bool> CreateBotLobby()
        {
            var gameConfig = new
            {
                customLobbyName = "Custom Lobby",
                gameMode = "CLASSIC",
                isCustom = "false",
                queueId = 890,
            };
            await LcuManager.Instance.PostClient("/lol-lobby/v2/lobby", gameConfig);
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
            await LcuManager.Instance.PostClient("/lol-lobby/v2/lobby", gameConfig);
            return true;
        }

        public static async Task<bool> SearchMath()
        {
            try
            {
                var response = await LcuManager.Instance.PostClient("/lol-lobby/v2/lobby/matchmaking/search", null);
            } catch (Exception e)
            {
                logger.Log("search match: ", e);
                return false;
            }
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
            var response = await LcuManager.Instance.GetClient("/lol-gameflow/v1/gameflow-phase");
            return JsonConvert.DeserializeObject<string>(response.Content);
        }

        public static async Task<bool> Accept()
        {
            try
            {
                var response = await LcuManager.Instance.PostClient("/lol-matchmaking/v1/ready-check/accept");
            } catch (Exception e)
            {
                logger.Log("accept: ", e);
                return false;
            }
            return true;
        }

        public static async Task<bool> PlayAgain()
        {
            try
            {
                var response = await LcuManager.Instance.PostClient("/lol-lobby/v2/play-again");
            } catch (Exception e)
            {
                logger.Log("play again: ", e);
                return false;
            }
            return true;
        }

        public static async Task<bool> ChampSelect(int champId)
        {
            var response = await LcuManager.Instance.GetClient("/lol-champ-select/v1/session");
            var session = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            var actions = (JArray)session["actions"];
            var localPlayerCellId = session["localPlayerCellId"].ToString();
            foreach (var a in actions[0])
            {
                var action = (JObject)a;
                foreach (var keyValuePair in action)
                {
                    logger.Log($"{keyValuePair.Value}");
                    if (keyValuePair.Key != "actorCellId") continue;
                    if (localPlayerCellId != keyValuePair.Value.ToString()) continue;
                    var body = new
                    {
                        completed = true,
                        type = "pick",
                        championId = champId
                    };
                    try
                    {
                        await LcuManager.Instance.PatchClient($"/lol-champ-select/v1/session/actions/{action["id"]}", body);
                    }
                    catch (Exception e)
                    {
                        logger.Log("champ select: ", e);
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        public static async Task<List<Champion>> GetAllChampions()
        {
            var response = await LcuManager.Instance.GetClient("/lol-game-data/assets/v1/champion-summary.json");
            return JsonConvert.DeserializeObject<List<Champion>>(response.Content);
        }
    }
}
