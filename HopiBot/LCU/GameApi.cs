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
                var myData = JObject.Parse(LcuManager.Instance.GetGameClient("/liveclientdata/activeplayer").Content);
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


    }
}
