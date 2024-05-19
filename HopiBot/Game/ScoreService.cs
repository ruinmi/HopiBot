using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HopiBot.LCU;

namespace HopiBot.Game
{
    public class ScoreService
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
                { "ally", FormatScores(CalculateScore(ally)) },
                { "enemy", FormatScores(CalculateScore(enemy)) }
            };
            return result;
        }

        /// <summary>
        /// 得到一个队伍每个玩家的平均分数
        /// </summary>
        /// <param name="teamPuuid">队伍所有人的puuid</param>
        /// <returns></returns>
        public static List<Tuple<string, double>> CalculateScore(List<string> teamPuuid)
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
            var matches = ClientApi.GetMatchesByPuuid(puuid);
            // 计算平均分数（去掉最高、去掉最低）
            var scores = new List<double>();
            foreach (var match in matches)
            {
                if (match.GameMode != "CLASSIC" || match.GameType != "MATCHED_GAME") continue;
                var m = ClientApi.GetMatch(match.GameId);
                var participantId = 0;
                foreach (var mParticipantIdentity in m.ParticipantIdentities)
                {
                    if (puuid == mParticipantIdentity.Player.Puuid)
                    {
                        participantId = mParticipantIdentity.ParticipantId;
                    }
                }

                foreach (var participant in m.Participants)
                {
                    if (participantId == participant.ParticipantId)
                    {
                        var score = participant.CalculateScore(match.GameDuration);
                        scores.Add(score);
                    }
                }
            }

            scores.Sort();
            var avg = 0.0;
            if (scores.Count > 2)
            {
                scores.RemoveAt(0);
                scores.RemoveAt(scores.Count - 1);
                avg = scores.Average();
            }

            return Tuple.Create(name, avg);
        }

        private static List<Tuple<string, double>> FormatScores(List<Tuple<string, double>> scores)
        {
            return scores.Select(score => Tuple.Create(score.Item1, Math.Round(score.Item2, 1))).ToList();
        }
    }
}
