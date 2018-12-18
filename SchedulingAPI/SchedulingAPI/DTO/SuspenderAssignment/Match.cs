using System;
using System.Collections.Generic;

namespace SchedulingAPI.DTO.SuspenderAssignment
{
    public class Match
    {
        public HashSet<Bettype> PickedBettypes { get; set; }

        public bool HasTV { get; set; }

        public double MaxBet { get; set; }

        public DateTime KOTime { get; set; }

        public bool HasReporter { get; set; }

        public bool RequireChinese { get; set; }

        public Provider Provider { get; set; }

        public int MatchId { get; set; }
    }
}