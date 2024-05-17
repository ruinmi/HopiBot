using System.Collections.Generic;

namespace HopiBot.LCU.bo
{
    public class Match
    {
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

        public double CalculateScore()
        {
            double score = 0;

            if (Timeline.Lane == "TOP" || Timeline.Lane == "MIDDLE")
            {
                // 上单或中单
                score = (Stats.Kills * 2) +
                        Stats.Assists -
                        Stats.Deaths +
                        (Stats.GoldEarned / 1000.0) +
                        (Stats.TotalDamageDealtToChampions / 1000.0) +
                        (Stats.TotalDamageTaken / 5000.0) +
                        (Stats.TotalMinionsKilled / 10.0);
            }
            else if (Timeline.Lane == "BOTTOM")
            {
                if (Timeline.Role == "CARRY")
                {
                    // ADC
                    score = (Stats.Kills * 2.5) +
                            Stats.Assists -
                            Stats.Deaths +
                            (Stats.GoldEarned / 1000.0) +
                            (Stats.TotalDamageDealtToChampions / 1000.0) +
                            (Stats.TotalMinionsKilled / 10.0);
                }
                else if (Timeline.Role == "SUPPORT")
                {
                    // 辅助
                    score = (Stats.Assists * 2) -
                            Stats.Deaths +
                            (Stats.TotalDamageDealtToChampions / 2000.0) +
                            (Stats.TotalDamageTaken / 4000.0);
                }
            }
            else if (Timeline.Lane == "JUNGLE")
            {
                // 打野
                score = (Stats.Kills * 2) +
                        Stats.Assists -
                        Stats.Deaths +
                        (Stats.GoldEarned / 1000.0) +
                        (Stats.TotalDamageDealtToChampions / 1000.0) +
                        (Stats.TotalMinionsKilled / 15.0);
            }

            return score;
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
