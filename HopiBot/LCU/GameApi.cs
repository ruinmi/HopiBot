using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HopiBot.LCU
{
    public class GameApi
    {
        public static async Task<string> GetGameEvent()
        {
            try
            {
                var response = await LcuManager.Instance.GetGameClient("/liveclientdata/eventdata");
                return response.Content;
            } catch
            {
                return null;
            }

        }
    }
}
