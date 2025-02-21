using System.Collections.Generic;
using System.Linq;
using HopiBot.LCU.bo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HopiBot.LCU
{
    public class GameApi
    {
        public static JToken GetGameEvents()
        {
            try
            {
                var response = LcuManager.Instance.GetGameClient("/liveclientdata/eventdata?eventID=0");
                var events = JObject.Parse(response.Content);
                return events["Events"];
            } catch
            {
                return null;
            }
        }

        public static int GetGameTime()
        {
            try
            {
                var response = LcuManager.Instance.GetGameClient("/liveclientdata/allgamedata");
                var allGameData = JObject.Parse(response.Content);
                var gameTime = allGameData["gameData"]["gameTime"].ToObject<int>();
                return gameTime;
            }
            catch
            {
                return -1;
            }
        }

        public static bool IsDead()
        {
            try
            {
                var myData = GetMyData();
                var myId = myData["riotId"];
                var players = JToken.Parse(LcuManager.Instance.GetGameClient("/liveclientdata/playerlist?teamID=100").Content);
                foreach (var player in players)
                {
                    if (myId.ToString() == player["riotId"].ToString())
                    {
                        return (bool)player["isDead"];
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static int GetCurrentHealth()
        {
            try
            {
                var myData = GetMyData();
                if (myData == null) return -1;
                var s = myData.GetValue("championStats");
                if (s == null) return -1;
                var currentHealth = myData["championStats"]["currentHealth"].ToObject<int>();
                return currentHealth;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the percentage of the current health of the player
        /// </summary>
        /// <returns>0-100</returns>
        public static int GetHealthPercent()
        {
            try
            {
                var myData = GetMyData();
                var currentHealth = myData["championStats"]["currentHealth"].ToObject<int>();
                var maxHealth = myData["championStats"]["maxHealth"].ToObject<int>();
                return currentHealth * 100 / maxHealth;
            }
            catch
            {
                return -1;
            }
        }

        public static int GetLevel()
        {
            try
            {
                var myData = GetMyData();
                var level = myData["level"].ToObject<int>();
                return level;
            }
            catch
            {
                return -1;
            }
        }

        public static Abilities GetAbilities()
        {
            try
            {
                var myData = GetMyData();
                var abilities = myData["abilities"].ToString();
                return JsonConvert.DeserializeObject<Abilities>(abilities);
            }
            catch
            {
                return null;
            }
        }

        public static List<string> GetEnemyChampions()
        {
            try
            {
                var myData = GetMyData();
                var myRiotId = myData["riotId"];
                var orderPlayers = JToken.Parse(LcuManager.Instance.GetGameClient("/liveclientdata/playerlist?teamID=ORDER").Content);
                var chaosPlayers = JToken.Parse(LcuManager.Instance.GetGameClient("/liveclientdata/playerlist?teamID=CHAOS").Content);

                var enemyPlayers = orderPlayers.All(orderPlayer => orderPlayer["riotId"].ToString() != myRiotId.ToString()) ? orderPlayers : chaosPlayers;

                return enemyPlayers.Select(player => player["rawChampionName"].ToString().Replace("game_character_displayname_", "")).Where(name => !name.Contains("Dummy")).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static JObject GetMyData()
        {
            try
            {
                var myData = JObject.Parse(LcuManager.Instance.GetGameClient("/liveclientdata/activeplayer").Content);

                return myData;
            }
            catch
            {
                return null;
            }
        }

    }
}
