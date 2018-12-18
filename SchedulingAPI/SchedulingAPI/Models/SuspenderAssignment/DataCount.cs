using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingAPI.Models.SuspenderAssignment
{
    public class DataCount
    {
        public int MatchCount { get; set; }

        public int SuspenderCount { get; set; }

        public int BettypeCount { get; set; }

        public int Total => SuspenderCount * MatchCount + SuspenderCount * BettypeCount + MatchCount + BettypeCount;
    }
}
