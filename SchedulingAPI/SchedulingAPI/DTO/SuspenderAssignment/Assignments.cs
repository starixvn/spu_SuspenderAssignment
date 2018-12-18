using System.Collections.Generic;

namespace SchedulingAPI.DTO.SuspenderAssignment
{
    public class Assignments
    {
        public Queue<int> MatchQueue { get; set; }

        public Queue<Bettype> BettypeQueue { get; set; }

        public IDictionary<int, HashSet<int>> AssignedSuspenderMatch { get; set; }

        public IDictionary<int, HashSet<Bettype>> AssignedSuspenderBettype { get; set; }

        public Match CurrentMatch { get; set; }

        public Bettype CurrentBettype { get; set; }

        public Assignments Clone()
        {
            return new Assignments
            {
                MatchQueue = new Queue<int>(MatchQueue),
                CurrentMatch = CurrentMatch,
                CurrentBettype = CurrentBettype,
                BettypeQueue = new Queue<Bettype>(BettypeQueue),
                AssignedSuspenderMatch = new Dictionary<int, HashSet<int>>(AssignedSuspenderMatch),
                AssignedSuspenderBettype = new Dictionary<int, HashSet<Bettype>>(AssignedSuspenderBettype)
            };
        }
    }
}