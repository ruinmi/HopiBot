using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HopiBot.LCU;

namespace HopiBot.Game
{
    public static class ScoreService
    {
        /// <summary>
        /// 得到两个队伍所有玩家的平均分数
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<Tuple<string, double>>> GetScores()
        {
            var myPuuid = ClientApi.GetMyPuuid();
            var teamPuuid = ClientApi.GetTeamPuuid();
            var (ally, enemy) = teamPuuid["teamOne"].Contains(myPuuid)
                ? (teamPuuid["teamOne"], teamPuuid["teamTwo"])
                : (teamPuuid["teamTwo"], teamPuuid["teamOne"]);

            var result = new Dictionary<string, List<Tuple<string, double>>>
            {
                { "ally", FormatScores(CalculateTeamScore(ally)) },
                { "enemy", FormatScores(CalculateTeamScore(enemy)) }
            };
            return result;
        }

        /// <summary>
        /// 得到一个队伍每个玩家的平均分数
        /// </summary>
        /// <param name="teamPuuid">队伍所有人的puuid</param>
        /// <returns></returns>
        public static List<Tuple<string, double>> CalculateTeamScore(List<string> teamPuuid)
        {
            var list = teamPuuid.Select(CalculateAvgScore).ToList();
            list.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return list;
        }

        /// <summary>
        /// 计算一个玩家的平均分数
        /// </summary>
        /// <param name="puuid">该玩家的puuid</param>
        /// <returns></returns>
        public static Tuple<string, double> CalculateAvgScore(string puuid)
        {
            var name = ClientApi.GetSummoner(puuid).DisplayName;
            var matches = ClientApi.GetMatchesByPuuid(puuid).FindAll(m => m.GameDuration > 900); // 大于15min的对局
            Logger.Log($"计算玩家分数{name} 共{matches.Count}场比赛");
            // 计算平均分数
            var scores = new List<double>();
            foreach (var match in matches)
            {
                // 获取比赛完整数据
                if (!match.Participants.Any()) continue;
                var score = match.Participants[0].CalculateScore(match.MapId, match.GameDuration);
                Logger.Log($"比赛: {match.GameMode} {DateTimeOffset.FromUnixTimeMilliseconds(match.GameCreation).ToLocalTime()} 分数: {score}");
                scores.Add(score);
            }

            scores.Sort();
            var avg = scores.Average();

            return Tuple.Create(name, avg);
        }

        private static List<Tuple<string, double>> FormatScores(List<Tuple<string, double>> scores)
        {
            return scores.Select(score => Tuple.Create(score.Item1, Math.Round(score.Item2, 1))).ToList();
        }
    }
}
