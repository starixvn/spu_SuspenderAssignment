using System.Collections.Generic;

namespace SchedulingAPI.DTO.SuspenderAssignment
{
    public class DataHolder
    {
        public IEnumerable<Match> Matches { get; set; }

        public IEnumerable<Suspender> Suspenders { get; set; }
    }
}