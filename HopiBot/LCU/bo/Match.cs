using System.Collections.Generic;

namespace HopiBot.LCU.bo
{
    public class Match
    {
        public long GameCreation { get; set; }
        public long GameDuration { get; set; }
        public long GameId { get; set; }
        public string GameMode { get; set; }
        public string GameType { get; set; }
        public int MapId { get; set; }
        public List<ParticipantIdentity> ParticipantIdentities { get; set; }
        public List<Participant> Participants { get; set; }
        public int QueueId { get; set; }
    }

    public class ParticipantIdentity
    {
        public int ParticipantId { get; set; }
        public Player Player { get; set; }
    }

    public class Player
    {
        public string Puuid { get; set; }
        public string SummonerName { get; set; }
    }

    public class Participant
    {
        public int ChampionId { get; set; }
        public int ParticipantId { get; set; }
        public Stats Stats { get; set; }
        public int TeamId { get; set; }
        public Timeline Timeline { get; set; }

        public double CalculateScore(long duration)
        {
            double normalizedKills = (double)Stats.Kills / duration * 10;
            double normalizedDeaths = (double)Stats.Deaths / duration * 10;
            double normalizedAssists = (double)Stats.Assists / duration * 10;
            double normalizedGoldEarned = ((double)Stats.GoldEarned / duration) / 1000 * 10;
            double normalizedDamageDealt = ((double)Stats.TotalDamageDealtToChampions / duration) / 1000 * 10;
            double normalizedDamageTaken = ((double)Stats.TotalDamageTaken / duration) / 1000 * 10;
            double normalizedMinionsKilled = (double)Stats.TotalMinionsKilled / duration;
            var level = Stats.ChampLevel;
            double winBonus = 0;

            double score = 0;

            if (Timeline.Lane == "TOP" || Timeline.Lane == "MIDDLE" || (Timeline.Lane == "BOTTOM" && Timeline.Role == "DUO"))
            {
                // 上单或中单
                if (normalizedDamageDealt > normalizedDamageTaken) // 输出型
                {
                    score = 3 * normalizedKills - 7 * normalizedDeaths + 0.7 * normalizedAssists + 1.0 * normalizedGoldEarned + 1.1 * normalizedDamageDealt + 4 * normalizedDamageTaken + 1.0 * normalizedMinionsKilled + winBonus + 0.004 * level;
                }
                else
                {
                    score = 3 * normalizedKills - 7 * normalizedDeaths + 0.7 * normalizedAssists + 1.0 * normalizedGoldEarned + 4 * normalizedDamageDealt + 1.1 * normalizedDamageTaken + 1.0 * normalizedMinionsKilled + winBonus + 0.004 * level;
                }
            }
            else if (Timeline.Lane == "JUNGLE")
            {
                // 打野
                if (normalizedDamageDealt > normalizedDamageTaken) // 输出型
                {
                    score = 3 * normalizedKills - 7 * normalizedDeaths + 0.6 * normalizedAssists + 1.1 * normalizedGoldEarned + 3.2 * normalizedDamageDealt + 1.1 * normalizedDamageTaken + 0.8 * normalizedMinionsKilled + winBonus + 0.004 * level;
                }
                else
                {
                    score = 3 * normalizedKills - 7 * normalizedDeaths + 0.6 * normalizedAssists + 1.1 * normalizedGoldEarned + 1.1 * normalizedDamageDealt + 3.2 * normalizedDamageTaken + 0.8 * normalizedMinionsKilled + winBonus + 0.004 * level;
                }
            }
            else
            {
                if (Timeline.Role == "CARRY")
                {
                    // ADC
                    score = 4 * normalizedKills - 8 * normalizedDeaths + 0.5 * normalizedAssists + 1.1 * normalizedGoldEarned + 4 * normalizedDamageDealt + 0.8 * normalizedMinionsKilled + winBonus + 0.003 * level;
                }
                else if (Timeline.Role == "SUPPORT" || Timeline.Role == "SOLO")
                {
                    // 辅助
                    if (normalizedDamageDealt > normalizedDamageTaken) // 输出型
                    {
                        score = 0.8 * normalizedKills - 7 * normalizedDeaths + 5 * normalizedAssists + 0.6 * normalizedGoldEarned + 3 * normalizedDamageDealt + 0.2 * normalizedDamageTaken + 0.3 * normalizedMinionsKilled + winBonus + 0.002 * level;
                    }
                    else
                    {
                        score = 0.8 * normalizedKills - 7 * normalizedDeaths + 6 * normalizedAssists + 0.6 * normalizedGoldEarned + 0.2 * normalizedDamageDealt + 3 * normalizedDamageTaken + 0.3 * normalizedMinionsKilled + winBonus + 0.002 * level;
                    }
                }
            }
            return score * 10;
        }
    }

    public class Stats
    {
        public int ChampLevel { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int GoldEarned { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TotalMinionsKilled { get; set; }
        public bool Win { get; set; }
    }

    public class Timeline
    {
        public string Lane { get; set; }
        public string Role { get; set; }
    }
}
